using Benita.Cg_df;
using System.Text;

namespace BenitaTestProject.Cg_df
{
    [TestClass]
    public class ArrayManagementTest
    {
        private CgArrayManagement _cgArrayManagement;

        [TestInitialize]
        public void TestInitialize()
        {
            _cgArrayManagement = new CgArrayManagement();
        }

        [TestMethod]
        public void TestHandleFunctionCall_ArrayLen()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgArrayManagement.HandleFunctionCall("array_len", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"template <typename T>{Environment.NewLine}int array_len(const std::vector<T>& vec);{Environment.NewLine}";
            string expectedFunction = $"int array_len(const std::vector<T>& vec){Environment.NewLine}{{{Environment.NewLine}    return static_cast<int>(vec.size());{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));
        }

        [TestMethod]
        public void TestHandleFunctionCall_ArrayAdd()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgArrayManagement.HandleFunctionCall("array_add", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"template <typename T>{Environment.NewLine}std::vector<T> array_add(const std::vector<T>& vec, const T& element);{Environment.NewLine}";
            string expectedFunction = $"std::vector<T> array_add(const std::vector<T>& vec, const T& element){Environment.NewLine}{{{Environment.NewLine}    std::vector<T> newVec = vec;{Environment.NewLine}    newVec.push_back(element);{Environment.NewLine}    return newVec;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.AreEqual(expectedHeader, codeHeader.ToString());
            Assert.AreEqual(expectedFunction, defaultFunction.ToString());
        }

        [TestMethod]
        public void TestHandleFunctionCall_ArrayRemove()
        {
            var code = new StringBuilder();
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();
            var codeInclude = new StringBuilder();

            _cgArrayManagement.HandleFunctionCall("array_remove", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);

            string expectedHeader = $"template <typename T>{Environment.NewLine}std::vector<T> array_remove(const std::vector<T>& vec, int index);{Environment.NewLine}";
            string expectedFunction = $"std::vector<T> array_remove(const std::vector<T>& vec, int index){Environment.NewLine}{{{Environment.NewLine}    if (index >= vec.size()) {{{Environment.NewLine}        throw std::out_of_range(\"Index is out of range.\");{Environment.NewLine}    }}{Environment.NewLine}    std::vector<T> newVec = vec;{Environment.NewLine}    newVec.erase(newVec.begin() + index);{Environment.NewLine}    return newVec;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

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

            _cgArrayManagement.HandleFunctionCall("unknown_function", ref code, ref defaultFunction, ref codeHeader, ref codeInclude);
        }

        [TestMethod]
        public void TestGenerateArrayLenFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgArrayManagement.GenerateArrayLenFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"template <typename T>{Environment.NewLine}int array_len(const std::vector<T>& vec);{Environment.NewLine}";
            string expectedFunction = $"int array_len(const std::vector<T>& vec){Environment.NewLine}{{{Environment.NewLine}    return static_cast<int>(vec.size());{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgArrayManagement.GenerateArrayLenFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateArrayAddFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgArrayManagement.GenerateArrayAddFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"template <typename T>{Environment.NewLine}std::vector<T> array_add(const std::vector<T>& vec, const T& element);{Environment.NewLine}";
            string expectedFunction = $"std::vector<T> array_add(const std::vector<T>& vec, const T& element){Environment.NewLine}{{{Environment.NewLine}    std::vector<T> newVec = vec;{Environment.NewLine}    newVec.push_back(element);{Environment.NewLine}    return newVec;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgArrayManagement.GenerateArrayAddFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestGenerateArrayRemoveFunction()
        {
            var defaultFunction = new StringBuilder();
            var codeHeader = new StringBuilder();

            _cgArrayManagement.GenerateArrayRemoveFunction(ref defaultFunction, ref codeHeader);

            string expectedHeader = $"template <typename T>{Environment.NewLine}std::vector<T> array_remove(const std::vector<T>& vec, int index);{Environment.NewLine}";
            string expectedFunction = $"std::vector<T> array_remove(const std::vector<T>& vec, int index){Environment.NewLine}{{{Environment.NewLine}    if (index >= vec.size()) {{{Environment.NewLine}        throw std::out_of_range(\"Index is out of range.\");{Environment.NewLine}    }}{Environment.NewLine}    std::vector<T> newVec = vec;{Environment.NewLine}    newVec.erase(newVec.begin() + index);{Environment.NewLine}    return newVec;{Environment.NewLine}}}{Environment.NewLine}{Environment.NewLine}";

            Assert.IsTrue(codeHeader.ToString().Contains(expectedHeader));
            Assert.IsTrue(defaultFunction.ToString().Contains(expectedFunction));

            // Call again to ensure it doesn't append duplicate headers/functions
            _cgArrayManagement.GenerateArrayRemoveFunction(ref defaultFunction, ref codeHeader);

            Assert.AreEqual(1, CountOccurrences(codeHeader.ToString(), expectedHeader));
            Assert.AreEqual(1, CountOccurrences(defaultFunction.ToString(), expectedFunction));
        }

        [TestMethod]
        public void TestAppendToSubstring()
        {
            var stringBuilder = new StringBuilder();

            // Test appending a new value
            bool result = SharedFunction.AppendToSubstring("template <typename T>", ref stringBuilder);
            Assert.IsTrue(result);
            Assert.IsTrue(stringBuilder.ToString().Contains("template <typename T>"));

            // Test appending the same value again (should not append)
            result = SharedFunction.AppendToSubstring("template <typename T>", ref stringBuilder);
            Assert.IsFalse(result);
            Assert.AreEqual(1, CountOccurrences(stringBuilder.ToString(), "template <typename T>"));
        }

        private int CountOccurrences(string source, string substring)
        {
            int count = 0, n = 0;

            while ((n = source.IndexOf(substring, n, StringComparison.Ordinal)) != -1)
            {
                n += substring.Length;
                count++;
            }

            return count;
        }
    }
}
