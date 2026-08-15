using Benita;

namespace BenitaTestProject;

[TestClass]
public class LanguageSyntaxRegressionTests
{
    [TestMethod]
    public void Check_SyntaxErrorAfterReturn_IsNotSkipped()
    {
        const string source = "func value() -> number { return 1; number broken = ; } _main_() {}";
        Assert.ThrowsException<ParserException>(() => new CompilerClass().Check(source));
    }

    [TestMethod]
    public void Check_NonVoidFunctionWithReturningIfElse_IsValid()
    {
        const string source = @"
func sign(number value) -> number {
    if (value >= 0) { return 1; } else { return -1; }
}
_main_() { print(sign(2)); }";
        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void Check_NonVoidFunctionWithMissingReturnPath_IsRejected()
    {
        const string source = "func value(bool flag) -> number { if (flag) { return 1; } } _main_() {}";
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "must return");
    }

    [DataTestMethod]
    [DataRow("break;")]
    [DataRow("continue;")]
    public void Check_LoopControlOutsideLoop_IsRejected(string statement)
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check($"_main_() {{ {statement} }}"));
        StringAssert.Contains(exception.Message, "inside a loop");
    }

    [TestMethod]
    public void Check_ForIncrement_IsSemanticallyAnalyzed()
    {
        const string source = "_main_() { for (number i = 0; i < 2; missing++) {} }";
        Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
    }

    [TestMethod]
    public void Exec_ForWithEmptyClauses_CanExitWithBreak()
    {
        const string source = "_main_() { for (;;) { print(1); break; } }";
        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual("1\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Check_MainWithParameters_IsRejected()
    {
        Assert.ThrowsException<ParserException>(() =>
            new CompilerClass().Check("_main_(number value) {}"));
    }

    [DataTestMethod]
    [DataRow("_main_() { void value; }")]
    [DataRow("func value() -> let { return 1; } _main_() {}")]
    [DataRow("func value(let input) -> number { return 1; } _main_() {}")]
    public void Check_InvalidTypePositions_AreRejected(string source)
    {
        Assert.ThrowsException<ParserException>(() => new CompilerClass().Check(source));
    }

    [DataTestMethod]
    [DataRow(".5", "BEN1005")]
    [DataRow("true & false", "BEN1001")]
    [DataRow("true | false", "BEN1001")]
    [DataRow("/* unfinished", "BEN1004")]
    [DataRow("\"bad\\q\"", "BEN1006")]
    public void Lexer_InvalidConstruct_ProducesSpecificDiagnostic(string source, string code)
    {
        var exception = Assert.ThrowsException<LexerException>(() => new Lexer(source).Tokenize());
        Assert.AreEqual(code, exception.Code);
    }

    [TestMethod]
    public void Lexer_StringEscapes_AreDecoded()
    {
        List<Token> tokens = new Lexer("\"line\\n\\t\\\"quote\\\"\\\\\"").Tokenize();
        Assert.AreEqual("line\n\t\"quote\"\\", tokens[0].Lexeme);
    }

    [TestMethod]
    public void Check_ReturnOutsideFunction_IsRejected()
    {
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check("return 1;"));
        StringAssert.Contains(exception.Message, "only be used inside a function");
    }

    [DataTestMethod]
    [DataRow("string value = \"x\"; value++;", "Increment and decrement require a number")]
    [DataRow("string value = \"x\"; value += \"y\";", "Compound assignment requires numeric operands")]
    public void Check_NonNumericMutation_IsRejected(string source, string expectedMessage)
    {
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, expectedMessage);
    }
}
