using Benita.itpr_df;

namespace BenitaTestProject.Itpr_df
{
    [TestClass]
    public class ArrayManagementTests
    {
        [TestMethod]
        public void TestArrayLen()
        {
            // Arrange
            var array = new object[] { 1, 2, 3 };
            var arguments = new List<object> { array };

            var arrayManagement = new ArrayManagement();

            // Act
            var result = arrayManagement.HandleFunctionCall("array_len", arguments);

            // Assert
            Assert.AreEqual(array.Length, result);
        }

        [TestMethod]
        public void TestArrayAdd()
        {
            // Arrange
            var initialArray = new object[] { 1, 2, 3 };
            var valueToAdd = 4;
            var expectedArray = new object[] { 1, 2, 3, 4 };
            var arguments = new List<object> { initialArray, valueToAdd };

            var arrayManagement = new ArrayManagement();

            // Act
            var result = arrayManagement.HandleFunctionCall("array_add", arguments);
            var resultArray = result as object[];

            // Assert
            Assert.AreNotEqual(null, resultArray);
            Assert.AreEqual(expectedArray.Length, resultArray.Length);
            CollectionAssert.AreEqual(expectedArray, resultArray);
        }

        [TestMethod]
        public void TestArrayRemove()
        {
            // Arrange
            var initialArray = new object[] { 1, 2, 3, 4 };
            var indexToRemove = 2; // Element '3'
            var expectedArray = new object[] { 1, 2, 4 };
            var arguments = new List<object> { initialArray, indexToRemove };

            var arrayManagement = new ArrayManagement();

            // Act
            var result = arrayManagement.HandleFunctionCall("array_remove", arguments);
            var resultArray = result as object[];

            // Assert
            Assert.AreNotEqual(null, resultArray);
            Assert.AreEqual(expectedArray.Length, resultArray.Length);
            CollectionAssert.AreEqual(expectedArray, resultArray);
        }

        [TestMethod]
        public void TestArrayLenWithInvalidArgument()
        {
            // Arrange
            var arguments = new List<object> { "not an array" };
            var arrayManagement = new ArrayManagement();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => arrayManagement.HandleFunctionCall("array_len", arguments));
            Assert.AreEqual("Argument to array_len must be an array", exception.Message);
        }

        [TestMethod]
        public void TestArrayAddWithInvalidArgument()
        {
            // Arrange
            var arguments = new List<object> { "not an array", 1 };
            var arrayManagement = new ArrayManagement();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => arrayManagement.HandleFunctionCall("array_add", arguments));
            Assert.AreEqual("Argument to array_add must be an array", exception.Message);
        }

        [TestMethod]
        public void TestArrayRemoveWithInvalidArgument()
        {
            // Arrange
            var arguments = new List<object> { "not an array", 0 };
            var arrayManagement = new ArrayManagement();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => arrayManagement.HandleFunctionCall("array_remove", arguments));
            Assert.AreEqual("Argument to array_remove must be an array", exception.Message);
        }

        [TestMethod]
        public void TestArrayRemoveWithOutOfRangeIndex()
        {
            // Arrange
            var initialArray = new object[] { 1, 2, 3 };
            var indexToRemove = 5; // Index out of range
            var arguments = new List<object> { initialArray, indexToRemove };

            var arrayManagement = new ArrayManagement();

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => arrayManagement.HandleFunctionCall("array_remove", arguments));
            Assert.AreEqual("Index is out of range. (Parameter 'index')", exception.Message);
        }

        [TestMethod]
        public void TestUnknownFunction()
        {
            // Arrange
            var arrayManagement = new ArrayManagement();
            var arguments = new List<object>();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => arrayManagement.HandleFunctionCall("unknown_function", arguments));
            Assert.AreEqual("Unknown function 'unknown_function'", exception.Message);
        }
    }
}
