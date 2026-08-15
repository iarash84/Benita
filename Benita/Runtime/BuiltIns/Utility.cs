namespace Benita.itpr_df
{
    /// <summary>توابع عمومی ورودی، خروجی، تبدیل نوع و محاسبات پایه را ارائه می‌کند.</summary>
    public class Utility : BuiltInHandler
    {
        /// <summary>تابع عمومی متناظر با نام دریافتی را اجرا می‌کند.</summary>
        protected override object Execute(string? functionName, List<object> arguments)
        {
            switch (functionName)
            {
                case "print":
                    foreach (var arg in arguments)
                    {
                        Console.WriteLine(arg);
                    }

                    return null;
                case "input":
                    return Console.ReadLine();
                case "to_string":
                {
                    var stringValue = arguments[0].ToString();
                    return stringValue;
                }
                case "to_number":
                {
                    var numberValue = Convert.ToDouble(arguments[0]);
                    return numberValue;
                }
                case "round_number":
                {
                    var numberValue = Convert.ToDouble(arguments[0]);
                    return Math.Round(numberValue);
                }
                case "sqrt_number":
                {
                    var numberValue = Convert.ToDouble(arguments[0]);
                    return Math.Sqrt(numberValue);
                }
                default:
                    throw new Exception($"Unknown utility function '{functionName}'");
            }
        }
    }
}
