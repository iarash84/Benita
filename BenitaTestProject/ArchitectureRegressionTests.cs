using Benita;

namespace BenitaTestProject;

[TestClass]
public class ArchitectureRegressionTests
{
    [TestMethod]
    public void Interpreter_WithSeparateRuntimeContexts_DoesNotShareState()
    {
        var firstContext = new RuntimeContext();
        var secondContext = new RuntimeContext();
        var program = new ProgramNode(
            [new VariableDeclarationNode("number", "value", new LiteralNode("42", TokenType.NUMBER_LITERAL))],
            [], [], null,
            [new ExpressionStatementNode(new LiteralNode("true", TokenType.TRUE_LITERAL))]);

        new Interpreter(context: firstContext).Visit(program);

        Assert.AreEqual(42d, firstContext.GlobalVariables["value"]);
        Assert.IsFalse(secondContext.GlobalVariables.ContainsKey("value"));
    }

    [TestMethod]
    public void BuiltInFailure_UsesStableDiagnosticCode()
    {
        const string source = "_main_() { number[] values = [1]; values = array_remove(values, 9); }";

        var exception = Assert.ThrowsException<BuiltInException>(() => new CompilerClass().Exec(source));

        Assert.AreEqual("BEN4101", exception.Code);
    }
}
