namespace Benita
{
    /// <summary>
    /// The SemanticAnalyzer class performs semantic analysis on a program to ensure that variables are declared before use,
    /// functions are properly defined, and expressions are correctly typed.
    /// </summary>
    public class SemanticAnalyzer
    {
        /// <summary>آخرین موقعیت AST در حال تحلیل را برای تبدیل خطای داخلی به diagnostic نگه می‌دارد.</summary>
        internal SourceSpan CurrentSpan { get; private set; } = SourceSpan.Unknown;
        /// <summary>
        /// Dictionary to store packages names and their associated PackageNode.
        /// </summary>
        private readonly Dictionary<string, PackageNode> _packages;
        private readonly BindingResult? _bindingResult;
        private readonly Scope _typeSymbols;
        private readonly Scope _functionSymbols;
        private readonly BuiltInClassRegistry _builtInClasses;
        /// <summary>قراردادهای interface ثبت‌شده را برای تحلیل نوع و member access نگه می‌دارد.</summary>
        private readonly Dictionary<string, InterfaceNode> _interfaces;


        /// <summary>
        /// Dictionary to store global variables and their types.
        /// </summary>
        private readonly Scope _globalVariables;

        /// <summary>
        /// Dictionary to store function names and their associated FunctionNode.
        /// </summary>
        private readonly Dictionary<string, FunctionNode> _functions;

        /// <summary>
        /// Initializes a new instance of the SemanticAnalyzer class.
        /// </summary>
        public SemanticAnalyzer(BuiltInClassRegistry? builtInClasses = null,
            BindingResult? bindingResult = null)
        {
            _bindingResult = bindingResult;
            _globalVariables = new Scope(name: "global");
            _typeSymbols = new Scope(name: "types");
            _functionSymbols = new Scope(name: "functions");
            _functions = new Dictionary<string, FunctionNode>();
            _packages = new Dictionary<string, PackageNode>();
            _interfaces = new Dictionary<string, InterfaceNode>();
            _builtInClasses = builtInClasses ?? BuiltInClassRegistry.CreateDefault();
        }

        /// <summary>
        /// Analyzes the provided ProgramNode to perform semantic checks.
        /// </summary>
        /// <param name="program">The ProgramNode to analyze.</param>
        public void Analyze(ProgramNode program)
        {
            SemanticAnalysisResult result = AnalyzeResultOrThrow(program);
            if (!result.IsSuccess)
            {
                CompilationDiagnostic diagnostic = result.Diagnostics.First(diagnostic =>
                    diagnostic.Severity == CompilationDiagnosticSeverity.Error);
                throw new Exception(diagnostic.Description);
            }
        }

        /// <summary>
        /// Runs the semantic phase through an explicit result contract. The method captures
        /// one semantic failure as a diagnostic; callers that need the legacy throwing
        /// behavior should use <see cref="Analyze"/>.
        /// </summary>
        public SemanticAnalysisResult AnalyzeResultOrThrow(ProgramNode program)
        {
            ArgumentNullException.ThrowIfNull(program);
            _packages.Clear();
            _interfaces.Clear();
            _typeSymbols.Clear();
            _functionSymbols.Clear();
            _globalVariables.Clear();
            _functions.Clear();

            try
            {
            if (program.MainFunction is null && program.Statements.Count == 0)
                throw new SemanticException("No main function or top-level executable statement was found.",
                    program.Span);

            foreach (InterfaceNode interfaceNode in program.Interfaces)
                DeclareInterface(interfaceNode);
            foreach (var packageNode in program.Packages)
                DeclarePackage(packageNode);
            ValidateBuiltInClassTypes();
            // امضاها پیش از تحلیل هر call site ثبت می‌شوند تا forward call در initializer نیز معتبر باشد.
            foreach (var function in program.Functions)
                DeclareFunction(function);
            foreach (InterfaceNode interfaceNode in program.Interfaces)
                AnalyzeInterface(interfaceNode);

            // globalها پیش از body بسته‌ها تحلیل می‌شوند تا همان outer scope قابل مشاهده در runtime
            // هنگام تحلیل متدها و initializerهای package نیز در دسترس باشد.
            foreach (var globalVar in program.GlobalVariables)
            {
                var declaredType = globalVar.Type;
                EnsureKnownType(declaredType);
                RequireInitializerForNamedType(declaredType, globalVar.Initializer, globalVar.Name);
                if (globalVar.Initializer != null)
                {
                    var initializerType = AnalyzeExpression(globalVar.Initializer, _globalVariables);
                    if (declaredType == "let")
                    {
                        declaredType = initializerType;
                        RequireConcreteInferredType(declaredType, globalVar.Name);
                    }

                    else if (!CheckType(declaredType!, initializerType))
                    {
                        throw new Exception(
                            $"Type mismatch in global variable '{globalVar.Name}'. Expected '{declaredType}' but got '{initializerType}'.");
                    }
                }
                else if (declaredType == "let")
                {
                    RequireConcreteInferredType(declaredType, globalVar.Name);
                }

                DeclareVariable(globalVar.Name, declaredType, _globalVariables);
            }

            foreach (var packageNode in program.Packages)
                AnalyzePackage(packageNode);

            foreach (var function in program.Functions)
            {
                AnalyzeFunction(function);
            }

            if (program.Statements != null)
            {
                var topLevelVariables = new Scope(_globalVariables, "top-level");
                foreach (var statement in program.Statements)
                {
                    if (statement != null)
                        AnalyzeStatement(statement, topLevelVariables);
                }
            }

            // Analyze the main function
            if (program.MainFunction != null)
                AnalyzeFunction(program.MainFunction);

            return new SemanticAnalysisResult(program, Array.Empty<CompilationDiagnostic>());
            }
            catch (BenitaException exception)
            {
                return new SemanticAnalysisResult(program, new[]
                {
                    new CompilationDiagnostic(exception.Code, exception.Description, exception.Span)
                });
            }
            catch (Exception exception)
            {
                return new SemanticAnalysisResult(program, new[]
                {
                    new CompilationDiagnostic("BEN3001", exception.Message, CurrentSpan)
                });
            }
        }

        private void ValidateBuiltInClassTypes()
        {
            foreach (BuiltInClassDefinition builtInClass in _builtInClasses.Classes.Values)
            {
                foreach (BuiltInParameter parameter in builtInClass.ConstructorParameters)
                    EnsureKnownType(parameter.Type.Name);

                foreach (BuiltInMethodDescriptor method in builtInClass.Methods.Values)
                {
                    EnsureKnownType(method.ReturnType.Name, allowVoid: true);
                    foreach (TypeSymbol parameterType in method.ParameterTypes)
                        EnsureKnownType(parameterType.Name);
                }

                foreach (BuiltInPropertyDescriptor property in builtInClass.Properties.Values)
                    EnsureKnownType(property.Type.Name);
            }
        }

        /// <summary>نام package را ثبت و از برخورد نام آن با سایر نوع‌ها جلوگیری می‌کند.</summary>
        private void DeclarePackage(PackageNode packageNode)
        {
            if (packageNode.Name == Types.Error.Name)
                throw new Exception("The name 'error' is reserved by the language error type.");
            if (_builtInClasses.TryGet(packageNode.Name, out _))
                throw new Exception($"The name '{packageNode.Name}' is reserved by a built-in class.");
            if (_packages.ContainsKey(packageNode.Name) || _interfaces.ContainsKey(packageNode.Name))
            {
                throw new Exception($"Package '{packageNode.Name}' is already declared.");
            }
            _packages[packageNode.Name] = packageNode;
            _typeSymbols.Declare(new TypeDeclarationSymbol(packageNode.Name, SymbolKind.Package,
                new NamedTypeSymbol(packageNode.Name), packageNode));
        }

        /// <summary>نام interface را پیش از تحلیل بدنه‌ها ثبت و تکراری‌بودن نوع را بررسی می‌کند.</summary>
        private void DeclareInterface(InterfaceNode interfaceNode)
        {
            if (interfaceNode.Name == Types.Error.Name)
                throw new Exception("The name 'error' is reserved by the language error type.");
            if (_builtInClasses.TryGet(interfaceNode.Name, out _))
                throw new Exception($"The name '{interfaceNode.Name}' is reserved by a built-in class.");
            if (_interfaces.ContainsKey(interfaceNode.Name) || _packages.ContainsKey(interfaceNode.Name))
                throw new Exception($"Type '{interfaceNode.Name}' is already declared.");
            _interfaces[interfaceNode.Name] = interfaceNode;
            _typeSymbols.Declare(new TypeDeclarationSymbol(interfaceNode.Name, SymbolKind.Interface,
                new NamedTypeSymbol(interfaceNode.Name), interfaceNode));
        }

        /// <summary>نوع‌های امضا و یکتایی متدها و پارامترهای یک interface را اعتبارسنجی می‌کند.</summary>
        private void AnalyzeInterface(InterfaceNode interfaceNode)
        {
            HashSet<string> methodNames = [];
            foreach (InterfaceMethodNode method in interfaceNode.Methods)
            {
                if (!methodNames.Add(method.Name))
                    throw new Exception($"Interface '{interfaceNode.Name}' declares method '{method.Name}' more than once.");
                EnsureKnownType(method.ReturnType, allowVoid: true);
                HashSet<string> parameterNames = [];
                foreach (ParameterNode parameter in method.Parameters)
                {
                    EnsureKnownType(parameter.Type);
                    if (!parameterNames.Add(parameter.Name))
                        throw new Exception($"Parameter '{parameter.Name}' is already declared in '{method.Name}'.");
                }
            }
        }

        /// <summary>فیلدها، مقداردهی اولیه و متدهای یک package را تحلیل معنایی می‌کند.</summary>
        private void AnalyzePackage(PackageNode packageNode)
        {
            ValidateImplementedInterfaces(packageNode);
            Scope allFieldScope = new(_globalVariables, $"package:{packageNode.Name}");
            allFieldScope.Declare(new VariableSymbol("this", TypeFacts.FromName(packageNode.Name), false));
            Dictionary<string, PackageFunctionNode> packageFunctions = new Dictionary<string, PackageFunctionNode>();
            HashSet<string> fieldNames = [];

            foreach (var packageMember in packageNode.Members)
            {
                if (packageMember is PackageVariableDeclarationNode variableDeclaration)
                {
                    if (!fieldNames.Add(variableDeclaration.Name))
                        throw new Exception($"Variable '{variableDeclaration.Name}' is already declared.");
                    TypeSymbol fieldType = TypeFacts.FromName(variableDeclaration.Type);
                    EnsureKnownType(fieldType.Name, allowLet: true);
                    RequireInitializerForNamedType(fieldType.Name, variableDeclaration.Initializer,
                        variableDeclaration.Name);
                    allFieldScope.Declare(new VariableSymbol(variableDeclaration.Name, fieldType));
                }
            }

            Scope initializedFieldScope = new(_globalVariables, $"package-init:{packageNode.Name}");
            initializedFieldScope.Declare(new VariableSymbol("this", TypeFacts.FromName(packageNode.Name), false));
            foreach (var field in packageNode.Members.OfType<PackageVariableDeclarationNode>())
            {
                if (field.Initializer is null)
                {
                    if (field.Type == "let") RequireConcreteInferredType(field.Type, field.Name);
                    initializedFieldScope.Declare(new VariableSymbol(field.Name, TypeFacts.FromName(field.Type)));
                    continue;
                }
                string? initializerType = AnalyzeExpression(field.Initializer, initializedFieldScope);
                if (field.Type == "let")
                {
                    RequireConcreteInferredType(initializerType, field.Name);
                    field.Type = initializerType;
                    allFieldScope.ReplaceLocal(new VariableSymbol(field.Name, TypeFacts.FromName(initializerType)));
                }
                else if (!CheckType(field.Type, initializerType))
                    throw new Exception($"Type mismatch in field '{field.Name}'. Expected '{field.Type}' but got '{initializerType}'.");
                initializedFieldScope.Declare(new VariableSymbol(field.Name, TypeFacts.FromName(field.Type)));
            }

            foreach (var packageMember in packageNode.Members)
            {
                if (packageMember is not PackageFunctionNode functionNode) continue;
                if (!packageFunctions.TryAdd(functionNode.Name, functionNode))
                    throw new Exception($"Function '{functionNode.Name}' is already declared.");
                AnalyzePackageFunction(functionNode, allFieldScope);
            }
        }

        /// <summary>کامل‌بودن، public بودن و تطابق دقیق امضای پیاده‌سازی interfaceها را بررسی می‌کند.</summary>
        private void ValidateImplementedInterfaces(PackageNode package)
        {
            HashSet<string> implemented = [];
            foreach (string interfaceName in package.Interfaces)
            {
                if (!implemented.Add(interfaceName))
                    throw new Exception($"Package '{package.Name}' lists interface '{interfaceName}' more than once.");
                if (!_interfaces.TryGetValue(interfaceName, out InterfaceNode? contract))
                    throw new Exception($"Unknown interface '{interfaceName}'.");
                foreach (InterfaceMethodNode required in contract.Methods)
                {
                    PackageFunctionNode? implementation = package.Members.OfType<PackageFunctionNode>()
                        .FirstOrDefault(method => method.Name == required.Name);
                    if (implementation is null)
                        throw new Exception($"Package '{package.Name}' does not implement '{interfaceName}.{required.Name}'.");
                    if (implementation.AccessModifier != AccessModifier.Public)
                        throw new Exception($"Implementation '{package.Name}.{required.Name}' must be public.");
                    if (implementation.ReturnType != required.ReturnType ||
                        implementation.Parameters.Count != required.Parameters.Count ||
                        implementation.Parameters.Where((parameter, index) =>
                            parameter.Type != required.Parameters[index].Type).Any())
                        throw new Exception($"Method '{package.Name}.{required.Name}' does not match interface '{interfaceName}'.");
                }
            }
        }

        private void AnalyzePackageFunction(PackageFunctionNode function, Scope variableScope)
        {
            EnsureKnownType(function.ReturnType, allowVoid: true);
            // Create a new scope for local variables
            Scope localVariables = new(variableScope, $"function:{function.Name}");

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
                TypeSymbol parameterType = TypeFacts.FromName(param.Type);
                EnsureKnownType(parameterType.Name);
                DeclareVariable(param.Name, param.Type, localVariables);
            }

            // Analyze the function body
            AnalyzeBlock(function.Body, localVariables, function.ReturnType);
            if (function.ReturnType != "void" && !AlwaysReturns(function.Body))
            {
                throw new Exception($"Function '{function.Name}' must return a value of type '{function.ReturnType}'.");
            }
        }

        /// <summary>
        /// Declares a function and performs analysis on it.
        /// </summary>
        /// <param name="function">The function to declare and analyze.</param>
        private void DeclareFunction(FunctionNode function)
        {
            if (function.Name == Types.Error.Name)
                throw new Exception("The function name 'error' is reserved by the language runtime.");
            if (_functions.ContainsKey(function.Name))
            {
                throw new Exception($"Function '{function.Name}' is already declared.");
            }

            _functions[function.Name] = function;
            _functionSymbols.Declare(new FunctionSymbol(
                function.Name,
                TypeFacts.FromName(function.ReturnType),
                function.Parameters.Select(parameter => TypeFacts.FromName(parameter.Type)).ToArray(),
                function));
        }

        /// <summary>
        /// Analyzes a function's parameters, body, and return type.
        /// </summary>
        /// <param name="function">The function to analyze.</param>
        private void AnalyzeFunction(FunctionNode function)
        {
            EnsureKnownType(function.ReturnType, allowVoid: true);
            // Create a new scope for local variables
            Scope localVariables = new(_globalVariables, $"function:{function.Name}");

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
                EnsureKnownType(param.Type);
                DeclareVariable(param.Name, param.Type, localVariables);
            }

            // Analyze the function body
            AnalyzeBlock(function.Body, localVariables, function.ReturnType);
            if (function.ReturnType != "void" && !AlwaysReturns(function.Body))
            {
                throw new Exception($"Function '{function.Name}' must return a value of type '{function.ReturnType}'.");
            }
        }

        /// <summary>
        /// Analyzes a block of statements.
        /// </summary>
        /// <param name="block">The block of statements to analyze.</param>
        /// <param name="localVariables">The local variables available in the block.</param>
        /// <param name="functionReturnType">The return type of the function (if any).</param>
        private void AnalyzeBlock(BlockNode block, Scope localVariables,
            string? functionReturnType = null, int loopDepth = 0)
        {
            foreach (var statement in block.Statements)
            {
                AnalyzeStatement(statement, localVariables, functionReturnType, loopDepth);
            }
        }

        /// <summary>
        /// Analyzes a single statement.
        /// </summary>
        /// <param name="statement">The statement to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <param name="functionReturnType">The return type of the function (if any).</param>
        private void AnalyzeStatement(StatementNode statement, Scope localVariables,
            string? functionReturnType = null, int loopDepth = 0)
        {
            if (statement.Span.Line > 0) CurrentSpan = statement.Span;
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    string? declaredType = varDecl.Type;
                    EnsureKnownType(declaredType, allowLet: true);
                    RequireInitializerForNamedType(declaredType, varDecl.Initializer, varDecl.Name);
                    if (varDecl.Type == "let")
                    {
                        declaredType = varDecl.Initializer != null
                            ? AnalyzeExpression(varDecl.Initializer, localVariables)
                            : varDecl.Type;
                        RequireConcreteInferredType(declaredType, varDecl.Name);
                    }
                    else if (varDecl.Initializer != null)
                    {
                        string? initializerType = AnalyzeExpression(varDecl.Initializer, localVariables);
                        if (!CheckType(varDecl.Type!, initializerType))
                        {
                            throw new Exception(
                                $"Type mismatch in variable '{varDecl.Name}'. Expected '{varDecl.Type}' but got '{initializerType}'.");
                        }
                    }
                    DeclareVariable(varDecl.Name, declaredType, localVariables);
                    break;
                case AssignmentNode assignment:
                    var valueType = AnalyzeExpression(assignment.Expression, localVariables);
                    if (!localVariables.TryGetValue(assignment.Name, out var variableType))
                    {
                        throw new Exception($"Undeclared variable '{assignment.Name}'.");
                    }

                    if (!CheckType(variableType, valueType))
                    {
                        throw new Exception($"Type mismatch in assignment to '{assignment.Name}'. Expected '{variableType}' but got '{valueType}'.");
                    }
                    break;
                case CompoundAssignmentNode compoundAssignment:
                    var compValueType = AnalyzeExpression(compoundAssignment.Expression, localVariables);
                    if (!localVariables.TryGetValue(compoundAssignment.Name, out var compVariableType))
                    {
                        throw new Exception($"Undeclared variable '{compoundAssignment.Name}'.");
                    }

                    if (compVariableType != "number" || compValueType != "number")
                    {
                        throw new Exception($"Compound assignment requires numeric operands, but '{compoundAssignment.Name}' is '{compVariableType}' and the value is '{compValueType}'.");
                    }
                    break;
                case IncrementDecrementNode incDec:
                    if (!localVariables.TryGetValue(incDec.Name, out var incrementType))
                    {
                        throw new Exception($"Undeclared variable '{incDec.Name}'.");
                    }
                    if (incrementType != "number")
                    {
                        throw new Exception($"Increment and decrement require a number, but '{incDec.Name}' is '{incrementType}'.");
                    }
                    break;
                case ExpressionStatementNode exprStmt:
                    AnalyzeExpression(exprStmt.Expression, localVariables);
                    break;
                case IfStatementNode ifStmt:
                    var conditionType = AnalyzeExpression(ifStmt.Condition, localVariables);
                    if (conditionType != "bool")
                    {
                        throw new Exception("Condition in 'if' statement must be a boolean.");
                    }
                    AnalyzeStatement(ifStmt.ThenBranch, new(localVariables), functionReturnType, loopDepth);
                    if (ifStmt.ElseBranch != null)
                    {
                        AnalyzeStatement(ifStmt.ElseBranch, new(localVariables), functionReturnType, loopDepth);
                    }
                    break;
                case MatchStatementNode matchStatement:
                    AnalyzeMatchStatement(matchStatement, localVariables, functionReturnType, loopDepth);
                    break;
                case WhileStatementNode whileStmt:
                    var whileConditionType = AnalyzeExpression(whileStmt.Condition, localVariables);
                    if (whileConditionType != "bool")
                    {
                        throw new Exception("Condition in 'while' statement must be a boolean.");
                    }
                    AnalyzeStatement(whileStmt.Body, new(localVariables), functionReturnType, loopDepth + 1);
                    break;

                case ForStatementNode forStmt:
                    var forScope = new Scope(localVariables);
                    if (forStmt.Initializer != null)
                        AnalyzeStatement(forStmt.Initializer, forScope, functionReturnType, loopDepth);
                    if (forStmt.Condition != null && AnalyzeExpression(forStmt.Condition, forScope) != "bool")
                    {
                        throw new Exception("Condition in 'for' statement must be a boolean.");
                    }
                    if (forStmt.Increment != null)
                        AnalyzeStatement(forStmt.Increment, forScope, functionReturnType, loopDepth + 1);
                    AnalyzeStatement(forStmt.Body, forScope, functionReturnType, loopDepth + 1);
                    break;
                case ForEachStatementNode forEach:
                    string? iterableType = AnalyzeExpression(forEach.Iterable, localVariables);
                    if (iterableType?.EndsWith("[]", StringComparison.Ordinal) != true)
                        throw new Exception($"The expression after 'in' must be an array, but got '{iterableType}'.");
                    var iterationScope = new Scope(localVariables);
                    DeclareVariable(forEach.VariableName, iterableType[..^2], iterationScope);
                    AnalyzeStatement(forEach.Body, iterationScope, functionReturnType, loopDepth + 1);
                    break;
                case BlockNode block:
                    AnalyzeBlock(block, localVariables, functionReturnType, loopDepth);
                    break;
                case ArrayAssignmentNode arrayAssignment:
                    AnalyzeArrayAssignment(arrayAssignment, localVariables);
                    break;
                case ThrowStatementNode throwStatement:
                    string? thrownType = AnalyzeExpression(throwStatement.Error, localVariables);
                    if (thrownType != Types.Error.Name)
                        throw new Exception($"A throw statement requires an error value, but got '{thrownType}'.");
                    break;
                case TryStatementNode tryStatement:
                    AnalyzeStatement(tryStatement.TryBlock, new(localVariables), functionReturnType, loopDepth);
                    if (tryStatement.CatchBlock is not null)
                    {
                        var catchScope = new Scope(localVariables);
                        catchScope[tryStatement.CatchVariable!] = Types.Error.Name;
                        AnalyzeStatement(tryStatement.CatchBlock, catchScope, functionReturnType, loopDepth);
                    }
                    if (tryStatement.FinallyBlock is not null)
                        AnalyzeStatement(tryStatement.FinallyBlock, new(localVariables), functionReturnType: null, loopDepth: 0);
                    break;
                case ReturnStatementNode returnStatementNode:
                    if (functionReturnType == null)
                    {
                        throw new Exception("'return' can only be used inside a function.");
                    }
                    var resultValueType = returnStatementNode.ReturnExpression == null
                        ? "void"
                        : AnalyzeExpression(returnStatementNode.ReturnExpression, localVariables);
                    if (!CheckType(functionReturnType, resultValueType))
                    {
                        throw new Exception($"Return type mismatch in function. Expected '{functionReturnType}' but got '{resultValueType}'.");
                    }
                    break;
                case ObjectInstantiationNode objectInstantiationNode:

                    if (!_packages.ContainsKey(objectInstantiationNode.PackageName))
                    {
                        throw new Exception(
                            $"A package with this name : {objectInstantiationNode.PackageName} has not been implemented ");
                    }
                    if (localVariables.TryLookupLocal(objectInstantiationNode.Name, out _))
                    {
                        throw new Exception($"Variable '{objectInstantiationNode.Name}' is already declared.");
                    }

                    localVariables.Declare(new VariableSymbol(objectInstantiationNode.Name,
                        TypeFacts.FromName(objectInstantiationNode.PackageName)));
                    break;
                case BreakStatementNode:
                case ContinueStatementNode:
                    if (loopDepth == 0)
                        throw new Exception($"'{(statement is BreakStatementNode ? "break" : "continue")}' can only be used inside a loop.");
                    break;
                default:
                    throw new Exception($"Unsupported statement type: {statement.GetType().Name}");
            }
        }

        private static bool AlwaysReturns(StatementNode? statement) => statement switch
        {
            ReturnStatementNode => true,
            ThrowStatementNode => true,
            BlockNode block => block.Statements.Any(AlwaysReturns),
            IfStatementNode branch => branch.ElseBranch != null &&
                                      AlwaysReturns(branch.ThenBranch) && AlwaysReturns(branch.ElseBranch),
            MatchStatementNode match => match.Arms.Any(arm => arm.IsDefault) &&
                                        match.Arms.All(arm => AlwaysReturns(arm.Body as StatementNode)),
            TryStatementNode tryStatement =>
                tryStatement.FinallyBlock is not null && AlwaysReturns(tryStatement.FinallyBlock) ||
                AlwaysReturns(tryStatement.TryBlock) &&
                (tryStatement.CatchBlock is null || AlwaysReturns(tryStatement.CatchBlock)),
            WhileStatementNode loop => IsAlwaysTrue(loop.Condition) &&
                                       AlwaysReturns(loop.Body) &&
                                       !ContainsBreakForCurrentLoop(loop.Body),
            ForStatementNode loop => (loop.Condition is null || IsAlwaysTrue(loop.Condition)) &&
                                     AlwaysReturns(loop.Body) &&
                                     !ContainsBreakForCurrentLoop(loop.Body),
            _ => false
        };

        /// <summary>تشخیص می‌دهد شرط حلقه literal صحیح و در نتیجه ورود به آن قطعی است.</summary>
        private static bool IsAlwaysTrue(ExpressionNode? expression) =>
            expression is LiteralNode { Type: TokenType.TRUE_LITERAL };

        /// <summary>تشخیص می‌دهد شرط literal قطعاً نادرست است و شاخهٔ then قابل‌دسترسی نیست.</summary>
        private static bool IsAlwaysFalse(ExpressionNode? expression) =>
            expression is LiteralNode { Type: TokenType.FALSE_LITERAL };

        /// <summary>وجود break متعلق به حلقهٔ جاری را بدون شمردن break حلقه‌های تو در تو بررسی می‌کند.</summary>
        private static bool ContainsBreakForCurrentLoop(StatementNode? statement) => statement switch
        {
            BreakStatementNode => true,
            BlockNode block => block.Statements.Any(ContainsBreakForCurrentLoop),
            IfStatementNode branch when IsAlwaysTrue(branch.Condition) =>
                ContainsBreakForCurrentLoop(branch.ThenBranch),
            IfStatementNode branch when IsAlwaysFalse(branch.Condition) =>
                ContainsBreakForCurrentLoop(branch.ElseBranch),
            IfStatementNode branch => ContainsBreakForCurrentLoop(branch.ThenBranch) ||
                                      ContainsBreakForCurrentLoop(branch.ElseBranch),
            MatchStatementNode match => match.Arms.Any(arm =>
                ContainsBreakForCurrentLoop(arm.Body as StatementNode)),
            TryStatementNode tryStatement => ContainsBreakForCurrentLoop(tryStatement.TryBlock) ||
                                             ContainsBreakForCurrentLoop(tryStatement.CatchBlock) ||
                                             ContainsBreakForCurrentLoop(tryStatement.FinallyBlock),
            WhileStatementNode or ForStatementNode or ForEachStatementNode => false,
            _ => false
        };

        private void AnalyzeMatchStatement(MatchStatementNode match,
            Scope localVariables, string? functionReturnType, int loopDepth)
        {
            string? valueType = AnalyzeExpression(match.Value, localVariables);
            foreach (MatchArm arm in match.Arms)
            {
                AnalyzeMatchPatterns(arm, valueType, localVariables);

                AnalyzeStatement((StatementNode)arm.Body, new(localVariables), functionReturnType, loopDepth);
            }
        }

        private void AnalyzeMatchPatterns(MatchArm arm, string? valueType,
            Scope localVariables)
        {
            foreach (MatchPatternNode pattern in arm.Patterns)
            {
                switch (pattern)
                {
                    case ValueMatchPatternNode valuePattern:
                        string? patternType = AnalyzeExpression(valuePattern.Value, localVariables);
                        if (!CheckType(valueType, patternType))
                            throw new Exception($"Match pattern type mismatch. Expected '{valueType}' but got '{patternType}'.");
                        break;
                    case RangeMatchPatternNode range:
                        string? startType = AnalyzeExpression(range.Start, localVariables);
                        string? endType = AnalyzeExpression(range.End, localVariables);
                        if (valueType != "number" || startType != "number" || endType != "number")
                            throw new Exception("Match range patterns require a number value and numeric boundaries.");
                        break;
                }
            }
        }

        /// <summary>
        /// Analyzes an array assignment statement.
        /// </summary>
        /// <param name="arrayAssignment">The array assignment statement to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        private void AnalyzeArrayAssignment(ArrayAssignmentNode arrayAssignment, Scope localVariables)
        {
            // Check if the array variable is declared
            if (!localVariables.TryGetValue(arrayAssignment.Name, out var arrayType))
            {
                throw new Exception($"Undeclared array variable '{arrayAssignment.Name}'.");
            }

            // Ensure the variable is indeed an array
            if (!arrayType.EndsWith("[]"))
            {
                throw new Exception($"Variable '{arrayAssignment.Name}' is not an array.");
            }

            // Analyze the index expression
            var indexType = AnalyzeExpression(arrayAssignment.Index, localVariables);

            // Ensure the index expression type is 'number'
            if (indexType != "number")
            {
                throw new Exception("Array index must be of type 'number'.");
            }

            // Analyze the value expression
            var valueType = AnalyzeExpression(arrayAssignment.Value, localVariables);

            // Remove the '[]' from arrayType to get the element type
            var elementType = arrayType.Substring(0, arrayType.Length - 2);

            // Ensure the value expression type matches the element type of the array
            if (elementType != valueType)
            {
                throw new Exception($"Type mismatch in array assignment to '{arrayAssignment.Name}[{arrayAssignment.Index}]'. Expected '{elementType}' but got '{valueType}'.");
            }
        }

        /// <summary>
        /// Analyzes an expression and determines its type.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the expression.</returns>
        private string? AnalyzeExpression(ExpressionNode expression, Scope localVariables)
        {
            if (expression.Span.Line > 0) CurrentSpan = expression.Span;
            switch (expression)
            {
                case LiteralNode literal:
                    return ConvertTokenTypeToString(literal.Type);
                case IdentifierNode identifier:
                    return HandleIdentifierNode(identifier, localVariables);
                case BinaryExpressionNode binary:
                    return HandleBinaryExpressionNode(binary, localVariables);
                case UnaryExpressionNode unary:
                    return HandleUnaryExpressionNode(unary, localVariables);
                case FunctionCallNode functionCall:
                    return HandleFunctionCallNode(functionCall, localVariables);
                case AsyncExpressionNode asyncExpression:
                    return Types.TaskOf(TypeFacts.FromName(
                        HandleFunctionCallNode(asyncExpression.Call, localVariables))).Name;
                case AwaitExpressionNode awaitExpression:
                    TypeSymbol awaitedType = TypeFacts.FromName(
                        AnalyzeExpression(awaitExpression.Task, localVariables));
                    if (awaitedType is not TaskTypeSymbol taskType)
                        throw new Exception("'await' requires a task value.");
                    return taskType.ResultType.Name;
                case ArrayInitializerNode arrayInit:
                    return HandleArrayInitializerNode(arrayInit, localVariables);
                case ArrayAccessNode arrayAccess:
                    return HandleArrayAccessNode(arrayAccess, localVariables);
                case LogicalExpressionNode logical:
                    return HandleLogicalExpressionNode(logical, localVariables);
                case MemberAccessNode memberAccess:
                    return HandleMemberAccessNode(memberAccess, localVariables);
                case NewExpressionNode creation:
                    return AnalyzeNewExpression(creation, localVariables);
                case MatchExpressionNode matchExpression:
                    return AnalyzeMatchExpression(matchExpression, localVariables);
                default:
                    throw new Exception($"Unsupported expression type: {expression.GetType().Name}");
            }
        }

        private string AnalyzeNewExpression(NewExpressionNode creation,
            Scope localVariables)
        {
            if (_builtInClasses.TryGet(creation.PackageName, out BuiltInClassDefinition? builtInClass))
            {
                ValidateArguments(creation.PackageName, creation.Arguments,
                    builtInClass.ConstructorParameters.Select(parameter =>
                        new ParameterNode(parameter.Type.Name, parameter.Name)).ToList(), localVariables);
                return builtInClass.Name;
            }

            if (!_packages.TryGetValue(creation.PackageName, out PackageNode? package))
                throw new Exception($"Unknown package or built-in class '{creation.PackageName}'.");

            PackageFunctionNode? initializer = package.Members.OfType<PackageFunctionNode>()
                .FirstOrDefault(member => member.Name == "init");
            if (initializer is null)
            {
                if (creation.Arguments.Count != 0)
                    throw new Exception($"Package '{package.Name}' has no init constructor and accepts no arguments.");
                return package.Name;
            }

            ValidateArguments("init", creation.Arguments, initializer.Parameters, localVariables);
            return package.Name;
        }

        private string? AnalyzeMatchExpression(MatchExpressionNode match,
            Scope localVariables)
        {
            string? valueType = AnalyzeExpression(match.Value, localVariables);
            string? resultType = null;
            foreach (MatchArm arm in match.Arms)
            {
                AnalyzeMatchPatterns(arm, valueType, localVariables);

                string? armType = AnalyzeExpression((ExpressionNode)arm.Body, localVariables);
                resultType ??= armType;
                if (!CheckType(resultType, armType))
                    throw new Exception($"All match expression arms must return the same type. Expected '{resultType}' but got '{armType}'.");
            }
            return resultType;
        }

        private string? HandleMemberAccessNode(MemberAccessNode memberAccess, Scope localVariables)
        {
            if (_builtInClasses.TryGet(memberAccess.ObjectName, out BuiltInClassDefinition? staticClass) &&
                memberAccess.Expression is FunctionCallNode staticCall &&
                staticClass.StaticMethods.ContainsKey(staticCall.FunctionName))
                return AnalyzeBuiltInStaticClassMemberAccess(staticClass, staticCall, localVariables);

            if (!localVariables.TryGetValue(memberAccess.ObjectName, out string? packageName) || packageName is null)
                throw new Exception($"Member access requires a package instance, but '{memberAccess.ObjectName}' is not one.");

            if (packageName == Types.Error.Name)
            {
                if (memberAccess.Expression is IdentifierNode errorMember &&
                    errorMember.Name is "code" or "message")
                    return "string";
                throw new Exception("An error value exposes only the read-only members 'code' and 'message'.");
            }

            if (_interfaces.TryGetValue(packageName, out InterfaceNode? outInterface))
            {
                if (memberAccess.Expression is not FunctionCallNode interfaceCall)
                    throw new Exception($"Interface '{outInterface.Name}' exposes methods only.");
                InterfaceMethodNode? contractMethod = outInterface.Methods
                    .FirstOrDefault(method => method.Name == interfaceCall.FunctionName);
                if (contractMethod is null)
                    throw new Exception($"Interface '{outInterface.Name}' has no method named '{interfaceCall.FunctionName}'.");
                ValidateArguments(interfaceCall.FunctionName, interfaceCall.Arguments,
                    contractMethod.Parameters, localVariables);
                return contractMethod.ReturnType;
            }

            if (_builtInClasses.TryGet(packageName, out BuiltInClassDefinition? builtInClass))
                return AnalyzeBuiltInClassMemberAccess(memberAccess, localVariables, builtInClass);

            if (!_packages.TryGetValue(packageName, out PackageNode? outPackage))
                throw new Exception($"Member access requires a package instance, but '{memberAccess.ObjectName}' is not one.");

            bool requirePublic = memberAccess.ObjectName != "this";
            switch (memberAccess.Expression)
            {
                case FunctionCallNode expression:
                    PackageFunctionNode? method = outPackage.Members.OfType<PackageFunctionNode>()
                        .FirstOrDefault(member => member.Name == expression.FunctionName);
                    if (method is null)
                        throw new Exception($"Package '{outPackage.Name}' has no method named '{expression.FunctionName}'.");
                    if (requirePublic)
                        EnsurePublic(method.AccessModifier, outPackage.Name, expression.FunctionName);
                    ValidateArguments(expression.FunctionName, expression.Arguments, method.Parameters, localVariables);
                    return method.ReturnType;

                case CompoundAssignmentNode compoundAssignment:

                    var compValueType = AnalyzeExpression(compoundAssignment.Expression, localVariables);
                    string? compVariableType = FindPackageFieldType(outPackage, compoundAssignment.Name, requirePublic);
                    if (compVariableType != "number" || compValueType != "number")
                        throw new Exception($"Compound assignment requires numeric operands for member '{compoundAssignment.Name}'.");
                    return compValueType;

                case IdentifierNode identifierNode:
                    return FindPackageFieldType(outPackage, identifierNode.Name, requirePublic);

                case AssignmentNode assignment:
                    string? fieldType = FindPackageFieldType(outPackage, assignment.Name, requirePublic);
                    string? assignedType = AnalyzeExpression(assignment.Expression, localVariables);
                    if (!CheckType(fieldType!, assignedType))
                        throw new Exception($"Type mismatch in assignment to member '{assignment.Name}'. Expected '{fieldType}' but got '{assignedType}'.");
                    return fieldType;

                case IncrementDecrementNode increment:
                    string incrementType = FindPackageFieldType(outPackage, increment.Name, requirePublic);
                    if (incrementType != "number")
                        throw new Exception($"Increment and decrement require a numeric member, but '{increment.Name}' is '{incrementType}'.");
                    return incrementType;

                default:
                    throw new Exception($"Unsupported expression type: {memberAccess.GetType().Name}");
            }
        }

        private string AnalyzeBuiltInStaticClassMemberAccess(
            BuiltInClassDefinition builtInClass,
            FunctionCallNode call,
            Scope localVariables)
        {
            BuiltInMethodDescriptor method = builtInClass.StaticMethods[call.FunctionName];
            if (call.Arguments.Count != method.ParameterTypes.Count)
                throw new SemanticException(
                    $"'{builtInClass.Name}.{call.FunctionName}' expects {method.ParameterTypes.Count} argument(s), but got {call.Arguments.Count}.");

            TypeSymbol? arrayElementType = null;
            for (int index = 0; index < call.Arguments.Count; index++)
            {
                string? actualType = AnalyzeExpression(call.Arguments[index], localVariables);
                string expectedType = method.ParameterTypes[index].Name;
                if (!CheckType(expectedType, actualType))
                    throw new SemanticException(
                        $"Type mismatch in argument {index + 1} of '{builtInClass.Name}.{call.FunctionName}'. Expected '{expectedType}' but got '{actualType}'.");

                if (builtInClass.Name == "Array" && index == 0 &&
                    TypeFacts.FromName(actualType) is ArrayTypeSymbol arrayType)
                    arrayElementType = arrayType.ElementType;
            }

            if (builtInClass.Name == "Array" && arrayElementType is not null)
            {
                bool elementArgument = call.FunctionName is "add" or "contains" or "index_of" && call.Arguments.Count > 1
                    ? call.FunctionName is "add" or "contains" or "index_of"
                    : call.FunctionName == "insert";
                int elementIndex = call.FunctionName == "insert" ? 2 : 1;
                if (elementArgument && call.Arguments.Count > elementIndex)
                {
                    TypeSymbol elementType = TypeFacts.FromName(AnalyzeExpression(call.Arguments[elementIndex], localVariables));
                    if (!TypeFacts.IsAssignableTo(elementType, arrayElementType))
                        throw new SemanticException(
                            $"Type mismatch in argument {elementIndex + 1} of '{builtInClass.Name}.{call.FunctionName}'. Expected '{arrayElementType}' but got '{elementType}'.");
                }

                if (call.FunctionName == "concat" && call.Arguments.Count > 1 &&
                    TypeFacts.FromName(AnalyzeExpression(call.Arguments[1], localVariables)) is ArrayTypeSymbol secondArray &&
                    !TypeFacts.IsAssignableTo(secondArray.ElementType, arrayElementType))
                    throw new SemanticException(
                        $"Type mismatch in argument 2 of '{builtInClass.Name}.{call.FunctionName}'. Expected '{arrayElementType}[]' but got '{secondArray}'.");
            }

            if (builtInClass.Name == "Array" &&
                method.ReturnType == Types.AnyArray &&
                call.Arguments.Count > 0)
            {
                TypeSymbol firstType = TypeFacts.FromName(AnalyzeExpression(call.Arguments[0], localVariables));
                if (firstType is ArrayTypeSymbol arrayType)
                    return Types.ArrayOf(arrayType.ElementType).Name;
            }

            return method.ReturnType.Name;
        }

        private string AnalyzeBuiltInClassMemberAccess(
            MemberAccessNode memberAccess,
            Scope localVariables,
            BuiltInClassDefinition builtInClass)
        {
            switch (memberAccess.Expression)
            {
                case FunctionCallNode call:
                    if (!builtInClass.Methods.TryGetValue(call.FunctionName, out BuiltInMethodDescriptor? method))
                        throw new SemanticException($"Class '{builtInClass.Name}' has no method named '{call.FunctionName}'.");
                    if (call.Arguments.Count != method.ParameterTypes.Count)
                        throw new SemanticException(
                            $"'{builtInClass.Name}.{call.FunctionName}' expects {method.ParameterTypes.Count} argument(s), but got {call.Arguments.Count}.");
                    for (int index = 0; index < call.Arguments.Count; index++)
                    {
                        string? actualType = AnalyzeExpression(call.Arguments[index], localVariables);
                        string expectedType = method.ParameterTypes[index].Name;
                        if (!CheckType(expectedType, actualType))
                            throw new SemanticException(
                                $"Type mismatch in argument {index + 1} of '{builtInClass.Name}.{call.FunctionName}'. Expected '{expectedType}' but got '{actualType}'.");
                    }
                    return method.ReturnType.Name;

                case IdentifierNode identifier:
                    if (!builtInClass.Properties.TryGetValue(identifier.Name, out BuiltInPropertyDescriptor? property))
                        throw new SemanticException($"Class '{builtInClass.Name}' has no property named '{identifier.Name}'.");
                    if (!property.CanRead)
                        throw new SemanticException($"Property '{builtInClass.Name}.{identifier.Name}' is not readable.");
                    return property.Type.Name;

                case AssignmentNode assignment:
                    if (!builtInClass.Properties.TryGetValue(assignment.Name, out BuiltInPropertyDescriptor? assignedProperty))
                        throw new SemanticException($"Class '{builtInClass.Name}' has no property named '{assignment.Name}'.");
                    if (!assignedProperty.CanWrite)
                        throw new SemanticException($"Property '{builtInClass.Name}.{assignment.Name}' is read-only.");
                    string? assignedType = AnalyzeExpression(assignment.Expression, localVariables);
                    if (!CheckType(assignedProperty.Type.Name, assignedType))
                        throw new SemanticException(
                            $"Type mismatch in assignment to property '{assignment.Name}'. Expected '{assignedProperty.Type}' but got '{assignedType}'.");
                    return assignedProperty.Type.Name;

                case CompoundAssignmentNode compound:
                    if (!builtInClass.Properties.TryGetValue(compound.Name, out BuiltInPropertyDescriptor? compoundProperty))
                        throw new SemanticException($"Class '{builtInClass.Name}' has no property named '{compound.Name}'.");
                    if (!compoundProperty.CanRead || !compoundProperty.CanWrite)
                        throw new SemanticException($"Property '{builtInClass.Name}.{compound.Name}' must be readable and writable.");
                    string? compoundType = AnalyzeExpression(compound.Expression, localVariables);
                    if (compoundProperty.Type != Types.Number || compoundType != Types.Number.Name)
                        throw new SemanticException($"Compound assignment requires numeric operands for property '{compound.Name}'.");
                    return compoundProperty.Type.Name;

                case IncrementDecrementNode increment:
                    if (!builtInClass.Properties.TryGetValue(increment.Name, out BuiltInPropertyDescriptor? incrementProperty))
                        throw new SemanticException($"Class '{builtInClass.Name}' has no property named '{increment.Name}'.");
                    if (!incrementProperty.CanRead || !incrementProperty.CanWrite ||
                        incrementProperty.Type != Types.Number)
                        throw new SemanticException($"Increment and decrement require a readable, writable numeric property '{increment.Name}'.");
                    return incrementProperty.Type.Name;

                default:
                    throw new SemanticException($"Unsupported member access for built-in class '{builtInClass.Name}'.");
            }
        }

        private static string FindPackageFieldType(PackageNode package, string fieldName, bool requirePublic = false)
        {
            PackageVariableDeclarationNode? field = package.Members.OfType<PackageVariableDeclarationNode>()
                .FirstOrDefault(member => member.Name == fieldName);
            if (field is null)
                throw new Exception($"Package '{package.Name}' has no field named '{fieldName}'.");
            if (requirePublic)
                EnsurePublic(field.AccessModifier, package.Name, fieldName);
            return field.Type!;
        }

        private static void EnsurePublic(AccessModifier access, string packageName, string memberName)
        {
            if (access == AccessModifier.Private)
                throw new Exception($"Member '{memberName}' is private in package '{packageName}'.");
        }

        private void ValidateArguments(string callableName, IReadOnlyList<ExpressionNode> arguments,
            IReadOnlyList<ParameterNode> parameters, Scope localVariables)
        {
            if (arguments.Count != parameters.Count)
                throw new Exception($"'{callableName}' expects {parameters.Count} argument(s), but got {arguments.Count}.");
            for (int index = 0; index < arguments.Count; index++)
            {
                string? actualType = AnalyzeExpression(arguments[index], localVariables);
                string? expectedType = parameters[index].Type;
                if (!CheckType(expectedType!, actualType))
                    throw new Exception($"Type mismatch in argument {index + 1} of '{callableName}'. Expected '{expectedType}' but got '{actualType}'.");
            }
        }

        /// <summary>
        /// Handles identifier nodes and retrieves their type from local variables.
        /// </summary>
        /// <param name="identifier">The identifier node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the identifier.</returns>
        private string? HandleIdentifierNode(IdentifierNode identifier, Scope localVariables)
        {
            if (_bindingResult is not null &&
                _bindingResult.TryGetSymbol(identifier, out Symbol? boundSymbol) &&
                boundSymbol!.Type is not null &&
                boundSymbol.Type != Types.Unknown &&
                boundSymbol.Type != Types.Inferred)
                return boundSymbol.Type.Name;

            if (!localVariables.TryGetValue(identifier.Name, out var node))
            {
                throw new Exception($"Undeclared variable '{identifier.Name}'.");
            }
            return node;
        }

        /// <summary>
        /// Handles binary expression nodes and determines their result type.
        /// </summary>
        /// <param name="binary">The binary expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the binary expression result.</returns>
        private string? HandleBinaryExpressionNode(BinaryExpressionNode binary, Scope localVariables)
        {
            var leftType = AnalyzeExpression(binary.Left, localVariables);
            var rightType = AnalyzeExpression(binary.Right, localVariables);

            if (IsArithmeticOperator(binary.Operator))
            {
                return HandleArithmeticOperator(binary, leftType, rightType);
            }

            if (binary.Operator is "==" or "!=")
            {
                return HandleEqualityOperator(leftType, rightType);
            }

            if (IsOrderedComparisonOperator(binary.Operator))
            {
                return HandleComparisonOperator(leftType, rightType);
            }

            return leftType; // Assuming both sides have the same type for non-arithmetic operations
        }

        /// <summary>
        /// Handles arithmetic operators in binary expressions and determines the result type.
        /// </summary>
        /// <param name="binary">The binary expression node to handle.</param>
        /// <param name="leftType">The type of the left operand.</param>
        /// <param name="rightType">The type of the right operand.</param>
        /// <returns>The type of the result of the arithmetic operation.</returns>
        private string? HandleArithmeticOperator(BinaryExpressionNode binary, string? leftType, string? rightType)
        {
            if (binary.Operator == "+" && (leftType == "string" || rightType == "string"))
            {
                if (leftType == "string" && rightType == "string")
                    return "string";
                if (leftType == "string" && rightType == "number")
                    return "string";
                if (leftType == "number" && rightType == "string")
                    return "string";

                throw new Exception("Operands of arithmetic operations must be of type 'number'.");
            }

            if (leftType != "number" || rightType != "number")
            {
                throw new Exception("Operands of arithmetic operations must be of type 'number'.");
            }
            return "number";
        }

        /// <summary>
        /// Handles comparison operators in binary expressions and determines the result type.
        /// </summary>
        /// <param name="leftType">The type of the left operand.</param>
        /// <param name="rightType">The type of the right operand.</param>
        /// <returns>The type of the result of the comparison operation.</returns>
        private string? HandleComparisonOperator(string? leftType, string? rightType)
        {
            if (leftType != "number" || rightType != "number")
            {
                throw new Exception("Operands of comparison operations must be of type 'number'.");
            }
            return "bool"; // Comparison operators result in boolean type
        }

        /// <summary>برابری را فقط برای دو مقدار scalar هم‌نوع معتبر می‌داند.</summary>
        private static string HandleEqualityOperator(string? leftType, string? rightType)
        {
            bool supportedType = leftType is "number" or "string" or "bool";
            if (!supportedType || leftType != rightType)
                throw new Exception("Equality operands must have the same scalar type.");
            return "bool";
        }

        /// <summary>
        /// Handles unary expression nodes and determines their result type.
        /// </summary>
        /// <param name="unary">The unary expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the unary expression result.</returns>
        private string? HandleUnaryExpressionNode(UnaryExpressionNode unary, Scope localVariables)
        {
            string? operandType = AnalyzeExpression(unary.Operand, localVariables);
            return unary.Operator switch
            {
                "-" when operandType == Types.Number.Name => Types.Number.Name,
                "!" when operandType == Types.Bool.Name => Types.Bool.Name,
                "-" => throw new Exception("Unary '-' requires an operand of type 'number'."),
                "!" => throw new Exception("Unary '!' requires an operand of type 'bool'."),
                _ => throw new Exception($"Unknown unary operator '{unary.Operator}'.")
            };
        }

        /// <summary>
        /// Handles function call nodes and determines the return type of the function call.
        /// </summary>
        /// <param name="functionCall">The function call node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The return type of the function call.</returns>
        private string? HandleFunctionCallNode(FunctionCallNode functionCall, Scope localVariables)
        {
            Symbol? functionSymbol = null;
            bool hasBoundFunction = _bindingResult is not null &&
                _bindingResult.TryGetSymbol(functionCall, out functionSymbol);
            if (!hasBoundFunction)
                _functionSymbols.TryLookup(functionCall.FunctionName, out functionSymbol);

            if (functionSymbol is not FunctionSymbol resolvedFunction ||
                resolvedFunction.Declaration is null)
                throw new Exception($"Undeclared function '{functionCall.FunctionName}'.");
            FunctionNode function = resolvedFunction.Declaration;

            List<TypeSymbol> parameterTypes = function.Parameters
                .Select(parameter => TypeFacts.FromName(parameter.Type))
                .ToList();
            if (functionCall.Arguments.Count != parameterTypes.Count)
            {
                throw new Exception($"Argument count mismatch in function call to '{functionCall.FunctionName}'. Expected {parameterTypes.Count} but got {functionCall.Arguments.Count}.");
            }

            for (int i = 0; i < functionCall.Arguments.Count; i++)
            {
                TypeSymbol argType = TypeFacts.FromName(
                    AnalyzeExpression(functionCall.Arguments[i], localVariables));
                TypeSymbol expectedType = parameterTypes[i];
                if (!CheckType(expectedType.Name, argType.Name))
                {
                    throw new Exception($"Type mismatch in argument {i + 1} of function call to '{functionCall.FunctionName}'. Expected '{expectedType}' but got '{argType}'.");
                }
            }

            return TypeFacts.FromName(function.ReturnType).Name;
        }

        /// <summary>
        /// Handles array initializer nodes and determines the type of the array.
        /// </summary>
        /// <param name="arrayInit">The array initializer node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the array.</returns>
        private string? HandleArrayInitializerNode(ArrayInitializerNode arrayInit, Scope localVariables)
        {
            if (arrayInit.ElementType is not null)
            {
                string? sizeType = AnalyzeExpression(arrayInit.SizeExpression, localVariables);
                if (sizeType != Types.Number.Name)
                    throw new Exception("Sized array length must be of type 'number'.");
                return $"{arrayInit.ElementType}[]";
            }

            var elementType = "unknown"; // Placeholder for the element type of the array
            foreach (var element in arrayInit.Elements)
            {
                var elementExprType = AnalyzeExpression(element, localVariables);
                if (elementType == "unknown")
                {
                    elementType = elementExprType; // Set the element type on first iteration
                }
                else if (elementType != elementExprType)
                {
                    throw new Exception("Array elements must have consistent types.");
                }
            }
            return $"{elementType}[]"; // Return the array type
        }

        /// <summary>
        /// Handles array access nodes and determines the type of the array element.
        /// </summary>
        /// <param name="arrayAccess">The array access node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the array element.</returns>
        private string? HandleArrayAccessNode(ArrayAccessNode arrayAccess, Scope localVariables)
        {
            if (!localVariables.TryGetValue(arrayAccess.Name, out var arrayType))
            {
                throw new Exception($"Undeclared array variable '{arrayAccess.Name}'.");
            }

            var indexType = AnalyzeExpression(arrayAccess.Index, localVariables);
            if (indexType != Types.Number.Name)
            {
                throw new Exception("Array index must be of type 'number'.");
            }
            if (!arrayType.EndsWith("[]"))
            {
                throw new Exception($"Cannot index into non-array type '{arrayType}'.");
            }
            var elementType = arrayType.Substring(0, arrayType.Length - 2);
            return elementType;
        }

        /// <summary>
        /// Handles logical expression nodes and determines their result type.
        /// </summary>
        /// <param name="logical">The logical expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the result of the logical operation.</returns>
        private string? HandleLogicalExpressionNode(LogicalExpressionNode logical, Scope localVariables)
        {
            var leftType = AnalyzeExpression(logical.Left, localVariables);
            var rightType = AnalyzeExpression(logical.Right, localVariables);
            if (leftType != "bool" || rightType != "bool")
            {
                throw new Exception("Operands of logical operations must be of type 'bool'.");
            }
            if (logical.Operator != "&&" && logical.Operator != "||")
            {
                throw new Exception("Operator of logical operations must be || or &&.");
            }
            return "bool";
        }

        /// <summary>
        /// Determines if a given operator is an arithmetic operator.
        /// </summary>
        /// <param name="op">The operator to check.</param>
        /// <returns>True if the operator is an arithmetic operator, otherwise false.</returns>
        private bool IsArithmeticOperator(string op)
        {
            return op == "+" || op == "-" || op == "*" || op == "/" || op == "%";
        }

        /// <summary>
        /// Determines if a given operator is a comparison operator.
        /// </summary>
        /// <param name="op">The operator to check.</param>
        /// <returns>True if the operator is a comparison operator, otherwise false.</returns>
        private bool IsOrderedComparisonOperator(string op)
        {
            return op == "<" || op == ">" || op == "<=" || op == ">=";
        }

        /// <summary>
        /// Checks if two types are compatible.
        /// </summary>
        /// <param name="firstType">The first type to check.</param>
        /// <param name="secondType">The second type to check.</param>
        /// <returns>True if the types are compatible, otherwise false.</returns>
        private bool CheckType(string firstType, string? secondType)
        {
            TypeSymbol target = TypeFacts.FromName(firstType);
            TypeSymbol source = TypeFacts.FromName(secondType);
            if (target is NamedTypeSymbol targetNamed && source is NamedTypeSymbol sourceNamed &&
                _typeSymbols.TryLookup(targetNamed.Name, out Symbol? targetSymbol) &&
                targetSymbol.Kind == SymbolKind.Interface &&
                _typeSymbols.TryLookup(sourceNamed.Name, out Symbol? sourceSymbol) &&
                sourceSymbol.Kind == SymbolKind.Package &&
                _packages.TryGetValue(sourceNamed.Name, out PackageNode? sourcePackage))
                return sourcePackage.Interfaces.Contains(targetNamed.Name);
            return TypeFacts.IsAssignableTo(source, target);
        }

        private void EnsureKnownType(string? typeName, bool allowVoid = false, bool allowLet = false)
        {
            if (typeName is null) return;
            if (allowVoid && typeName == "void" || allowLet && typeName == "let") return;
            TypeSymbol type = TypeFacts.FromName(typeName);
            if (type is NamedTypeSymbol named &&
                !_typeSymbols.TryLookup(named.Name, out _) &&
                !_builtInClasses.TryGet(named.Name, out _))
                throw new Exception($"Unknown package, interface, or built-in class type '{named.Name}'.");
        }

        /// <summary>Benita مقدار null ندارد؛ بنابراین reference نام‌دار باید هنگام declaration مقدار بگیرد.</summary>
        private static void RequireInitializerForNamedType(string? typeName, ExpressionNode? initializer,
            string variableName)
        {
            if (TypeFacts.FromName(typeName) is NamedTypeSymbol && initializer is null)
                throw new Exception($"Variable '{variableName}' of named type '{typeName}' requires an initializer.");
        }

        /// <summary>از ورود نوع‌های placeholder به symbol table پس از استنتاج let جلوگیری می‌کند.</summary>
        private static void RequireConcreteInferredType(string? typeName, string variableName)
        {
            TypeSymbol type = TypeFacts.FromName(typeName);
            if (!IsConcreteInferredType(type))
                throw new Exception(
                    $"Cannot infer a concrete type for let variable '{variableName}'. Use an explicit type or a typed initializer.");
        }

        private static bool IsConcreteInferredType(TypeSymbol type) => type switch
        {
            PrimitiveTypeSymbol primitive => primitive != Types.Void,
            NamedTypeSymbol => true,
            ArrayTypeSymbol array => IsConcreteInferredType(array.ElementType),
            TaskTypeSymbol task => task.ResultType == Types.Void || IsConcreteInferredType(task.ResultType),
            _ => false
        };

        /// <summary>
        /// Converts a token type to its corresponding string representation.
        /// </summary>
        /// <param name="tokenType">The token type to convert.</param>
        /// <returns>The string representation of the token type.</returns>
        private string? ConvertTokenTypeToString(TokenType tokenType)
        {
            return TypeFacts.FromToken(tokenType).Name;
        }

        /// <summary>
        /// Declares a variable in the given scope.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="variableScope">The scope in which to declare the variable.</param>
        private void DeclareVariable(string name, string? type, Scope variableScope)
        {
            if (variableScope.TryLookupLocal(name, out _))
            {
                throw new Exception($"Variable '{name}' is already declared.");
            }
            variableScope.Declare(new VariableSymbol(name, TypeFacts.FromName(type)));
        }
    }
}



