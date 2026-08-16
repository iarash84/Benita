using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>کد، متن و موقعیت مبدأ عیب‌یابی‌های مراحل کامپایل را بررسی می‌کند.</summary>
public class DiagnosticTests
{
    [TestMethod]
    public void Tokenize_WithUnexpectedCharacter_ReportsCodeLocationAndSourceLine()
    {
        const string source = "_main_() {\n    @\n}";

        var exception = Assert.ThrowsException<LexerException>(() =>
            new Lexer(source, sourceName: "sample.ben").Tokenize());

        Assert.AreEqual("BEN1001", exception.Code);
        Assert.AreEqual(2, exception.Span.Line);
        Assert.AreEqual(5, exception.Span.Column);
        StringAssert.Contains(exception.Message, "sample.ben:2:5");
        StringAssert.Contains(exception.Message, "    @");
        StringAssert.Contains(exception.Message, "    ^");
    }

    [TestMethod]
    public void Parse_WithMissingSemicolon_ReportsTokenLocationAndExpectedSyntax()
    {
        const string source = "_main_() { print(\"hello\") }";
        var tokens = new Lexer(source, sourceName: "main.ben").Tokenize();

        var exception = Assert.ThrowsException<ParserException>(() => new Parser(tokens).Parse());

        Assert.AreEqual("BEN2001", exception.Code);
        StringAssert.Contains(exception.Message, "main.ben:1:");
        StringAssert.Contains(exception.Message, "Expected ';'");
        StringAssert.Contains(exception.Message, "^");
    }

    [TestMethod]
    public void Compile_WithSemanticError_ReportsSemanticDiagnosticCode()
    {
        const string source = "_main_() { x = 1; }";

        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Exec(source, sourceName: "semantic.ben"));

        Assert.AreEqual("BEN3001", exception.Code);
        StringAssert.Contains(exception.Message, "Undeclared variable 'x'");
    }

    [TestMethod]
    public void Compile_WithNestedSemanticError_PointsToFailingOperand()
    {
        const string source = "_main_() {\n    number value = 1 + missing;\n}";

        SemanticException exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(source, sourceName: "nested.ben"));

        Assert.AreEqual(2, exception.Span.Line);
        Assert.AreEqual(24, exception.Span.Column);
        StringAssert.Contains(exception.Message, "nested.ben:2:24");
        StringAssert.Contains(exception.Message, "                       ^^^^^^^");
    }
}
