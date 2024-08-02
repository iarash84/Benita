using Benita.itpr_df;

namespace BenitaTestProject.Itpr_df
{
    [TestClass]
    public class FileManagementTests
    {

        private const string TestFilePath = "testfile.txt";

        [TestMethod]
        public void TestFileRead()
        {
            // Arrange
            var fileContent = "Test file content";
            File.WriteAllText(TestFilePath, fileContent);

            var fileManagement = new FileManagement();
            var arguments = new List<object> { TestFilePath };

            // Act
            var result = fileManagement.HandleFunctionCall("file_read", arguments);

            // Assert
            Assert.AreEqual(fileContent, result);
        }

        [TestMethod]
        public void TestFileWrite()
        {
            // Arrange
            var fileManagement = new FileManagement();
            var contentToWrite = "Content to write";
            var arguments = new List<object> { TestFilePath, contentToWrite };

            // Act
            fileManagement.HandleFunctionCall("file_write", arguments);

            // Assert
            Assert.AreEqual(contentToWrite, File.ReadAllText(TestFilePath));

            // Clean up
            File.Delete(TestFilePath);
        }

        [TestMethod]
        public void TestFileExist()
        {
            // Arrange
            var fileManagement = new FileManagement();
            var arguments = new List<object> { TestFilePath };

            // Act
            var result = fileManagement.HandleFunctionCall("file_exist", arguments);

            // Assert
            Assert.IsFalse((bool)result);

            // Create the file
            File.WriteAllText(TestFilePath, "dummy content");

            // Act again
            result = fileManagement.HandleFunctionCall("file_exist", arguments);

            // Assert
            Assert.IsTrue((bool)result);

            // Clean up
            File.Delete(TestFilePath);
        }

        [TestMethod]
        public void TestFileDelete()
        {
            // Arrange
            var fileManagement = new FileManagement();
            File.WriteAllText(TestFilePath, "dummy content");
            var arguments = new List<object> { TestFilePath };

            // Act
            var resultBeforeDelete = fileManagement.HandleFunctionCall("file_exist", arguments);
            var deleteResult = fileManagement.HandleFunctionCall("file_delete", arguments);
            var resultAfterDelete = fileManagement.HandleFunctionCall("file_exist", arguments);

            // Assert
            Assert.IsTrue((bool)resultBeforeDelete);
            Assert.IsTrue((bool)deleteResult);
            Assert.IsFalse((bool)resultAfterDelete);
        }

        [TestMethod]
        public void TestUnknownFunction()
        {
            // Arrange
            var fileManagement = new FileManagement();
            var arguments = new List<object>();

            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => fileManagement.HandleFunctionCall("unknown_function", arguments));
            Assert.AreEqual("Unknown function 'unknown_function'", exception.Message);
        }

        // Clean up any leftover files
        public FileManagementTests()
        {
            if (File.Exists(TestFilePath))
            {
                File.Delete(TestFilePath);
            }
        }
    }
}
