using Benita;

namespace BenitaTestProject;

/// <summary>
/// تطابق resolution معنایی با lookup و assignment واقعی runtime را بررسی می‌کند.
/// </summary>
[TestClass]
public class SymbolScopeIntegrationTests
{
    [TestMethod]
    public void GlobalSymbol_IsResolvedBySemanticAnalyzerAndRuntimeFunctionLookup()
    {
        const string source = """
            import Terminal;
            number shared = 7;
            func read_shared() -> number { return shared; }
            _main_() { Terminal.print(read_shared()); }
            """;

        var compiler = new CompilerClass();
        compiler.Check(source);

        using var output = new ConsoleOutput();
        compiler.Exec(source);

        Assert.AreEqual($"7{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void BlockSymbol_ShadowingIsAcceptedSemanticallyAndRestoredAtRuntime()
    {
        const string source = """
            import Terminal;
            _main_() {
                number value = 1;
                if (true) {
                    number value = 2;
                    Terminal.print(value);
                }
                Terminal.print(value);
            }
            """;

        var compiler = new CompilerClass();
        compiler.Check(source);

        using var output = new ConsoleOutput();
        compiler.Exec(source);

        Assert.AreEqual($"2{Environment.NewLine}1{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void FunctionAssignment_ResolvesOuterSymbolAndUpdatesRuntimeParentScope()
    {
        const string source = """
            import Terminal;
            number counter = 1;
            func increment() -> void {
                counter++;
            }
            _main_() {
                increment();
                Terminal.print(counter);
            }
            """;

        var compiler = new CompilerClass();
        compiler.Check(source);

        using var output = new ConsoleOutput();
        compiler.Exec(source);

        Assert.AreEqual($"2{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void FunctionParameter_ShadowingDoesNotMutateOuterSymbolOrRuntimeValue()
    {
        const string source = """
            import Terminal;
            number value = 10;
            func read(number value) -> number { return value; }
            _main_() {
                Terminal.print(read(4));
                Terminal.print(value);
            }
            """;

        var compiler = new CompilerClass();
        compiler.Check(source);

        using var output = new ConsoleOutput();
        compiler.Exec(source);

        Assert.AreEqual($"4{Environment.NewLine}10{Environment.NewLine}", output.GetOutput());
    }
}
