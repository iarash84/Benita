using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
public class ErrorHandlingTests
{
    [TestMethod]
    public void ThrowCatchFinally_ExposeErrorAndAlwaysRunCleanup()
    {
        const string source = """
            _main_() {
                try {
                    throw error("APP100", "operation failed");
                } catch (failure) {
                    print(failure.code);
                    print(failure.message);
                } finally {
                    print("cleanup");
                }
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source, optimizeAst: true);
        Assert.AreEqual($"APP100{Environment.NewLine}operation failed{Environment.NewLine}cleanup{Environment.NewLine}",
            output.GetOutput());
    }

    [TestMethod]
    public void Catch_ConvertsBuiltInRuntimeFailureToErrorValue()
    {
        const string source = """
            _main_() {
                try {
                    number value = to_number("not-a-number");
                } catch (failure) {
                    print(failure.code);
                }
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"BEN4101{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Finally_RunsBeforeFunctionReturn()
    {
        const string source = """
            func value() -> number {
                try { return 7; }
                finally { print("finally"); }
            }
            _main_() { print(value()); }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"finally{Environment.NewLine}7{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Finally_RunsWhenErrorContinuesToOuterCatch()
    {
        const string source = """
            _main_() {
                try {
                    try { throw error("INNER", "failed"); }
                    finally { print("inner cleanup"); }
                } catch (failure) {
                    print(failure.code);
                }
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"inner cleanup{Environment.NewLine}INNER{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void CatchVariable_ShadowsAndThenRestoresOuterVariable()
    {
        const string source = """
            _main_() {
                string failure = "outer";
                try { throw error("INNER", "failed"); }
                catch (failure) { print(failure.code); }
                print(failure);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"INNER{Environment.NewLine}outer{Environment.NewLine}", output.GetOutput());
    }

    [DataTestMethod]
    [DataRow("_main_() { throw \"bad\"; }", "requires an error value")]
    [DataRow("_main_() { try { print(1); } }", "requires a catch or finally")]
    [DataRow("_main_() { try {} catch (failure) {} print(failure.message); }", "Member access requires")]
    [DataRow("func value() -> number { try { return 1; } finally { return 2; } } _main_() {}", "return")]
    [DataRow("_main_() { while (true) { try {} finally { break; } } }", "break")]
    [DataRow("pkg error {} _main_() {}", "reserved")]
    [DataRow("interface error {} _main_() {}", "reserved")]
    [DataRow("func error(string code, string message) -> error { return error(code, message); } _main_() {}", "reserved")]
    public void InvalidErrorHandling_IsRejected(string source, string expectedMessage)
    {
        try
        {
            new CompilerClass().Check(source);
            Assert.Fail("Expected invalid error-handling syntax or semantics to be rejected.");
        }
        catch (BenitaException exception)
        {
            StringAssert.Contains(exception.Message, expectedMessage);
        }
    }

    [TestMethod]
    public void Finally_AllowsLoopControlThatStaysInsideFinally()
    {
        const string source = "_main_() { try {} finally { while (true) { break; } } }";
        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void UncaughtError_PreservesUserCodeAtProgramBoundary()
    {
        UnhandledErrorException exception = Assert.ThrowsException<UnhandledErrorException>(() =>
            new CompilerClass().Exec("_main_() { throw error(\"APP900\", \"unhandled\"); }"));

        Assert.AreEqual("APP900", exception.Code);
        Assert.AreEqual("unhandled", exception.Description);
    }
}
