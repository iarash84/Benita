using Benita.Cg_df;
using System.Text;

namespace BenitaTestProject.Cg_df
{
    [TestClass]
    public class FileManagementTests
    {
        private CgFileManagement _cgFileManagement;

        [TestInitialize]
        public void TestInitialize()
        {
            _cgFileManagement = new CgFileManagement();
        }

        [TestMethod]
        public void TestHandleFunctionCall_FileRead()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.HandleFunctionCall("file_read", ref code, ref defaultFunction, ref codeHeader,
                ref codeInclude);

            string expectedHeader = $"std::string file_read(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"std::string file_read(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    std::ifstream file(filename);{Environment.NewLine}    if (!file.is_open()){Environment.NewLine}        throw std::runtime_error(\"Could not open file\");{Environment.NewLine}    std::string content((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());{Environment.NewLine}    file.close();{Environment.NewLine}    return content;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_FileWrite()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.HandleFunctionCall("file_write", ref code, ref defaultFunction, ref codeHeader,
                ref codeInclude);

            string expectedHeader =
                $"void file_write(const std::string& filename, const std::string& content);{Environment.NewLine}";
            string expectedFunction =
                $"void file_write(const std::string& filename, const std::string& content){Environment.NewLine}{{{Environment.NewLine}    std::ofstream file(filename);{Environment.NewLine}    if (!file.is_open()){Environment.NewLine}        throw std::runtime_error(\"Could not open file\");{Environment.NewLine}    file << content;{Environment.NewLine}    file.close();{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_FileExist()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.HandleFunctionCall("file_exist", ref code, ref defaultFunction, ref codeHeader,
                ref codeInclude);

            string expectedHeader = $"bool file_exist(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"bool file_exist(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    std::ifstream file(filename);{Environment.NewLine}    return file.is_open();{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_FileDelete()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.HandleFunctionCall("file_delete", ref code, ref defaultFunction, ref codeHeader,
                ref codeInclude);

            string expectedInclude = $"#include <filesystem>{Environment.NewLine}";
            string expectedHeader = $"bool file_delete(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"bool file_delete(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    if (std::filesystem::remove(filename)){Environment.NewLine}        return true;{Environment.NewLine}    return false;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedInclude, codeInclude.ToString());
            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestHandleFunctionCall_UnknownFunction()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.HandleFunctionCall("unknown_function", ref code, ref defaultFunction, ref codeHeader,
                ref codeInclude);
        }

        [TestMethod]
        public void TestGenerateReadFileFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgFileManagement.GenerateReadFileFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"std::string file_read(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"std::string file_read(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    std::ifstream file(filename);{Environment.NewLine}    if (!file.is_open()){Environment.NewLine}        throw std::runtime_error(\"Could not open file\");{Environment.NewLine}    std::string content((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());{Environment.NewLine}    file.close();{Environment.NewLine}    return content;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgFileManagement.GenerateReadFileFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateWriteFileFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgFileManagement.GenerateWriteFileFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader =
                $"void file_write(const std::string& filename, const std::string& content);{Environment.NewLine}";
            string expectedFunction =
                $"void file_write(const std::string& filename, const std::string& content){Environment.NewLine}{{{Environment.NewLine}    std::ofstream file(filename);{Environment.NewLine}    if (!file.is_open()){Environment.NewLine}        throw std::runtime_error(\"Could not open file\");{Environment.NewLine}    file << content;{Environment.NewLine}    file.close();{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgFileManagement.GenerateWriteFileFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateExistFileFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgFileManagement.GenerateExistFileFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"bool file_exist(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"bool file_exist(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    std::ifstream file(filename);{Environment.NewLine}    return file.is_open();{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgFileManagement.GenerateExistFileFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateDeleteFileFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgFileManagement.GenerateDeleteFileFunction(ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedInclude = $"#include <filesystem>{Environment.NewLine}";
            string expectedHeader = $"bool file_delete(const std::string& filename);{Environment.NewLine}";
            string expectedFunction =
                $"bool file_delete(const std::string& filename){Environment.NewLine}{{{Environment.NewLine}    if (std::filesystem::remove(filename)){Environment.NewLine}        return true;{Environment.NewLine}    return false;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeInclude.ToString().Contains(expectedInclude));
            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgFileManagement.GenerateDeleteFileFunction(ref defaultFunction, ref codeHeader, ref codeInclude);

            Assert.AreEqual(1, CountOccurrences(codeInclude.ToString(), expectedInclude));
            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestAppendToSubstring()
        {
            var stringBuilder = new StringBuilder();
            string value = "test_value";

            bool result = SharedFunction.AppendToSubstring(value, ref stringBuilder);
            Assert.IsTrue(result);
            Assert.IsTrue(stringBuilder.ToString().Contains(value));

            // Call again to ensure it doesn't append duplicate values
            result = SharedFunction.AppendToSubstring(value, ref stringBuilder);
            Assert.IsFalse(result);
            Assert.AreEqual(1, CountOccurrences(stringBuilder.ToString(), value));
        }

        private int CountOccurrences(string source, string substring)
        {
            int count = 0;
            int index = 0;

            while ((index = source.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += substring.Length;
            }

            return count;
        }
    }
}
