using Benita;

namespace BenitaTestProject;

[TestClass]
public class ObjectModelTests
{
    [TestMethod]
    public void InitAndThis_CreateInitializedInstance()
    {
        const string source = """
            pkg Person {
                string name = "";

                init(string value) {
                    this.name = value;
                }

                public func describe() -> string {
                    return name;
                }
            }

            _main_() {
                Person person = new Person("Benita");
                print(person.describe());
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source, optimizeAst: true);
        Assert.AreEqual($"Benita{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void NewExpression_CanBeReturnedAndStoredWithLet()
    {
        const string source = """
            pkg Message {
                public string text = "";
                init(string value) { this.text = value; }
            }

            func create_message() -> Message {
                return new Message("hello");
            }

            _main_() {
                let message = create_message();
                print(message.text);
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"hello{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void PackageFields_SupportComposition()
    {
        const string source = """
            pkg Engine {
                public string state = "ready";
                init() { }
            }

            pkg Car {
                Engine engine = new Engine();
                init() { }
                public func status() -> string { return engine.state; }
            }

            _main_() {
                Car car = new Car();
                print(car.status());
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"ready{Environment.NewLine}", output.GetOutput());
    }

    [DataTestMethod]
    [DataRow("pkg A { init(number value) { } } _main_() { A a = new A(\"bad\"); }")]
    [DataRow("pkg A { init() { } } _main_() { A a = new A(); print(a.missing); }")]
    [DataRow("_main_() { let value = new Missing(); }")]
    [DataRow("_main_() { Missing value; }")]
    public void InvalidObjectOperations_AreRejected(string source)
    {
        Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
    }
}
