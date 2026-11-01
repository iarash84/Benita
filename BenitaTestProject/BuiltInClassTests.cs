using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
public class BuiltInClassTests
{
    [TestMethod]
    public void BuiltInClassRegistry_RegistersAndLooksUpDefinitions()
    {
        BuiltInClassRegistry registry = new();
        ProbeClass definition = new();

        registry.Register(definition);

        Assert.IsTrue(registry.TryGet("Probe", out BuiltInClassDefinition registered));
        Assert.AreSame(definition, registered);
        Assert.ThrowsException<InvalidOperationException>(() => registry.Register(new ProbeClass()));
    }

    [TestMethod]
    public void BuiltInClass_CreatesInstancesAndSupportsMethodsAndProperties()
    {
        const string source = """
            _main_() {
                Probe probe = new Probe("start");
                print(probe.value);
                probe.value = "changed";
                print(probe.append("!"));
                print(probe.value);
            }
            """;
        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        registry.Register(new ProbeClass());
        using var output = new ConsoleOutput();

        new CompilerClass(registry).Exec(source);

        Assert.AreEqual($"start{Environment.NewLine}changed!{Environment.NewLine}changed!{Environment.NewLine}",
            output.GetOutput());
    }

    [TestMethod]
    public void BuiltInClass_NativeCounterSupportsConstructionAndMutableMembers()
    {
        const string source = """
            _main_() {
                NativeCounter counter = new NativeCounter(2);
                print(counter.value);
                counter.value = 5;
                print(counter.increment(3));
                counter.value += 2;
                print(counter.value);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source, optimizeAst: true);

        Assert.AreEqual($"2{Environment.NewLine}8{Environment.NewLine}10{Environment.NewLine}",
            output.GetOutput());
    }

    [TestMethod]
    public void BuiltInClass_DateTimeSupportsImmutableOperationsAndFormatting()
    {
        const string source = """
            _main_() {
                DateTime initial = new DateTime(0);
                DateTime later = initial.addHours(2.5);
                print(initial.year);
                print(later.timestamp);
                print(initial.differenceInSeconds(later));
                print(initial.isBefore(later));
                print(later.isAfter(initial));
                print(initial.format("yyyy-MM-dd HH:mm:ss"));
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source, optimizeAst: true);

        Assert.AreEqual(
            $"1970{Environment.NewLine}" +
            $"9000{Environment.NewLine}" +
            $"-9000{Environment.NewLine}" +
            $"True{Environment.NewLine}" +
            $"True{Environment.NewLine}" +
            $"1970-01-01 00:00:00{Environment.NewLine}",
            output.GetOutput());
    }

    [TestMethod]
    public void BuiltInClass_FileSupportsTextOperationsAndReadOnlyMetadata()
    {
        string path = Path.Combine(Path.GetTempPath(), $"benita-{Guid.NewGuid():N}.txt");
        string copyPath = path + ".copy";
        try
        {
            string source = $$"""
                _main_() {
                    File file = new File("{{path.Replace("\\", "\\\\")}}");
                    print(file.exists());
                    file.writeText("Hello");
                    file.appendText(" Benita");
                    print(file.exists());
                    print(file.readText());
                    print(file.length > 0);
                    file.copyTo("{{copyPath.Replace("\\", "\\\\")}}");
                    File copy = new File("{{copyPath.Replace("\\", "\\\\")}}");
                    print(copy.exists());
                    file.delete();
                    print(file.exists());
                }
                """;
            using var output = new ConsoleOutput();

            new CompilerClass().Exec(source, optimizeAst: true);

            Assert.AreEqual(
                $"False{Environment.NewLine}" +
                $"True{Environment.NewLine}" +
                $"Hello Benita{Environment.NewLine}" +
                $"True{Environment.NewLine}" +
                $"True{Environment.NewLine}" +
                $"False{Environment.NewLine}",
                output.GetOutput());
        }
        finally
        {
            File.Delete(path);
            File.Delete(copyPath);
        }
    }

    [TestMethod]
    public void BuiltInClass_DateTimeRejectsWritablePropertiesAndInvalidTimestamp()
    {
        SemanticException semanticException = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("_main_() { DateTime value = new DateTime(0); value.year = 2024; }"));

        Assert.AreEqual("BEN3001", semanticException.Code);

        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        Assert.IsTrue(registry.TryGet("DateTime", out BuiltInClassDefinition definition));
        BuiltInException runtimeException = Assert.ThrowsException<BuiltInException>(() =>
            definition.Create([double.PositiveInfinity]));

        Assert.AreEqual("BEN4101", runtimeException.Code);
    }

    [TestMethod]
    public void BuiltInClass_InvalidCallsAndPropertyAccessAreRejectedSemantically()
    {
        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        registry.Register(new ProbeClass());

        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass(registry).Check("_main_() { let counter = new NativeCounter(\"bad\"); }"));
        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass(registry).Check("_main_() { let probe = new Probe(\"x\"); probe.missing; }"));
        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass(registry).Check("_main_() { let probe = new Probe(\"x\"); probe.label = \"no\"; }"));
        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass(registry).Check("pkg NativeCounter {} _main_() {}"));
    }

    [TestMethod]
    public void BuiltInClass_RuntimeConstructorValidationUsesBuiltInDiagnostic()
    {
        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        Assert.IsTrue(registry.TryGet("NativeCounter", out BuiltInClassDefinition definition));

        BuiltInException exception = Assert.ThrowsException<BuiltInException>(() =>
            definition.Create(["not a number"]));

        Assert.AreEqual("BEN4101", exception.Code);
    }

    [TestMethod]
    public void BuiltInClass_RuntimeValidationRejectsWrongArgumentCount()
    {
        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        Assert.IsTrue(registry.TryGet("NativeCounter", out BuiltInClassDefinition definition));

        BuiltInException exception = Assert.ThrowsException<BuiltInException>(() => definition.Create([]));

        Assert.AreEqual("BEN4101", exception.Code);
        StringAssert.Contains(exception.Description, "expects 1 argument");
    }

    [TestMethod]
    public void BuiltInClass_RuntimeValidationRejectsInvalidMethodAndPropertyResults()
    {
        const string source = """
            _main_() {
                BadResult value = new BadResult();
                print(value.invalidMethod());
            }
            """;
        BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
        registry.Register(new BadResultClass());

        BuiltInException exception = Assert.ThrowsException<BuiltInException>(() =>
            new CompilerClass(registry).Exec(source));

        Assert.AreEqual("BEN4101", exception.Code);
        StringAssert.Contains(exception.Description, "returned");
    }

    [TestMethod]
    public void BuiltInClass_SemanticMemberErrorsUseBenitaDiagnostic()
    {
        SemanticException exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("_main_() { let counter = new NativeCounter(1); counter.missing; }"));

        Assert.AreEqual("BEN3001", exception.Code);
    }

    [TestMethod]
    public void BuiltInClass_TaskCaptureClonesMutableInstances()
    {
        const string source = """
            func mutate(NativeCounter counter) -> number {
                return counter.increment(1);
            }
            _main_() {
                NativeCounter counter = new NativeCounter(0);
                let operation = async mutate(counter);
                print(await operation);
                print(counter.value);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source);

        Assert.AreEqual($"1{Environment.NewLine}0{Environment.NewLine}", output.GetOutput());
    }

    private sealed class BadResultClass : BuiltInClassDefinition
    {
        internal BadResultClass()
            : base("BadResult",
                [],
                [new BuiltInMethodDescriptor("invalidMethod", Types.String, [])],
                [new BuiltInPropertyDescriptor("invalidProperty", Types.String)])
        {
        }

        protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
            new BadResultInstance();
    }

    private sealed class BadResultInstance : IBuiltInClassInstance
    {
        public string ClassName => "BadResult";

        public object InvokeMethod(string methodName, IReadOnlyList<object?> arguments) => 42d;

        public object GetProperty(string propertyName) => 42d;

        public void SetProperty(string propertyName, object? value) =>
            throw new BuiltInException("Property is read-only.");

        public IBuiltInClassInstance Clone() => new BadResultInstance();
    }

    private sealed class ProbeClass : BuiltInClassDefinition
    {
        internal ProbeClass()
            : base("Probe",
                [new BuiltInParameter("initialValue", Types.String)],
                [new BuiltInMethodDescriptor("append", Types.String, [Types.String])],
                [
                    new BuiltInPropertyDescriptor("value", Types.String, CanRead: true, CanWrite: true),
                    new BuiltInPropertyDescriptor("label", Types.String, CanRead: true, CanWrite: false)
                ])
        {
        }

        protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
            new ProbeInstance((string)arguments[0]!);
    }

    private sealed class ProbeInstance(string value) : IBuiltInClassInstance
    {
        private string _value = value;

        public string ClassName => "Probe";

        public object InvokeMethod(string methodName, IReadOnlyList<object?> arguments) =>
            methodName switch
            {
                "append" => _value += (string)arguments[0]!,
                _ => throw new BuiltInException($"Class '{ClassName}' has no method named '{methodName}'.")
            };

        public object GetProperty(string propertyName) =>
            propertyName switch
            {
                "value" => _value,
                "label" => "probe",
                _ => throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.")
            };

        public void SetProperty(string propertyName, object? value)
        {
            if (propertyName != "value")
                throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.");
            _value = (string)value!;
        }

        public IBuiltInClassInstance Clone() => new ProbeInstance(_value);
    }
}
