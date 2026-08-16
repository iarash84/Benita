namespace BenitaTestProject;

[TestClass]
/// <summary>یکسان‌سازی خروجی کنسول در سیستم‌عامل‌های مختلف را بررسی می‌کند.</summary>
public class ConsoleOutputTests
{
    [TestMethod]
    public void GetOutput_WithUnixLineEndings_ReturnsCanonicalLineEndings()
    {
        using var consoleOutput = new ConsoleOutput();
        Console.Write("first\nsecond\n");

        Assert.AreEqual("first\r\nsecond\r\n", consoleOutput.GetOutput());
    }
}
