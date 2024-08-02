using System.Text;

namespace Benita.Cg_df
{
    public class CgFileManagement : ICodeGeneratorClass
    {
        public void HandleFunctionCall(string? functionName, ref StringBuilder code, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader, ref StringBuilder codeInclude)
        {
            switch (functionName)
            {
                case "file_read":
                    GenerateReadFileFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "file_write":
                    GenerateWriteFileFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "file_exist":
                    GenerateExistFileFunction(ref defaultFunction, ref codeHeader);
                    break;
                case "file_delete":
                    GenerateDeleteFileFunction(ref defaultFunction, ref codeHeader, ref codeInclude);
                    break;
                default:
                    throw new Exception($"Unknown function '{functionName}'");
            }
        }

        public void GenerateReadFileFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("std::string file_read(const std::string& filename);", ref codeHeader))
            {
                defaultFunction.AppendLine("std::string file_read(const std::string& filename)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    std::ifstream file(filename);");
                defaultFunction.AppendLine("    if (!file.is_open())");
                defaultFunction.AppendLine("        throw std::runtime_error(\"Could not open file\");");
                defaultFunction.AppendLine(
                    "    std::string content((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());");
                defaultFunction.AppendLine("    file.close();");
                defaultFunction.AppendLine("    return content;");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateWriteFileFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("void file_write(const std::string& filename, const std::string& content);",
                    ref codeHeader))
            {
                defaultFunction.AppendLine("void file_write(const std::string& filename, const std::string& content)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    std::ofstream file(filename);");
                defaultFunction.AppendLine("    if (!file.is_open())");
                defaultFunction.AppendLine("        throw std::runtime_error(\"Could not open file\");");
                defaultFunction.AppendLine("    file << content;");
                defaultFunction.AppendLine("    file.close();");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateExistFileFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader)
        {
            if (AppendToSubstring("bool file_exist(const std::string& filename);", ref codeHeader))
            {
                defaultFunction.AppendLine("bool file_exist(const std::string& filename)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    std::ifstream file(filename);");
                defaultFunction.AppendLine("    return file.is_open();");
                defaultFunction.AppendLine("}");
                defaultFunction.AppendLine();
            }
        }

        public void GenerateDeleteFileFunction(ref StringBuilder defaultFunction, ref StringBuilder codeHeader,
            ref StringBuilder codeInclude)
        {
            AppendToSubstring("#include <filesystem>", ref codeInclude);

            if (AppendToSubstring("bool file_delete(const std::string& filename);", ref codeHeader))
            {
                defaultFunction.AppendLine("bool file_delete(const std::string& filename)");
                defaultFunction.AppendLine("{");
                defaultFunction.AppendLine("    if (std::filesystem::remove(filename))");
                defaultFunction.AppendLine("        return true;");
                defaultFunction.AppendLine("    return false;");
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
