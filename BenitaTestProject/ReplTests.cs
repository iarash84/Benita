using Benita;

namespace BenitaTestProject;

[TestClass]
public class ReplTests
{
    [TestMethod]
    public void Run_PreservesVariablesBetweenSubmissionsAndPrintsExpressions()
    {
        const string input = "number value = 40;\nvalue = value + 2;\nvalue\n:exit\n";
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        StringAssert.Contains(consoleOutput.GetOuput(), "42\r\n");
    }

    [TestMethod]
    public void Run_PreservesMultilineFunctionsBetweenSubmissions()
    {
        const string input = "func double(number value) -> number {\nreturn value * 2;\n}\ndouble(6)\n:exit\n";
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        StringAssert.Contains(consoleOutput.GetOuput(), "12\r\n");
    }

    [TestMethod]
    public void Run_ResetClearsHistoryAndReportsReset()
    {
        const string input = "number value = 1;\n:reset\n:history\n:exit\n";
        var output = new StringWriter();

        new Repl(new StringReader(input), output).Run();

        StringAssert.Contains(output.ToString(), "Session reset.");
        Assert.IsFalse(output.ToString().Contains("1: number value", StringComparison.Ordinal));
    }
}
