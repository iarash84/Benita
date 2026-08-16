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

    [TestMethod]
    public void FunctionCall_WhenGlobalVariableIsAssigned_PreservesTheMutation()
    {
        const string source = """
            number counter = 0;
            func set_counter() -> void {
                counter = 7;
            }
            _main_() {
                set_counter();
                print(counter);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source);

        Assert.AreEqual($"7{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void NestedFunctionCalls_WhenGlobalVariableIsAssigned_PreserveTheFinalMutation()
    {
        const string source = """
            number counter = 0;
            func increment() -> void {
                counter += 1;
            }
            func increment_twice() -> void {
                increment();
                increment();
            }
            _main_() {
                increment_twice();
                print(counter);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source);

        Assert.AreEqual($"2{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void CompilerExec_WhenArrayIndexIsInvalid_NormalizesItToRuntimeDiagnostic()
    {
        const string source = "_main_() { number[] values = [1]; values[3] = 2; }";

        RuntimeException exception = Assert.ThrowsException<RuntimeException>(() =>
            new CompilerClass().Exec(source));

        Assert.AreEqual("BEN4001", exception.Code);
        Assert.IsInstanceOfType<ArgumentOutOfRangeException>(exception.InnerException);
        StringAssert.Contains(exception.Description, "out of range");
    }

    [TestMethod]
    public void CompilerExec_WhenRuntimeThrowsBenitaException_PreservesItsSpecificTypeAndCode()
    {
        const string source = "_main_() { number[] values = [1]; values = array_remove(values, 9); }";

        BuiltInException exception = Assert.ThrowsException<BuiltInException>(() =>
            new CompilerClass().Exec(source));

        Assert.AreEqual("BEN4101", exception.Code);
    }
}
