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

        StringAssert.Contains(consoleOutput.GetOutput(), "42\r\n");
    }

    [TestMethod]
    public void Run_PreservesMultilineFunctionsBetweenSubmissions()
    {
        const string input = "func double(number value) -> number {\nreturn value * 2;\n}\ndouble(6)\n:exit\n";
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        StringAssert.Contains(consoleOutput.GetOutput(), "12\r\n");
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

    [TestMethod]
    public void Run_BlankLineSubmitsIncompleteBlockAndRecoversAfterSyntaxError()
    {
        const string input = "for (number i = 0; i < 2; i++) {\nprint(i);\n\n1 + 1\n:exit\n";
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        StringAssert.Contains(consoleOutput.GetOutput(), "BEN2");
        StringAssert.Contains(consoleOutput.GetOutput(), "2\r\n");
    }

    [TestMethod]
    public void Run_CancelAbandonsMultilineSubmissionAndContinuesSession()
    {
        const string input = "if (true) {\n:cancel\n3 + 4\n:exit\n";
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        StringAssert.Contains(consoleOutput.GetOutput(), "Current submission cancelled.");
        StringAssert.Contains(consoleOutput.GetOutput(), "7\r\n");
    }

    [TestMethod]
    public void Run_RuntimeFailureRollsBackScalarAndArrayMutations()
    {
        const string input = """
            number value = 1;
            number[] items = [1];
            value = 2; items[0] = 9; print(items[5]);
            value
            items[0]
            :exit
            """;
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        string output = consoleOutput.GetOutput();
        StringAssert.Contains(output, "BEN4001");
        StringAssert.Contains(output, $"1{Environment.NewLine}");
        Assert.IsFalse(output.Contains($"2{Environment.NewLine}", StringComparison.Ordinal));
        Assert.IsFalse(output.Contains($"9{Environment.NewLine}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Run_RuntimeFailureRollsBackPackageInstanceMutation()
    {
        const string input = """
            pkg Counter {
            number value = 1;
            public func set(number next) -> void { value = next; }
            public func current() -> number { return value; }
            }
            Counter counter = new Counter();
            counter.set(9); number[] items = [1]; print(items[5]);
            counter.current()
            :exit
            """;
        using var consoleOutput = new ConsoleOutput();

        new Repl(new StringReader(input), Console.Out).Run();

        string output = consoleOutput.GetOutput();
        StringAssert.Contains(output, "BEN4001");
        StringAssert.Contains(output, $"1{Environment.NewLine}");
        Assert.IsFalse(output.Contains($"9{Environment.NewLine}", StringComparison.Ordinal));
    }
}
