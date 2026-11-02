using Benita;

namespace BenitaTestProject;

[TestClass]
public class BoundProgramTests
{
    [TestMethod]
    public void Factory_CreatesBoundIdentifierWithResolvedSymbol()
    {
        const string source = "number value = 7; func read() -> number { return value; } _main_() { read(); }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        BoundProgram bound = BoundProgramFactory.Create(program);
        IdentifierNode identifier = (IdentifierNode)program.Functions[0].ReturnStatement!.ReturnExpression!;

        Assert.IsTrue(bound.TryBindIdentifier(identifier, out BoundIdentifier? resolved));
        Assert.AreEqual(SymbolKind.Variable, resolved!.Symbol.Kind);
        Assert.AreEqual(Types.Number, resolved.Type);
    }

    [TestMethod]
    public void Factory_CreatesBoundFunctionCallWithResolvedSignature()
    {
        const string source = "func echo(number value) -> number { return value; } _main_() { echo(1); }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        BoundProgram bound = BoundProgramFactory.Create(program);
        FunctionCallNode call = (FunctionCallNode)((ExpressionStatementNode)
            program.MainFunction!.Body.Statements[0]).Expression!;

        Assert.IsTrue(bound.TryBindFunctionCall(call, out BoundFunctionCall? resolved));
        Assert.AreEqual("echo", resolved!.Function.Name);
        Assert.AreEqual(Types.Number, resolved.ReturnType);
        CollectionAssert.AreEqual(new[] { Types.Number }, resolved.ParameterTypes.ToArray());
    }

    [TestMethod]
    public void BoundProgram_DoesNotChangeRuntimeExecutionPath()
    {
        const string source = "import Terminal; number value = 7; _main_() { Terminal.print(value); }";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        BoundProgram bound = BoundProgramFactory.Create(program);
        Assert.AreSame(program, bound.SyntaxTree);

        using var output = new ConsoleOutput();
        new Interpreter().Visit(bound.SyntaxTree);

        Assert.AreEqual($"7{Environment.NewLine}", output.GetOutput());
    }
}
