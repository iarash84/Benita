using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>حالت بررسی بدون اجرای Compiler و انتشار خطاهای آن را آزمایش می‌کند.</summary>
public class CompilerCheckTests
{
    [TestMethod]
    public void Check_WithValidProgram_CompletesWithoutExecutingProgram()
    {
        const string source = "_main_() { print(\"must not be printed\"); }";
        using var consoleOutput = new ConsoleOutput();

        new CompilerClass().Check(source, sourceName: "valid.ben");

        Assert.AreEqual(string.Empty, consoleOutput.GetOutput());
    }

    [TestMethod]
    public void Check_WithInvalidSyntax_ThrowsParserException()
    {
        const string source = "_main_() { print(\"missing semicolon\") }";

        var exception = Assert.ThrowsException<ParserException>(() =>
            new CompilerClass().Check(source, sourceName: "syntax-error.ben"));

        Assert.AreEqual("BEN2001", exception.Code);
        StringAssert.Contains(exception.Message, "syntax-error.ben:1:");
    }

    [TestMethod]
    public void Check_WithSemanticError_ThrowsSemanticException()
    {
        const string source = "_main_() { missing = 1; }";

        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(source, sourceName: "semantic-error.ben"));

        Assert.AreEqual("BEN3001", exception.Code);
        StringAssert.Contains(exception.Message, "Undeclared variable 'missing'");
    }
}
