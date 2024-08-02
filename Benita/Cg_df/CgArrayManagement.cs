using System.Text;

namespace Benita.Cg_df
{
    public class CgArrayManagement : ICodeGeneratorClass
    {
        public void HandleFunctionCall(string? functionName, ref StringBuilder code, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader, ref StringBuilder codeInclude)
        {
            switch (functionName)
            {
                case "array_len":
                    GenerateArrayLenFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "array_add":
                    GenerateArrayAddFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "array_remove":
                    GenerateArrayRemoveFunction(ref defaultFunction, ref codeHeader);
                    break;
                default:
                    throw new Exception($"Unknown function '{functionName}'");
            }
        }

        public void GenerateArrayLenFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            AppendToSubstring("template <typename T>", ref codeHeader);
            if (AppendToSubstring("int array_len(const std::vector<T>& vec);", ref codeHeader))
            {
                defaultFunction.AppendLine("int array_len(const std::vector<T>& vec)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    return static_cast<int>(vec.size());");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateArrayAddFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            AppendToSubstring("template <typename T>", ref codeHeader);
            if (AppendToSubstring("std::vector<T> array_add(const std::vector<T>& vec, const T& element);", ref codeHeader))
            {
                defaultFunction.AppendLine("std::vector<T> array_add(const std::vector<T>& vec, const T& element)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    std::vector<T> newVec = vec;");
                defaultFunction.AppendLine("    newVec.push_back(element);");
                defaultFunction.AppendLine("    return newVec;");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateArrayRemoveFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            AppendToSubstring("template <typename T>", ref codeHeader);
            if (AppendToSubstring("std::vector<T> array_remove(const std::vector<T>& vec, int index);", ref codeHeader))
            {
                defaultFunction.AppendLine("std::vector<T> array_remove(const std::vector<T>& vec, int index)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    if (index >= vec.size()) {");
                defaultFunction.AppendLine("        throw std::out_of_range(\"Index is out of range.\");");
                defaultFunction.AppendLine("    }");
                defaultFunction.AppendLine("    std::vector<T> newVec = vec;");
                defaultFunction.AppendLine("    newVec.erase(newVec.begin() + index);");
                defaultFunction.AppendLine("    return newVec;");
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
