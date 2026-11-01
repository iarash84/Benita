using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>سازگاری معنایی و زمان اجرای توابع داخلی رشته را بررسی می‌کند.</summary>
public class StringFunctionTests
{
    private const string Source = @"
_main_() {
    string value = ""  Hello Benita  "";
    Console.print(String.length(value));
    Console.print(String.char_at(value, 2));
    Console.print(String.substring(value, 2, 5));
    Console.print(String.contains(value, ""Benita""));
    Console.print(String.index_of(value, ""Benita""));
    Console.print(String.replace(value, ""Benita"", ""World""));
    string[] parts = String.split(""one,two,three"", "","");
    Console.print(parts[1]);
    Console.print(String.trim(value));
    Console.print(String.to_lower(""BENITA""));
    Console.print(String.to_upper(""benita""));
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
        const string invalidSource = @"
_main_() {
    number length = String.length(42);
}";

        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(invalidSource));

        StringAssert.Contains(exception.Message, "String.length");
    }
}
