using System.Text;
using Benita.Cg_df;

namespace BenitaTestProject.Cg_df;

[TestClass]
public class StringManagementTests
{
    [DataTestMethod]
    [DataRow("string_len", "int string_len(const std::string& value);")]
    [DataRow("string_char_at", "std::string string_char_at(const std::string& value, int index);")]
    [DataRow("string_substring", "std::string string_substring(const std::string& value, int start, int length);")]
    [DataRow("string_contains", "bool string_contains(const std::string& value, const std::string& search);")]
    [DataRow("string_index_of", "int string_index_of(const std::string& value, const std::string& search);")]
    [DataRow("string_replace", "std::string string_replace(const std::string& value, const std::string& oldValue, const std::string& newValue);")]
    [DataRow("string_split", "std::vector<std::string> string_split(const std::string& value, const std::string& separator);")]
    [DataRow("string_trim", "std::string string_trim(const std::string& value);")]
    [DataRow("string_to_lower", "std::string string_to_lower(const std::string& value);")]
    [DataRow("string_to_upper", "std::string string_to_upper(const std::string& value);")]
    public void HandleFunctionCall_WithKnownFunction_AddsHelperOnce(string functionName, string declaration)
    {
        var generator = new CgStringManagement();
        var code = new StringBuilder();
        var functions = new StringBuilder();
        var headers = new StringBuilder();
        var includes = new StringBuilder();

        generator.HandleFunctionCall(functionName, ref code, ref functions, ref headers, ref includes);
        generator.HandleFunctionCall(functionName, ref code, ref functions, ref headers, ref includes);

        Assert.AreEqual(1, CountOccurrences(headers.ToString(), declaration));
        Assert.AreEqual(1, CountOccurrences(functions.ToString(), declaration[..declaration.IndexOf('(')] + "("));
    }

    private static int CountOccurrences(string value, string search)
    {
        int count = 0;
        int index = 0;
        while ((index = value.IndexOf(search, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += search.Length;
        }

        return count;
    }
}
