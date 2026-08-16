using Benita;

namespace BenitaTestProject;

[TestClass]
public class ObjectModelTests
{
    [TestMethod]
    public void PackageFieldInitializer_CanReferenceOnlyPreviouslyInitializedFields()
    {
        const string validSource = """
            pkg Values {
                number first = 1;
                public number second = first + 1;
            }
            _main_() { Values values = new Values(); print(values.second); }
            """;
        const string invalidSource = """
            pkg Values {
                number first = second;
                number second = 2;
            }
            _main_() { Values values = new Values(); }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(validSource, optimizeAst: optimize);
            Assert.AreEqual($"2{Environment.NewLine}", output.GetOutput());
        }

        SemanticException exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(invalidSource));
        StringAssert.Contains(exception.Message, "Undeclared variable 'second'");
    }

    [TestMethod]
    public void PackageAndGlobalSymbols_AreVisibleAndSynchronizedAcrossRuntimeScopes()
    {
        const string source = """
            number seed = 7;
            number value = 10;
            func initial() -> number { return seed; }

            pkg Box {
                public number value = initial();
                init() { seed = seed + 1; }
                public func readSeed() -> number { return seed; }
                public func incrementSeed() -> void { seed = seed + 1; }
                public func incrementField() -> void { value = value + 1; }
            }

            Box globalBox = new Box();
            _main_() {
                print(globalBox.value);
                print(globalBox.readSeed());
                globalBox.incrementSeed();
                globalBox.incrementField();
                print(seed);
                print(value);
                print(globalBox.value);
            }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual(
                $"7{Environment.NewLine}8{Environment.NewLine}9{Environment.NewLine}" +
                $"10{Environment.NewLine}8{Environment.NewLine}", output.GetOutput());
        }
    }

    [TestMethod]
    public void ExternalMemberInputs_AreEvaluatedInCallerScope()
    {
        const string source = """
            pkg Box {
                public number value = 0;
                public func set(number next) -> void { value = next; }
            }
            _main_() {
                Box box = new Box();
                number value = 9;
                box.set(value);
                print(box.value);
                value = 12;
                box.value = value;
                print(box.value);
            }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual($"9{Environment.NewLine}12{Environment.NewLine}", output.GetOutput());
        }
    }

    [TestMethod]
    public void PackageLetField_InfersCompositeInitializerType()
    {
        const string source = """
            pkg Counter {
                let value = 1 + 1;
                public func next() -> number { return value + 1; }
            }
            _main_() { Counter counter = new Counter(); print(counter.next()); }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual($"3{Environment.NewLine}", output.GetOutput());
        }
    }

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

    [TestMethod]
    public void DefaultPrivateGlobalPackageVariable_IsVisibleToFunctionsAndMain()
    {
        const string source = """
            pkg Value {
                public number amount = 4;
            }

            Value value = new Value();
            func read() -> number { return value.amount; }
            _main_() { print(read()); print(value.amount); }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual($"4{Environment.NewLine}4{Environment.NewLine}", output.GetOutput());
        }
    }

    [TestMethod]
    public void FieldInitializer_CanCallMethodDeclaredLater()
    {
        const string source = """
            pkg Counter {
                public number value = this.initial_value();
                private func initial_value() -> number { return 7; }
            }

            _main_() {
                Counter counter = new Counter();
                print(counter.value);
            }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual($"7{Environment.NewLine}", output.GetOutput());
        }
    }

    [DataTestMethod]
    [DataRow("pkg A { init(number value) { } } _main_() { A a = new A(\"bad\"); }")]
    [DataRow("pkg A { func A(number value) -> void { } } _main_() { A a = new A(1); }")]
    [DataRow("pkg A { init() { } } _main_() { A a = new A(); print(a.missing); }")]
    [DataRow("_main_() { let value = new Missing(); }")]
    [DataRow("_main_() { Missing value; }")]
    public void InvalidObjectOperations_AreRejected(string source)
    {
        Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
    }

    [DataTestMethod]
    [DataRow("pkg Counter {} Counter counter; _main_() {}")]
    [DataRow("pkg Counter {} _main_() { Counter counter; }")]
    [DataRow("pkg Counter {} pkg Holder { Counter counter; } _main_() {}")]
    public void NamedTypeVariableWithoutInitializer_IsRejectedBeforeRuntime(string source)
    {
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "requires an initializer");
    }
}
