namespace Benita;

/// <summary>کلاس داخلی نمونه برای اعتبارسنجی قرارداد ثبت و اجرای built-in classها.</summary>
internal sealed class NativeCounterClass : BuiltInClassDefinition
{
    internal NativeCounterClass()
        : base("NativeCounter",
            [new BuiltInParameter("initialValue", Types.Number)],
            [new BuiltInMethodDescriptor("increment", Types.Number, [Types.Number])],
            [new BuiltInPropertyDescriptor("value", Types.Number, CanRead: true, CanWrite: true)])
    {
    }

    protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
        new NativeCounterInstance(RuntimeValue.From(arguments[0]).Require<double>("NativeCounter constructor"));
}

internal sealed class NativeCounterInstance(double value) : IBuiltInClassInstance
{
    private double _value = value;

    public string ClassName => "NativeCounter";

    public object InvokeMethod(string methodName, IReadOnlyList<object?> arguments) =>
        methodName switch
        {
            "increment" => _value += RuntimeValue.From(arguments[0]).Require<double>("NativeCounter.increment"),
            _ => throw new BuiltInException($"Class '{ClassName}' has no method named '{methodName}'.")
        };

    public object GetProperty(string propertyName) =>
        propertyName == "value"
            ? _value
            : throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.");

    public void SetProperty(string propertyName, object? value)
    {
        if (propertyName != "value")
            throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.");
        _value = RuntimeValue.From(value).Require<double>("NativeCounter.value");
    }

    public IBuiltInClassInstance Clone() => new NativeCounterInstance(_value);
}
