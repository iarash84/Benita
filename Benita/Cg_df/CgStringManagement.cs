using System.Text;

namespace Benita.Cg_df
{
    public class CgStringManagement : ICodeGeneratorClass
    {
        public void HandleFunctionCall(string? functionName, ref StringBuilder code, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader, ref StringBuilder codeInclude)
        {
            switch (functionName)
            {
                case "string_len":
                    AddFunction("int string_len(const std::string& value);", """
                        int string_len(const std::string& value)
                        {
                            return static_cast<int>(value.size());
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_char_at":
                    AddInclude("#include <stdexcept>", ref codeInclude);
                    AddFunction("std::string string_char_at(const std::string& value, int index);", """
                        std::string string_char_at(const std::string& value, int index)
                        {
                            if (index < 0 || index >= static_cast<int>(value.size()))
                                throw std::out_of_range("String index is out of range.");
                            return std::string(1, value[static_cast<std::size_t>(index)]);
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_substring":
                    AddInclude("#include <stdexcept>", ref codeInclude);
                    AddFunction("std::string string_substring(const std::string& value, int start, int length);", """
                        std::string string_substring(const std::string& value, int start, int length)
                        {
                            if (start < 0 || length < 0 || start > static_cast<int>(value.size()) ||
                                length > static_cast<int>(value.size()) - start)
                                throw std::out_of_range("Substring range is out of bounds.");
                            return value.substr(static_cast<std::size_t>(start), static_cast<std::size_t>(length));
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_contains":
                    AddFunction("bool string_contains(const std::string& value, const std::string& search);", """
                        bool string_contains(const std::string& value, const std::string& search)
                        {
                            return value.find(search) != std::string::npos;
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_index_of":
                    AddFunction("int string_index_of(const std::string& value, const std::string& search);", """
                        int string_index_of(const std::string& value, const std::string& search)
                        {
                            std::size_t index = value.find(search);
                            return index == std::string::npos ? -1 : static_cast<int>(index);
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_replace":
                    AddInclude("#include <stdexcept>", ref codeInclude);
                    AddFunction("std::string string_replace(const std::string& value, const std::string& oldValue, const std::string& newValue);", """
                        std::string string_replace(const std::string& value, const std::string& oldValue, const std::string& newValue)
                        {
                            if (oldValue.empty())
                                throw std::invalid_argument("The value to replace cannot be empty.");
                            std::string result = value;
                            std::size_t position = 0;
                            while ((position = result.find(oldValue, position)) != std::string::npos)
                            {
                                result.replace(position, oldValue.size(), newValue);
                                position += newValue.size();
                            }
                            return result;
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_split":
                    AddFunction("std::vector<std::string> string_split(const std::string& value, const std::string& separator);", """
                        std::vector<std::string> string_split(const std::string& value, const std::string& separator)
                        {
                            if (separator.empty())
                                return { value };
                            std::vector<std::string> parts;
                            std::size_t start = 0;
                            std::size_t position;
                            while ((position = value.find(separator, start)) != std::string::npos)
                            {
                                parts.push_back(value.substr(start, position - start));
                                start = position + separator.size();
                            }
                            parts.push_back(value.substr(start));
                            return parts;
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_trim":
                    AddInclude("#include <cctype>", ref codeInclude);
                    AddFunction("std::string string_trim(const std::string& value);", """
                        std::string string_trim(const std::string& value)
                        {
                            std::size_t start = 0;
                            while (start < value.size() && std::isspace(static_cast<unsigned char>(value[start])))
                                ++start;
                            std::size_t end = value.size();
                            while (end > start && std::isspace(static_cast<unsigned char>(value[end - 1])))
                                --end;
                            return value.substr(start, end - start);
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_to_lower":
                    AddInclude("#include <algorithm>", ref codeInclude);
                    AddInclude("#include <cctype>", ref codeInclude);
                    AddFunction("std::string string_to_lower(const std::string& value);", """
                        std::string string_to_lower(const std::string& value)
                        {
                            std::string result = value;
                            std::transform(result.begin(), result.end(), result.begin(),
                                [](unsigned char character) { return static_cast<char>(std::tolower(character)); });
                            return result;
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                case "string_to_upper":
                    AddInclude("#include <algorithm>", ref codeInclude);
                    AddInclude("#include <cctype>", ref codeInclude);
                    AddFunction("std::string string_to_upper(const std::string& value);", """
                        std::string string_to_upper(const std::string& value)
                        {
                            std::string result = value;
                            std::transform(result.begin(), result.end(), result.begin(),
                                [](unsigned char character) { return static_cast<char>(std::toupper(character)); });
                            return result;
                        }
                        """, ref defaultFunction, ref codeHeader);
                    break;
                default:
                    throw new Exception($"Unknown string function '{functionName}'");
            }
        }

        private static void AddFunction(string declaration, string definition, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader)
        {
            if (!AppendToSubstring(declaration, ref codeHeader))
                return;

            defaultFunction.AppendLine(definition);
            defaultFunction.AppendLine();
        }

        private static void AddInclude(string include, ref StringBuilder codeInclude) =>
            AppendToSubstring(include, ref codeInclude);

        private static bool AppendToSubstring(string value, ref StringBuilder stringBuilder)
        {
            if (stringBuilder.ToString().Contains(value, StringComparison.Ordinal))
                return false;

            stringBuilder.AppendLine(value);
            return true;
        }
    }
}
