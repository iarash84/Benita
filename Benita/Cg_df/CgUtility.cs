using System.Text;

namespace Benita.Cg_df
{
    public class CgUtility : ICodeGeneratorClass
    {
        public void HandleFunctionCall(string? functionName, ref StringBuilder code, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader, ref StringBuilder codeInclude)
        {
            switch (functionName)
            {
                //case "print":
                //    GeneratePrintFunctionCall(functionCallNode, ref _code);
                //    break;
                case "input":
                    GenerateInputFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "to_string":
                    GenerateToStringFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "to_number":
                    GenerateToNumberFunction(ref defaultFunction, ref codeHeader);
                    break;
                default:
                    throw new Exception($"Unknown utility function '{functionName}'");
            }
        }

        public void GenerateInputFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("std::string input();", ref codeHeader)) // Declare input function
            {
                defaultFunction.AppendLine("std::string input()");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    std::string inputString;");
                defaultFunction.AppendLine("    std::getline(std::cin, inputString);");
                defaultFunction.AppendLine("    return inputString;");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateToStringFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("std::string to_string(int num);", ref codeHeader))
            {
                defaultFunction.AppendLine("std::string to_string(int num)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    return std::to_string(num);");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateToNumberFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("int to_number(const std::string& str);", ref codeHeader))
            {
                defaultFunction.AppendLine("int to_number(const std::string& str)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    try {");
                defaultFunction.AppendLine("        return std::stoi(str);");
                defaultFunction.AppendLine("    } catch (...) {");
                defaultFunction.AppendLine("        return 0;");
                defaultFunction.AppendLine("    }");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        private bool AppendToSubstring(string value, ref StringBuilder stringBuilder)
        {
            string sbContent = stringBuilder.ToString();
            if (!sbContent.Contains(value))
            {
                stringBuilder.AppendLine(value);
                return true;
            }

            return false;
        }
    }
}
