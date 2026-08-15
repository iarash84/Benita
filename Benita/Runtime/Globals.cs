namespace Benita
{
    /// <summary>وضعیت مشترک فعلی متغیرها، توابع و بسته‌های زمان اجرا را نگهداری می‌کند.</summary>
    internal static class Globals
    {
        public static Dictionary<string, object> GlobalVariable = new();
        public static Dictionary<string, FunctionNode> GlobalFunctions = new();
        public static Dictionary<string, PackageNode> PackageList = new();
    }
}
