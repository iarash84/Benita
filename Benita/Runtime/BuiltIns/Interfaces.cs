namespace Benita
{
    /// <summary>قرارداد مشترک ارائه‌دهندگان توابع داخلی مفسر را تعریف می‌کند.</summary>
    public interface IInterpreterClass
    {
        /// <summary>تابع داخلی را با آرگومان‌های ارزیابی‌شده اجرا می‌کند.</summary>
        object HandleFunctionCall(string? functionName, List<object> arguments);
    }

    /// <summary>خطاهای .NET تولیدشده در built-inها را به خطای پایدار BEN4101 تبدیل می‌کند.</summary>
    public abstract class BuiltInHandler : IInterpreterClass
    {
        public object HandleFunctionCall(string? functionName, List<object> arguments)
        {
            try
            {
                return Execute(functionName, arguments);
            }
            catch (BuiltInException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new BuiltInException(exception.Message, exception);
            }
        }

        protected abstract object Execute(string? functionName, List<object> arguments);
    }
}
