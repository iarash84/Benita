using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>سازگاری معنایی و زمان اجرای توابع داخلی رشته را بررسی می‌کند.</summary>
public class StringFunctionTests
{
    private const string Source = @"import Terminal;
import Text;

_main_() {
    string value = ""  Hello Benita  "";
    Terminal.print(Text.length(value));
    Terminal.print(Text.char_at(value, 2));
    Terminal.print(Text.substring(value, 2, 5));
    Terminal.print(Text.contains(value, ""Benita""));
    Terminal.print(Text.index_of(value, ""Benita""));
    Terminal.print(Text.replace(value, ""Benita"", ""World""));
    string[] parts = Text.split(""one,two,three"", "","");
    Terminal.print(parts[1]);
    Terminal.print(Text.trim(value));
    Terminal.print(Text.to_lower(""BENITA""));
    Terminal.print(Text.to_upper(""benita""));
}";

    [TestMethod]
    public void Exec_WithAllStringFunctions_ProducesExpectedOutput()
    {
        const string expected = "16\r\nH\r\nHello\r\nTrue\r\n8\r\n  Hello World  \r\ntwo\r\nHello Benita\r\nbenita\r\nBENITA\r\n";
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(Source);

        Assert.AreEqual(expected, output.GetOutput());
    }

    [TestMethod]
    public void Check_WithWrongStringFunctionArgumentType_ThrowsSemanticException()
    {
        const string invalidSource = @"import Text;

_main_() {
    number length = Text.length(42);
}";

        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(invalidSource));

        StringAssert.Contains(exception.Message, "Text.length");
    }
}
