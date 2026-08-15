namespace Benita
{
    /// <summary>قرارداد مشترک ارائه‌دهندگان توابع داخلی مفسر را تعریف می‌کند.</summary>
    internal interface IInterpreterClass
    {
        /// <summary>تابع داخلی را با آرگومان‌های ارزیابی‌شده اجرا می‌کند.</summary>
        object HandleFunctionCall(string? functionName, List<object> arguments);
    }
}
