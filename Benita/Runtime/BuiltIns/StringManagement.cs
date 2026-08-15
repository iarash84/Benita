namespace Benita.itpr_df
{
    /// <summary>توابع داخلی پردازش رشته را با رفتار مستقل از فرهنگ سیستم اجرا می‌کند.</summary>
    public class StringManagement : BuiltInHandler
    {
        /// <summary>تابع رشته‌ای متناظر با نام دریافتی را اجرا می‌کند.</summary>
        protected override object Execute(string? functionName, List<object> arguments)
        {
            string value = Convert.ToString(arguments[0]) ?? string.Empty;

            return functionName switch
            {
                "string_len" => value.Length,
                "string_char_at" => StringCharAt(value, Convert.ToInt32(arguments[1])),
                "string_substring" => value.Substring(
                    Convert.ToInt32(arguments[1]), Convert.ToInt32(arguments[2])),
                "string_contains" => value.Contains(
                    Convert.ToString(arguments[1]) ?? string.Empty, StringComparison.Ordinal),
                "string_index_of" => value.IndexOf(
                    Convert.ToString(arguments[1]) ?? string.Empty, StringComparison.Ordinal),
                "string_replace" => value.Replace(
                    Convert.ToString(arguments[1]) ?? string.Empty,
                    Convert.ToString(arguments[2]) ?? string.Empty,
                    StringComparison.Ordinal),
                "string_split" => StringSplit(value, Convert.ToString(arguments[1]) ?? string.Empty),
                "string_trim" => value.Trim(),
                "string_to_lower" => value.ToLowerInvariant(),
                "string_to_upper" => value.ToUpperInvariant(),
                _ => throw new Exception($"Unknown string function '{functionName}'")
            };
        }

        private static string StringCharAt(string value, int index)
        {
            if (index < 0 || index >= value.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "String index is out of range.");

            return value[index].ToString();
        }

        private static object[] StringSplit(string value, string separator)
        {
            if (separator.Length == 0)
                return new object[] { value };

            return value.Split(new[] { separator }, StringSplitOptions.None)
                .Cast<object>()
                .ToArray();
        }
    }
}
