namespace Benita.itpr_df
{
    public class Utility : IInterpreterClass
    {
        public object HandleFunctionCall(string? functionName, List<object> arguments)
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