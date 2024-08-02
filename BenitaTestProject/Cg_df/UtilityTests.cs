using Benita.Cg_df;
using System.Text;

namespace BenitaTestProject.Cg_df
{
    [TestClass]
    public class UtilityTests
    {
        private CgUtility _cgUtility;

        [TestInitialize]
        public void TestInitialize()
        {
            _cgUtility = new CgUtility();
        }

        [TestMethod]
        public void TestHandleFunctionCall_Input()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgUtility.HandleFunctionCall("input", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"std::string input();{Environment.NewLine}";
            string expectedFunction = $"std::string input(){Environment.NewLine}{{{Environment.NewLine}    std::string inputString;{Environment.NewLine}    std::getline(std::cin, inputString);{Environment.NewLine}    return inputString;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_ToString()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgUtility.HandleFunctionCall("to_string", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"std::string to_string(int num);{Environment.NewLine}";
            string expectedFunction = $"std::string to_string(int num){Environment.NewLine}{{{Environment.NewLine}    return std::to_string(num);{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_ToNumber()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgUtility.HandleFunctionCall("to_number", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"int to_number(const std::string& str);{Environment.NewLine}";
            string expectedFunction = $"int to_number(const std::string& str){Environment.NewLine}{{{Environment.NewLine}    try {{{Environment.NewLine}        return std::stoi(str);{Environment.NewLine}    }} catch (...) {{{Environment.NewLine}        return 0;{Environment.NewLine}    }}{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

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

            _cgUtility.HandleFunctionCall("unknown_function", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);
        }

        [TestMethod]
        public void TestGenerateInputFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgUtility.GenerateInputFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"std::string input();{Environment.NewLine}";
            string expectedFunction = $"std::string input(){Environment.NewLine}{{{Environment.NewLine}    std::string inputString;{Environment.NewLine}    std::getline(std::cin, inputString);{Environment.NewLine}    return inputString;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgUtility.GenerateInputFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateToStringFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgUtility.GenerateToStringFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"std::string to_string(int num);{Environment.NewLine}";
            string expectedFunction = $"std::string to_string(int num){Environment.NewLine}{{{Environment.NewLine}    return std::to_string(num);{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgUtility.GenerateToStringFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateToNumberFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgUtility.GenerateToNumberFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"int to_number(const std::string& str);{Environment.NewLine}";
            string expectedFunction = $"int to_number(const std::string& str){Environment.NewLine}{{{Environment.NewLine}    try {{{Environment.NewLine}        return std::stoi(str);{Environment.NewLine}    }} catch (...) {{{Environment.NewLine}        return 0;{Environment.NewLine}    }}{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgUtility.GenerateToNumberFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestAppendToSubstring()
        {
            var stringBuilder = new StringBuilder();
            string value = "test_value";

            bool result = AppendToSubstring(value, ref stringBuilder);
            Assert.IsTrue(result);
            Assert.IsTrue(stringBuilder.ToString().Contains(value));

            // Call again to ensure it doesn't append duplicate values
            result = AppendToSubstring(value, ref stringBuilder);
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
