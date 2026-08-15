namespace Benita;

/// <summary>وضعیت مستقل یک محیط اجرا شامل متغیرهای سراسری، توابع و بسته‌ها را نگه می‌دارد.</summary>
public sealed class RuntimeContext
{
    public Dictionary<string, object> GlobalVariables { get; } = [];
    public Dictionary<string, FunctionNode> GlobalFunctions { get; } = [];
    public Dictionary<string, PackageNode> Packages { get; } = [];

    /// <summary>تمام وضعیت ثبت‌شده در این محیط اجرا را پاک می‌کند.</summary>
    public void Clear()
    {
        GlobalVariables.Clear();
        GlobalFunctions.Clear();
        Packages.Clear();
    }
}
