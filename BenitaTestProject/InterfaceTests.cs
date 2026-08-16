using Benita;

namespace BenitaTestProject;

[TestClass]
public class InterfaceTests
{
    [TestMethod]
    public void InterfaceTypedParameter_DispatchesToConcretePackage()
    {
        const string source = """
            interface Operation {
                func apply(number value) -> number;
            }

            pkg DoubleOperation : Operation {
                public func apply(number value) -> number { return value * 2; }
            }

            pkg TripleOperation : Operation {
                public func apply(number value) -> number { return value * 3; }
            }

            func calculate(Operation operation, number value) -> number {
                return operation.apply(value);
            }

            _main_() {
                Operation operation = new DoubleOperation();
                print(calculate(operation, 4));
                operation = new TripleOperation();
                print(calculate(operation, 4));
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source, optimizeAst: true);
        Assert.AreEqual($"8{Environment.NewLine}12{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Package_CanImplementMultipleInterfaces()
    {
        const string source = """
            interface Reader { func read() -> string; }
            interface Writer { func write(string value) -> string; }
            pkg Channel : Reader, Writer {
                public func read() -> string { return "read"; }
                public func write(string value) -> string { return value; }
            }
            _main_() {
                Reader reader = new Channel();
                Writer writer = new Channel();
                print(reader.read() + writer.write("/write"));
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"read/write{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Interface_CanBeUsedAsFieldAndReturnType()
    {
        const string source = """
            interface Message { func text() -> string; }
            pkg Email : Message { public func text() -> string { return "email"; } }
            pkg Factory { public func create() -> Message { return new Email(); } }
            pkg Client {
                Message message = new Email();
                public func result() -> string { return message.text(); }
            }
            _main_() {
                Factory factory = new Factory();
                Message message = factory.create();
                Client client = new Client();
                print(message.text() + "/" + client.result());
            }
            """;

        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual($"email/email{Environment.NewLine}", output.GetOutput());
    }

    [DataTestMethod]
    [DataRow("interface A { func run() -> void; } pkg B : A { } _main_() {}", "does not implement")]
    [DataRow("interface A { func run(number x) -> void; } pkg B : A { public func run(string x) -> void {} } _main_() {}", "does not match")]
    [DataRow("interface A { func run() -> void; } pkg B : A { func run() -> void {} } _main_() {}", "must be public")]
    [DataRow("pkg B : Missing { } _main_() {}", "Unknown interface")]
    [DataRow("interface A { func run() -> void; func run() -> void; } _main_() {}", "more than once")]
    public void InvalidInterfaceContracts_AreRejected(string source, string expectedMessage)
    {
        SemanticException exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, expectedMessage);
    }

    [TestMethod]
    public void UnrelatedPackage_CannotBeAssignedToInterface()
    {
        const string source = "interface A { func run() -> void; } pkg B { public func run() -> void {} } _main_() { A value = new B(); }";
        SemanticException exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "Type mismatch");
    }
}
