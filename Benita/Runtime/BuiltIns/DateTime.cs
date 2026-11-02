using System.Globalization;

namespace Benita;

/// <summary>کلاس داخلی immutable برای نمایش یک لحظهٔ زمانی بر پایهٔ Unix timestamp بر حسب ثانیه.</summary>
internal sealed class DateTimeClass : BuiltInClassDefinition
{
    internal DateTimeClass()
        : base(
            "DateTime",
            [new BuiltInParameter("timestamp", Types.Number)],
            [
                new BuiltInMethodDescriptor("addDays", TypeFacts.FromName("DateTime"), [Types.Number]),
                new BuiltInMethodDescriptor("addHours", TypeFacts.FromName("DateTime"), [Types.Number]),
                new BuiltInMethodDescriptor("differenceInSeconds", Types.Number, [TypeFacts.FromName("DateTime")]),
                new BuiltInMethodDescriptor("format", Types.String, [Types.String]),
                new BuiltInMethodDescriptor("isBefore", Types.Bool, [TypeFacts.FromName("DateTime")]),
                new BuiltInMethodDescriptor("isAfter", Types.Bool, [TypeFacts.FromName("DateTime")])
            ],
            [
                new BuiltInPropertyDescriptor("year", Types.Number),
                new BuiltInPropertyDescriptor("month", Types.Number),
                new BuiltInPropertyDescriptor("day", Types.Number),
                new BuiltInPropertyDescriptor("hour", Types.Number),
                new BuiltInPropertyDescriptor("minute", Types.Number),
                new BuiltInPropertyDescriptor("second", Types.Number),
                new BuiltInPropertyDescriptor("timestamp", Types.Number)
            ])
    {
    }

    protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
        new DateTimeInstance(
            RuntimeValue.From(arguments[0]).Require<double>("DateTime constructor"));
}

internal sealed class DateTimeInstance : IBuiltInClassInstance
{
    private readonly DateTimeOffset _value;

    internal DateTimeInstance(double timestamp)
    {
        if (double.IsNaN(timestamp) || double.IsInfinity(timestamp))
            throw new BuiltInException("DateTime timestamp must be finite.");

        try
        {
            _value = DateTimeOffset.FromUnixTimeMilliseconds(
                checked((long)Math.Round(timestamp * 1000, MidpointRounding.ToEven)));
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            throw new BuiltInException("DateTime timestamp is outside the supported range.", exception);
        }
    }

    private DateTimeInstance(DateTimeOffset value) => _value = value;

    public string ClassName => "DateTime";

    public object? InvokeMethod(string methodName, IReadOnlyList<object?> arguments) => methodName switch
    {
        "addDays" => AddDays(arguments),
        "addHours" => AddHours(arguments),
        "differenceInSeconds" => DifferenceInSeconds(arguments),
        "format" => Format(arguments),
        "isBefore" => IsBefore(arguments),
        "isAfter" => IsAfter(arguments),
        _ => throw new BuiltInException($"Class '{ClassName}' has no method named '{methodName}'.")
    };

    public object GetProperty(string propertyName) => propertyName switch
    {
        "year" => (double)_value.Year,
        "month" => (double)_value.Month,
        "day" => (double)_value.Day,
        "hour" => (double)_value.Hour,
        "minute" => (double)_value.Minute,
        "second" => (double)_value.Second,
        "timestamp" => _value.ToUnixTimeMilliseconds() / 1000d,
        _ => throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.")
    };

    public void SetProperty(string propertyName, object? value) =>
        throw new BuiltInException($"Property '{ClassName}.{propertyName}' is read-only.");

    public IBuiltInClassInstance Clone() => new DateTimeInstance(_value);

    private object AddDays(IReadOnlyList<object?> arguments) =>
        CreateFrom(_value.AddDays(RuntimeValue.From(arguments[0]).Require<double>("DateTime.addDays")));

    private object AddHours(IReadOnlyList<object?> arguments) =>
        CreateFrom(_value.AddHours(RuntimeValue.From(arguments[0]).Require<double>("DateTime.addHours")));

    private object DifferenceInSeconds(IReadOnlyList<object?> arguments)
    {
        DateTimeInstance other = RuntimeValue.From(arguments[0])
            .Require<DateTimeInstance>("DateTime.differenceInSeconds");
        return (_value - other._value).TotalSeconds;
    }

    private object Format(IReadOnlyList<object?> arguments)
    {
        string pattern = RuntimeValue.From(arguments[0]).Require<string>("DateTime.format");
        return _value.ToString(pattern, CultureInfo.InvariantCulture);
    }

    private object IsBefore(IReadOnlyList<object?> arguments)
    {
        DateTimeInstance other = RuntimeValue.From(arguments[0])
            .Require<DateTimeInstance>("DateTime.isBefore");
        return _value < other._value;
    }

    private object IsAfter(IReadOnlyList<object?> arguments)
    {
        DateTimeInstance other = RuntimeValue.From(arguments[0])
            .Require<DateTimeInstance>("DateTime.isAfter");
        return _value > other._value;
    }

    private static DateTimeInstance CreateFrom(DateTimeOffset value) =>
        new(value);
}
