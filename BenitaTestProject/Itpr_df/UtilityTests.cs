using Benita.itpr_df;

namespace BenitaTestProject.Itpr_df
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestHandleFunctionCall_Print()
        {
            // Arrange
            var utility = new Utility();
            var arguments = new List<object> { "Hello", "world!" };
            var expectedOutput = "Hello" + Environment.NewLine + "world!" + Environment.NewLine;

            // Act
            using (var consoleOutput = new ConsoleOutput())
            {
                utility.HandleFunctionCall("print", arguments);

                // Assert
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void TestHandleFunctionCall_Input()
        {
            // Arrange
            var utility = new Utility();
            var expectedInput = "test input";

            // Act
            using (var consoleInput = new ConsoleInput(expectedInput))
            {
                var result = utility.HandleFunctionCall("input", new List<object>());

                // Assert
                Assert.AreEqual(expectedInput, result);
            }
        }

        [TestMethod]
        public void TestHandleFunctionCall_ToString()
        {
            // Arrange
            var utility = new Utility();
            var argument = 42;
            var expectedString = "42";

            // Act
            var result = utility.HandleFunctionCall("to_string", new List<object> { argument });

            // Assert
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void TestHandleFunctionCall_ToNumber()
        {
            // Arrange
            var utility = new Utility();
            var argument = "42";
            var expectedNumber = 42;

            // Act
            var result = utility.HandleFunctionCall("to_number", new List<object> { argument });

            // Assert
            Assert.AreEqual(expectedNumber, result);
        }

        [TestMethod]
        public void TestHandleFunctionCall_UnknownFunction()
        {
            // Arrange
            var utility = new Utility();
            var functionName = "unknown_function";
            var arguments = new List<object>();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => utility.HandleFunctionCall(functionName, arguments));
            Assert.AreEqual($"Unknown utility function '{functionName}'", exception.Message);
        }
    }
}
