namespace Benita.itpr_df;

/// <summary>مقدار استاندارد error را برای استفاده در throw ایجاد می‌کند.</summary>
public sealed class ErrorManagement : BuiltInHandler
{
    /// <summary>تابع داخلی error(code, message) را اجرا می‌کند.</summary>
    protected override object Execute(string? functionName, List<object> arguments) => functionName switch
    {
        "error" => new ErrorValue(arguments[0].ToString()!, arguments[1].ToString()!),
        _ => throw new Exception($"Unknown error function '{functionName}'")
    };
}
