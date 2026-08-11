using Benita.itpr_df;

namespace BenitaTestProject.Itpr_df
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void HandleFunctionCall_WithPrint_WritesEachArgument()
        {
            // Arrange
            var utility = new Utility();
            var arguments = new List<object> { "Hello", "world!" };
            var expectedOutput = "Hello\r\nworld!\r\n";

            // Act
            using (var consoleOutput = new ConsoleOutput())
            {
                utility.HandleFunctionCall("print", arguments);

                // Assert
                Assert.AreEqual(expectedOutput, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void HandleFunctionCall_WithInput_ReturnsConsoleInput()
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
        public void HandleFunctionCall_WithToString_ReturnsStringRepresentation()
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
        public void HandleFunctionCall_WithToNumber_ReturnsNumericValue()
        {
            // Arrange
            var utility = new Utility();
            var argument = "42";
            var expectedNumber = 42d;

            // Act
            var result = utility.HandleFunctionCall("to_number", new List<object> { argument });

            // Assert
            Assert.AreEqual(expectedNumber, result);
        }

        [TestMethod]
        public void HandleFunctionCall_WithUnknownFunction_ThrowsException()
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
