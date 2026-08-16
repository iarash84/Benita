using Benita;

namespace BenitaTestProject;

[TestClass]
public class StringFunctionTests
{
    private const string Source = @"
_main_() {
    string value = ""  Hello Benita  "";
    print(string_len(value));
    print(string_char_at(value, 2));
    print(string_substring(value, 2, 5));
    print(string_contains(value, ""Benita""));
    print(string_index_of(value, ""Benita""));
    print(string_replace(value, ""Benita"", ""World""));
    string[] parts = string_split(""one,two,three"", "","");
    print(parts[1]);
    print(string_trim(value));
    print(string_to_lower(""BENITA""));
    print(string_to_upper(""benita""));
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
    number length = string_len(42);
}";

        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(invalidSource));

        StringAssert.Contains(exception.Message, "string_len");
    }
}
