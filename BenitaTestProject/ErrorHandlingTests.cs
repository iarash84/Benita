using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
/// <summary>رفتار `throw`، `catch` و `finally` را در تحلیل و زمان اجرا بررسی می‌کند.</summary>
public class ErrorHandlingTests
{
    [TestMethod]
    public void ThrowCatchFinally_ExposeErrorAndAlwaysRunCleanup()
    {
        const string source = """
            import Terminal;
            import Error;
            _main_() {
                try {
                    throw Error.create("APP100", "operation failed");
                } catch (failure) {
                    Terminal.print(failure.code);
                    Terminal.print(failure.message);
                } finally {
                    Terminal.print("cleanup");
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
            import Terminal;
            import Convert;
            _main_() {
                try {
                    number value = Convert.to_number("not-a-number");
                } catch (failure) {
                    Terminal.print(failure.code);
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
            import Terminal;
            func value() -> number {
                try { return 7; }
                finally { Terminal.print("finally"); }
            }
            _main_() { Terminal.print(value()); }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"finally{Environment.NewLine}7{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void FinallyThatAlwaysThrows_SatisfiesNonVoidControlFlow()
    {
        const string source = """
            import Terminal;
            import Error;
            func value() -> number {
                try { Terminal.print("work"); }
                finally { throw Error.create("STOP", "stopped"); }
            }
            _main_() { value(); }
            """;

        new CompilerClass().Check(source);
        var exception = Assert.ThrowsException<UnhandledErrorException>(() =>
            new CompilerClass().Exec(source));
        Assert.AreEqual("STOP", exception.Code);
    }

    [TestMethod]
    public void Finally_RunsWhenErrorContinuesToOuterCatch()
    {
        const string source = """
            import Terminal;
            import Error;
            _main_() {
                try {
                    try { throw Error.create("INNER", "failed"); }
                    finally { Terminal.print("inner cleanup"); }
                } catch (failure) {
                    Terminal.print(failure.code);
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
            import Terminal;
            import Error;
            _main_() {
                string failure = "outer";
                try { throw Error.create("INNER", "failed"); }
                catch (failure) { Terminal.print(failure.code); }
                Terminal.print(failure);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"INNER{Environment.NewLine}outer{Environment.NewLine}", output.GetOutput());
    }

    [DataTestMethod]
    [DataRow("_main_() { throw \"bad\"; }", "requires an error value")]
    [DataRow("_main_() { try { Terminal.print(1); } }", "requires a catch or finally")]
    [DataRow("_main_() { try {} catch (failure) {} Terminal.print(failure.message); }", "Member access requires")]
    [DataRow("func value() -> number { try { return 1; } finally { return 2; } } _main_() {}", "return")]
    [DataRow("_main_() { while (true) { try {} finally { break; } } }", "break")]
    [DataRow("pkg error {} _main_() {}", "reserved")]
    [DataRow("interface error {} _main_() {}", "reserved")]
    [DataRow("func error(string code, string message) -> error { return Error.create(code, message); } _main_() {}", "reserved")]
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
            new CompilerClass().Exec("import Error; _main_() { throw Error.create(\"APP900\", \"unhandled\"); }"));

        Assert.AreEqual("APP900", exception.Code);
        Assert.AreEqual("unhandled", exception.Description);
    }
}
