using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
/// <summary>ثبت دامنه، جداسازی state و انتشار نتیجه یا خطای taskها را بررسی می‌کند.</summary>
public class AsyncAwaitTests
{
    [TestMethod]
    public void AsyncPackageArgument_IsDeeplyIsolatedFromCaller()
    {
        const string source = """
            pkg Box {
                public number value = 0;
                public func set(number next) -> void { value = next; }
            }

            func mutate(Box box) -> number {
                box.set(9);
                return box.value;
            }

            _main_() {
                Box box = new Box();
                let operation = async mutate(box);
                print(await operation);
                print(box.value);
            }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual($"9{Environment.NewLine}0{Environment.NewLine}", output.GetOutput());
        }
    }

    [TestMethod]
    public void AsyncCalls_CanBeAwaitedInAnyOrder()
    {
        const string source = """
            func add(number left, number right) -> number { return left + right; }
            _main_() {
                let first = async add(10, 20);
                let second = async add(1, 2);
                print(await second);
                print(await first);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source, optimizeAst: true);
        Assert.AreEqual($"3{Environment.NewLine}30{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Await_CanReadCompletedTaskMoreThanOnce()
    {
        const string source = """
            func value() -> number { return 42; }
            _main_() {
                let operation = async value();
                print(await operation);
                print(await operation);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"42{Environment.NewLine}42{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Await_PropagatesTaskErrorToCatch()
    {
        const string source = """
            func fail() -> number { throw error("ASYNC100", "task failed"); }
            _main_() {
                let operation = async fail();
                try { number result = await operation; }
                catch (failure) {
                    print(failure.code);
                    print(failure.message);
                }
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"ASYNC100{Environment.NewLine}task failed{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Async_CapturesArgumentBeforeTaskStarts()
    {
        const string source = """
            func identity(number value) -> number { return value; }
            _main_() {
                number value = 7;
                let operation = async identity(value);
                value = 9;
                print(await operation);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"7{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void AwaitingNonTask_IsRejected()
    {
        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("_main_() { number value = await 1; }"));
    }

    [TestMethod]
    public void AsyncWithoutFunctionCall_IsRejected()
    {
        Assert.ThrowsException<ParserException>(() =>
            new CompilerClass().Check("_main_() { let value = async 1; }"));
    }
}
