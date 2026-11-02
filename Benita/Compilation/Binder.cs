namespace Benita;

/// <summary>نتیجهٔ phase اتصال declarationها و referenceهای AST به symbolها.</summary>
public sealed class BindingResult
{
    internal BindingResult(
        ProgramNode program,
        IReadOnlyDictionary<AstNode, Symbol> references,
        IReadOnlyDictionary<AstNode, Symbol> declarations,
        IReadOnlyList<CompilationDiagnostic> diagnostics)
    {
        Program = program;
        References = references;
        Declarations = declarations;
        Diagnostics = diagnostics;
    }

    public ProgramNode Program { get; }
    public IReadOnlyDictionary<AstNode, Symbol> References { get; }
    public IReadOnlyDictionary<AstNode, Symbol> Declarations { get; }
    public IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
    public bool IsSuccess => Diagnostics.All(diagnostic =>
        diagnostic.Severity != CompilationDiagnosticSeverity.Error);

    public bool TryGetSymbol(AstNode reference, out Symbol? symbol) =>
        References.TryGetValue(reference, out symbol);

    public bool TryGetDeclarationSymbol(AstNode declaration, out Symbol? symbol) =>
        Declarations.TryGetValue(declaration, out symbol);
}

/// <summary>
/// نام‌ها را از روی AST به symbolهای semantic متصل می‌کند.
/// این phase عمداً type checking و اجرای برنامه را انجام نمی‌دهد.
/// </summary>
public sealed class Binder
{
    private readonly Scope _globalScope = new(name: "global");
    private readonly Scope _functionScope = new(name: "functions");
    private readonly Scope _typeScope = new(name: "types");
    private readonly Dictionary<AstNode, Symbol> _references =
        new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AstNode, Symbol> _declarations =
        new(ReferenceEqualityComparer.Instance);
    private readonly List<CompilationDiagnostic> _diagnostics = [];
    private Scope _currentScope;

    public Binder()
    {
        _currentScope = _globalScope;
    }

    public BindingResult Bind(ProgramNode program)
    {
        ArgumentNullException.ThrowIfNull(program);
        Reset();

        DeclareTypes(program);
        DeclareGlobalVariables(program);
        DeclareFunctions(program);
        DeclarePackages(program);

        foreach (VariableDeclarationNode declaration in program.GlobalVariables)
            BindInitializer(declaration.Initializer);
        foreach (FunctionNode function in program.Functions)
            BindFunction(function);
        if (program.MainFunction is not null)
            BindFunction(program.MainFunction);
        foreach (PackageNode package in program.Packages)
            BindPackage(package);
        if (program.Statements is not null)
        {
            Scope topLevel = new(_globalScope, "top-level");
            WithScope(topLevel, () =>
            {
                foreach (StatementNode? statement in program.Statements)
                    if (statement is not null) BindStatement(statement);
            });
        }

        return new BindingResult(program, _references, _declarations, _diagnostics);
    }

    private void Reset()
    {
        _globalScope.Clear();
        _functionScope.Clear();
        _typeScope.Clear();
        _references.Clear();
        _declarations.Clear();
        _diagnostics.Clear();
        _currentScope = _globalScope;
    }

    private void DeclareTypes(ProgramNode program)
    {
        foreach (InterfaceNode item in program.Interfaces)
            Declare(_typeScope, new TypeDeclarationSymbol(item.Name, SymbolKind.Interface,
                new NamedTypeSymbol(item.Name), item), item);
        foreach (PackageNode item in program.Packages)
            Declare(_typeScope, new TypeDeclarationSymbol(item.Name, SymbolKind.Package,
                new NamedTypeSymbol(item.Name), item), item);
    }

    private void DeclareGlobalVariables(ProgramNode program)
    {
        foreach (VariableDeclarationNode item in program.GlobalVariables)
            DeclareVariable(_globalScope, item.Name, item.Type, item);
    }

    private void DeclareFunctions(ProgramNode program)
    {
        foreach (FunctionNode item in program.Functions.Append(program.MainFunction)
                     .Where(function => function is not null))
        {
            FunctionNode function = item!;
            FunctionSymbol symbol = new(function.Name, TypeFacts.FromName(function.ReturnType),
                function.Parameters.Select(parameter => TypeFacts.FromName(parameter.Type)).ToArray(), function);
            Declare(_functionScope, symbol, function);
        }
    }

    private void DeclarePackages(ProgramNode program)
    {
        // Package members have visibility, instance and initialization rules that are
        // owned by SemanticAnalyzer. They are intentionally not promoted to global
        // bindings in this phase.
    }

    private void BindFunction(FunctionNode function)
    {
        Scope functionScope = new(_globalScope, $"function:{function.Name}");
        WithScope(functionScope, () =>
        {
            foreach (ParameterNode parameter in function.Parameters)
                Declare(functionScope, new ParameterSymbol(parameter.Name, TypeFacts.FromName(parameter.Type)), parameter);
            BindBlock(function.Body, createScope: false);
            if (function.ReturnStatement is not null)
                BindExpression(function.ReturnStatement.ReturnExpression);
        });
    }

    private void BindPackage(PackageNode package)
    {
        // Deferred until Binder has a package-local symbol table with visibility and
        // inferred-field semantics matching SemanticAnalyzer.
    }

    private void BindBlock(BlockNode block, bool createScope = true)
    {
        if (createScope)
        {
            WithScope(new Scope(_currentScope, "block"), () =>
            {
                foreach (StatementNode statement in block.Statements) BindStatement(statement);
            });
            return;
        }
        foreach (StatementNode statement in block.Statements) BindStatement(statement);
    }

    private void BindStatement(StatementNode statement)
    {
        switch (statement)
        {
            case VariableDeclarationNode variable:
                DeclareVariable(_currentScope, variable.Name, variable.Type, variable);
                BindInitializer(variable.Initializer);
                break;
            case BlockNode block: BindBlock(block); break;
            case ExpressionStatementNode expression: BindExpression(expression.Expression); break;
            case AssignmentNode assignment: ResolveName(assignment.Name, assignment); BindExpression(assignment.Expression); break;
            case ArrayAssignmentNode assignment: ResolveName(assignment.Name, assignment); BindExpression(assignment.Index); BindExpression(assignment.Value); break;
            case CompoundAssignmentNode assignment: ResolveName(assignment.Name, assignment); BindExpression(assignment.Expression); break;
            case IncrementDecrementNode increment: ResolveName(increment.Name, increment); break;
            case ReturnStatementNode value: BindExpression(value.ReturnExpression); break;
            case IfStatementNode conditional:
                BindExpression(conditional.Condition); BindNestedStatement(conditional.ThenBranch); BindNestedStatement(conditional.ElseBranch); break;
            case WhileStatementNode loop: BindExpression(loop.Condition); BindNestedStatement(loop.Body); break;
            case ForStatementNode loop:
                WithScope(new Scope(_currentScope, "for"), () =>
                {
                    if (loop.Initializer is not null) BindStatement(loop.Initializer);
                    BindExpression(loop.Condition);
                    if (loop.Increment is not null) BindStatement(loop.Increment);
                    BindNestedStatement(loop.Body);
                });
                break;
            case ForEachStatementNode loop:
                BindExpression(loop.Iterable);
                WithScope(new Scope(_currentScope, "foreach"), () =>
                {
                    DeclareVariable(_currentScope, loop.VariableName, null, loop);
                    BindNestedStatement(loop.Body);
                });
                break;
            case ThrowStatementNode thrown: BindExpression(thrown.Error); break;
            case TryStatementNode attempt:
                BindNestedStatement(attempt.TryBlock);
                if (attempt.CatchBlock is not null)
                    WithScope(new Scope(_currentScope, "catch"), () =>
                    {
                        if (attempt.CatchVariable is not null)
                            DeclareVariable(_currentScope, attempt.CatchVariable, "error", attempt.CatchBlock);
                        BindBlock(attempt.CatchBlock, createScope: false);
                    });
                BindNestedStatement(attempt.FinallyBlock);
                break;
            case MatchStatementNode match:
                BindExpression(match.Value); BindMatchArms(match.Arms); break;
            case ObjectInstantiationNode creation:
                foreach (ExpressionNode argument in creation.Arguments) BindExpression(argument);
                break;
        }
    }

    private void BindNestedStatement(StatementNode? statement)
    {
        if (statement is null) return;
        if (statement is BlockNode block) BindBlock(block);
        else BindStatement(statement);
    }

    private void BindExpression(ExpressionNode? expression)
    {
        if (expression is null) return;
        switch (expression)
        {
            case IdentifierNode identifier: ResolveName(identifier.Name, identifier); break;
            case FunctionCallNode call:
                ResolveFunction(call.FunctionName, call);
                foreach (ExpressionNode argument in call.Arguments) BindExpression(argument);
                break;
            case BinaryExpressionNode binary: BindExpression(binary.Left); BindExpression(binary.Right); break;
            case LogicalExpressionNode logical: BindExpression(logical.Left); BindExpression(logical.Right); break;
            case UnaryExpressionNode unary: BindExpression(unary.Operand); break;
            case AsyncExpressionNode async: BindExpression(async.Call); break;
            case AwaitExpressionNode await: BindExpression(await.Task); break;
            case NewExpressionNode creation:
                foreach (ExpressionNode argument in creation.Arguments) BindExpression(argument);
                break;
            case MemberAccessNode member: BindMemberAccess(member); break;
            case ArrayAccessNode access: ResolveName(access.Name, access); BindExpression(access.Index); break;
            case ArrayInitializerNode array:
                foreach (ExpressionNode element in array.Elements) BindExpression(element);
                BindExpression(array.SizeExpression); break;
            case MatchExpressionNode match: BindExpression(match.Value); BindMatchArms(match.Arms); break;
        }
    }

    private void BindMemberAccess(MemberAccessNode member)
    {
        // Package and built-in member access has phase-specific visibility and overload rules.
        // Leave the object/member pair to the semantic analyzer, but still bind arguments
        // of a member call because those are ordinary expressions.
        if (member.Expression is FunctionCallNode call)
        {
            foreach (ExpressionNode argument in call.Arguments)
                BindExpression(argument);
            return;
        }

        if (member.Expression is not IdentifierNode)
            BindExpression(member.Expression as ExpressionNode);
    }

    private void BindMatchArms(IEnumerable<MatchArm> arms)
    {
        foreach (MatchArm arm in arms)
        {
            foreach (MatchPatternNode pattern in arm.Patterns)
            {
                if (pattern is ValueMatchPatternNode value) BindExpression(value.Value);
                if (pattern is RangeMatchPatternNode range)
                { BindExpression(range.Start); BindExpression(range.End); }
            }
            if (arm.Body is StatementNode statement) BindStatement(statement);
            else if (arm.Body is ExpressionNode expression) BindExpression(expression);
        }
    }

    private void BindInitializer(ExpressionNode? initializer) => BindExpression(initializer);

    private void DeclareVariable(Scope scope, string name, string? type, AstNode declaration) =>
        Declare(scope, new VariableSymbol(name, TypeFacts.FromName(type)), declaration);

    private void Declare(Scope scope, Symbol symbol, AstNode declaration)
    {
        try
        {
            scope.Declare(symbol);
            _declarations[declaration] = symbol;
        }
        catch (InvalidOperationException exception)
        {
            AddDiagnostic($"Duplicate declaration of '{symbol.Name}'.", declaration, exception.Message);
        }
    }

    private void ResolveName(string name, AstNode reference)
    {
        if (_currentScope.TryLookup(name, out Symbol? symbol) || _globalScope.TryLookup(name, out symbol))
        {
            _references[reference] = symbol!;
            return;
        }
        AddDiagnostic($"Undeclared identifier '{name}'.", reference);
    }

    private void ResolveFunction(string name, AstNode reference)
    {
        if (_functionScope.TryLookup(name, out Symbol? symbol) ||
            (_globalScope.TryLookup(name, out symbol) && symbol is FunctionSymbol))
        {
            _references[reference] = symbol!;
            return;
        }

        // Calls represented by a member-access expression (for example a built-in
        // method) are resolved by the semantic member/type phase, not by this name table.
        if (name.Contains('.', StringComparison.Ordinal))
            return;
        AddDiagnostic($"Undeclared function '{name}'.", reference);
    }

    private void AddDiagnostic(string description, AstNode node, string? detail = null) =>
        _diagnostics.Add(new CompilationDiagnostic("BEN2001", detail is null ? description : description,
            node.Span));

    private void WithScope(Scope scope, Action action)
    {
        Scope previous = _currentScope;
        _currentScope = scope;
        try { action(); }
        finally { _currentScope = previous; }
    }
}
