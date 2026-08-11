namespace BenitaTestProject;

[TestClass]
public class ConsoleOutputTests
{
    [TestMethod]
    public void AssertTextEqualIgnoringLineEndings_WithDifferentLineEndings_Succeeds()
    {
        SharedFunction.AssertTextEqualIgnoringLineEndings(
            "first\r\nsecond\r\n",
            "first\nsecond\n");
    }

    [TestMethod]
    public void GetOutput_WithUnixLineEndings_ReturnsCanonicalLineEndings()
    {
        using var consoleOutput = new ConsoleOutput();
        Console.Write("first\nsecond\n");

        Assert.AreEqual("first\r\nsecond\r\n", consoleOutput.GetOuput());
    }
}
