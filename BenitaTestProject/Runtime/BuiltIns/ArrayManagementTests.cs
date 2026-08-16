using Benita.itpr_df;

using Benita;

namespace BenitaTestProject.Runtime.BuiltIns
{
    [TestClass]
    public class ArrayManagementTests
    {
        [TestMethod]
        public void HandleFunctionCall_WithArrayLength_ReturnsElementCount()
        {
            var array = new object[] { 1, 2, 3 };
            var arguments = new List<object> { array };

            var arrayManagement = new ArrayManagement();

            var result = arrayManagement.HandleFunctionCall("array_len", arguments);

            Assert.AreEqual(array.Length, result);
        }

        [TestMethod]
        public void HandleFunctionCall_WithArrayAdd_ReturnsArrayWithAppendedElement()
        {
            var initialArray = new object[] { 1, 2, 3 };
            var valueToAdd = 4;
            var expectedArray = new object[] { 1, 2, 3, 4 };
            var arguments = new List<object> { initialArray, valueToAdd };

            var arrayManagement = new ArrayManagement();

            var result = arrayManagement.HandleFunctionCall("array_add", arguments);
            var resultArray = result as object[];

            Assert.AreNotEqual(null, resultArray);
            Assert.AreEqual(expectedArray.Length, resultArray.Length);
            CollectionAssert.AreEqual(expectedArray, resultArray);
        }

        [TestMethod]
        public void HandleFunctionCall_WithArrayRemove_ReturnsArrayWithoutSelectedElement()
        {
            var initialArray = new object[] { 1, 2, 3, 4 };
            var indexToRemove = 2; // Element '3'
            var expectedArray = new object[] { 1, 2, 4 };
            var arguments = new List<object> { initialArray, indexToRemove };

            var arrayManagement = new ArrayManagement();

            var result = arrayManagement.HandleFunctionCall("array_remove", arguments);
            var resultArray = result as object[];

            Assert.AreNotEqual(null, resultArray);
            Assert.AreEqual(expectedArray.Length, resultArray.Length);
            CollectionAssert.AreEqual(expectedArray, resultArray);
        }

        [TestMethod]
        public void HandleFunctionCall_WithNonArrayLengthArgument_ThrowsException()
        {
            var arguments = new List<object> { "not an array" };
            var arrayManagement = new ArrayManagement();

            var exception = Assert.ThrowsException<BuiltInException>(() => arrayManagement.HandleFunctionCall("array_len", arguments));
            Assert.AreEqual("Argument to array_len must be an array", exception.Description);
        }

        [TestMethod]
        public void HandleFunctionCall_WithNonArrayAddArgument_ThrowsException()
        {
            var arguments = new List<object> { "not an array", 1 };
            var arrayManagement = new ArrayManagement();

            var exception = Assert.ThrowsException<BuiltInException>(() => arrayManagement.HandleFunctionCall("array_add", arguments));
            Assert.AreEqual("Argument to array_add must be an array", exception.Description);
        }

        [TestMethod]
        public void HandleFunctionCall_WithNonArrayRemoveArgument_ThrowsException()
        {
            var arguments = new List<object> { "not an array", 0 };
            var arrayManagement = new ArrayManagement();

            var exception = Assert.ThrowsException<BuiltInException>(() => arrayManagement.HandleFunctionCall("array_remove", arguments));
            Assert.AreEqual("Argument to array_remove must be an array", exception.Description);
        }

        [TestMethod]
        public void HandleFunctionCall_WithOutOfRangeRemoveIndex_ThrowsArgumentOutOfRangeException()
        {
            var initialArray = new object[] { 1, 2, 3 };
            var indexToRemove = 5; // Index out of range
            var arguments = new List<object> { initialArray, indexToRemove };

            var arrayManagement = new ArrayManagement();

            var exception = Assert.ThrowsException<BuiltInException>(() => arrayManagement.HandleFunctionCall("array_remove", arguments));
            Assert.AreEqual("Index is out of range. (Parameter 'index')", exception.Description);
        }

        [TestMethod]
        public void HandleFunctionCall_WithUnknownFunction_ThrowsException()
        {
            var arrayManagement = new ArrayManagement();
            var arguments = new List<object>();

            var exception = Assert.ThrowsException<BuiltInException>(() => arrayManagement.HandleFunctionCall("unknown_function", arguments));
            Assert.AreEqual("Unknown function 'unknown_function'", exception.Description);
        }

        [TestMethod]
        public void HandleFunctionCall_WithArrayLookupFunctions_FindsElements()
        {
            var service = new ArrayManagement();
            var array = new object[] { "a", "b", "c" };
            Assert.AreEqual(true, service.HandleFunctionCall("array_contains", [array, "b"]));
            Assert.AreEqual(false, service.HandleFunctionCall("array_contains", [array, "z"]));
            Assert.AreEqual(1, service.HandleFunctionCall("array_index_of", [array, "b"]));
            Assert.AreEqual(-1, service.HandleFunctionCall("array_index_of", [array, "z"]));
        }

        [TestMethod]
        public void HandleFunctionCall_WithArrayTransformFunctions_ReturnsNewArrays()
        {
            var service = new ArrayManagement();
            var source = new object[] { 3, 1, 2 };
            CollectionAssert.AreEqual(new object[] { 2, 1, 3 }, (object[])service.HandleFunctionCall("array_reverse", [source]));
            CollectionAssert.AreEqual(Array.Empty<object>(), (object[])service.HandleFunctionCall("array_clear", [source]));
            CollectionAssert.AreEqual(new object[] { 3, 1, 2 }, source);
        }

        [TestMethod]
        public void HandleFunctionCall_WithArrayCompositionFunctions_ReturnsExpectedArrays()
        {
            var service = new ArrayManagement();
            var inserted = (object[])service.HandleFunctionCall("array_insert", [new object[] { 10, 30 }, 1, 20]);
            CollectionAssert.AreEqual(new object[] { 10, 20, 30 }, inserted);
            CollectionAssert.AreEqual(new object[] { 20, 30 }, (object[])service.HandleFunctionCall("array_slice", [inserted, 1, 2]));
            var combined = (object[])service.HandleFunctionCall("array_concat", [new object[] { 3, 1 }, new object[] { 4, 2 }]);
            CollectionAssert.AreEqual(new object[] { 1, 2, 3, 4 }, (object[])service.HandleFunctionCall("array_sort", [combined]));
        }

        [TestMethod]
        public void HandleFunctionCall_WithInvalidArrayRanges_ThrowsException()
        {
            var service = new ArrayManagement();
            var array = new object[] { 1, 2, 3 };
            Assert.ThrowsException<BuiltInException>(() => service.HandleFunctionCall("array_insert", [array, 4, 9]));
            Assert.ThrowsException<BuiltInException>(() => service.HandleFunctionCall("array_slice", [array, 2, 2]));
        }
    }
}
