using Benita.itpr_df;

using Benita;

namespace BenitaTestProject.Runtime.BuiltIns
{
    [TestClass]
    /// <summary>رفتار توابع داخلی عمومی مانند ورودی، خروجی و تبدیل نوع را بررسی می‌کند.</summary>
    public class UtilityTests
    {
        [TestMethod]
        public void HandleFunctionCall_WithPrint_WritesEachArgument()
        {
            var utility = new Utility();
            var arguments = new List<object> { "Hello", "world!" };
            var expectedOutput = "Hello\r\nworld!\r\n";

            using (var consoleOutput = new ConsoleOutput())
            {
                utility.HandleFunctionCall("print", arguments);

                Assert.AreEqual(expectedOutput, consoleOutput.GetOutput());
            }
        }

        [TestMethod]
        public void HandleFunctionCall_WithInput_ReturnsConsoleInput()
        {
            var utility = new Utility();
            var expectedInput = "test input";

            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                var result = utility.HandleFunctionCall("input", new List<object>());

                Assert.AreEqual(expectedInput, result);
            }
        }

        [TestMethod]
        public void HandleFunctionCall_WithToString_ReturnsStringRepresentation()
        {
            var utility = new Utility();
            var argument = 42;
            var expectedString = "42";

            var result = utility.HandleFunctionCall("to_string", new List<object> { argument });

            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void HandleFunctionCall_WithToNumber_ReturnsNumericValue()
        {
            var utility = new Utility();
            var argument = "42";
            var expectedNumber = 42d;

            var result = utility.HandleFunctionCall("to_number", new List<object> { argument });

            Assert.AreEqual(expectedNumber, result);
        }

        [TestMethod]
        public void HandleFunctionCall_WithUnknownFunction_ThrowsException()
        {
            var utility = new Utility();
            var functionName = "unknown_function";
            var arguments = new List<object>();

            var exception = Assert.ThrowsException<BuiltInException>(() => utility.HandleFunctionCall(functionName, arguments));
            Assert.AreEqual($"Unknown utility function '{functionName}'", exception.Description);
        }
    }
}
