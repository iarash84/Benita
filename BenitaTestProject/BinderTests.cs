using Benita;

namespace BenitaTestProject;

[TestClass]
public class BinderTests
{
    [TestMethod]
    public void Bind_ResolvesGlobalIdentifierToItsDeclarationSymbol()
    {
        const string source = "number value = 7; func read() -> number { return value; } _main_() { read(); }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();
        Binder binder = new();

        BindingResult result = binder.Bind(program);
        IdentifierNode identifier = (IdentifierNode)program.Functions[0].ReturnStatement!.ReturnExpression!;

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.TryGetSymbol(identifier, out Symbol? symbol));
        Assert.AreEqual(SymbolKind.Variable, symbol!.Kind);
        Assert.IsTrue(result.TryGetDeclarationSymbol(program.GlobalVariables[0], out Symbol? declaration));
        Assert.AreSame(declaration, symbol);
    }

    [TestMethod]
    public void Bind_ResolvesFunctionCallAndParameterInFunctionScope()
    {
        const string source = "func echo(number value) -> number { return value; } _main_() { echo(1); }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();
        BindingResult result = new Binder().Bind(program);

        FunctionNode function = program.Functions[0];
        IdentifierNode parameterUse = (IdentifierNode)function.ReturnStatement!.ReturnExpression!;
        FunctionCallNode call = (FunctionCallNode)((ExpressionStatementNode)
            program.MainFunction!.Body.Statements[0]).Expression!;

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.TryGetSymbol(parameterUse, out Symbol? parameterSymbol));
        Assert.AreEqual(SymbolKind.Parameter, parameterSymbol!.Kind);
        Assert.IsTrue(result.TryGetSymbol(call, out Symbol? functionSymbol));
        Assert.AreEqual(SymbolKind.Function, functionSymbol!.Kind);
    }

    [TestMethod]
    public void Bind_BlockDeclarationShadowsOuterDeclaration()
    {
        const string source = "_main_() { number value = 1; if (true) { number value = 2; value; } }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();
        BindingResult result = new Binder().Bind(program);

        BlockNode innerBlock = (BlockNode)((IfStatementNode)program.MainFunction!.Body.Statements[1]).ThenBranch!;
        VariableDeclarationNode innerDeclaration = (VariableDeclarationNode)innerBlock.Statements[0];
        IdentifierNode use = (IdentifierNode)((ExpressionStatementNode)innerBlock.Statements[1]).Expression!;

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.TryGetSymbol(use, out Symbol? resolved));
        Assert.IsTrue(result.TryGetDeclarationSymbol(innerDeclaration, out Symbol? declared));
        Assert.AreSame(declared, resolved);
    }

    [TestMethod]
    public void Bind_ReportsUndeclaredIdentifier()
    {
        const string source = "_main_() { missing; }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        BindingResult result = new Binder().Bind(program);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Diagnostics[0].Description, "missing");
    }

    [TestMethod]
    public void Bind_ReportsDuplicateDeclarationInSameScope()
    {
        const string source = "number value = 1; number value = 2; _main_() {}";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        BindingResult result = new Binder().Bind(program);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Diagnostics[0].Description, "value");
    }
}
