using Benita.itpr_df;

using Benita;

namespace BenitaTestProject.Runtime.BuiltIns
{
    [TestClass]
    /// <summary>خواندن، نوشتن، بررسی وجود و حذف فایل توسط توابع داخلی را بررسی می‌کند.</summary>
    public class FileManagementTests
    {

        private const string TestFilePath = "testfile.txt";

        [TestMethod]
        public void HandleFunctionCall_WithFileRead_ReturnsFileContent()
        {
            var fileContent = "Test file content";
            File.WriteAllText(TestFilePath, fileContent);

            var fileManagement = new FileManagement();
            var arguments = new List<object> { TestFilePath };

            var result = fileManagement.HandleFunctionCall("file_read", arguments);

            Assert.AreEqual(fileContent, result);
        }

        [TestMethod]
        public void HandleFunctionCall_WithFileWrite_WritesFileContent()
        {
            var fileManagement = new FileManagement();
            var contentToWrite = "Content to write";
            var arguments = new List<object> { TestFilePath, contentToWrite };

            fileManagement.HandleFunctionCall("file_write", arguments);

            Assert.AreEqual(contentToWrite, File.ReadAllText(TestFilePath));

            // Clean up
            File.Delete(TestFilePath);
        }

        [TestMethod]
        public void HandleFunctionCall_WithFileExist_ReturnsCurrentExistenceState()
        {
            var fileManagement = new FileManagement();
            var arguments = new List<object> { TestFilePath };

            var result = fileManagement.HandleFunctionCall("file_exist", arguments);

            Assert.IsFalse((bool)result);

            // Create the file
            File.WriteAllText(TestFilePath, "dummy content");

            result = fileManagement.HandleFunctionCall("file_exist", arguments);

            Assert.IsTrue((bool)result);

            // Clean up
            File.Delete(TestFilePath);
        }

        [TestMethod]
        public void HandleFunctionCall_WithFileDelete_DeletesExistingFile()
        {
            var fileManagement = new FileManagement();
            File.WriteAllText(TestFilePath, "dummy content");
            var arguments = new List<object> { TestFilePath };

            var resultBeforeDelete = fileManagement.HandleFunctionCall("file_exist", arguments);
            var deleteResult = fileManagement.HandleFunctionCall("file_delete", arguments);
            var resultAfterDelete = fileManagement.HandleFunctionCall("file_exist", arguments);

            Assert.IsTrue((bool)resultBeforeDelete);
            Assert.IsTrue((bool)deleteResult);
            Assert.IsFalse((bool)resultAfterDelete);
        }

        [TestMethod]
        public void HandleFunctionCall_WithUnknownFunction_ThrowsException()
        {
            var fileManagement = new FileManagement();
            var arguments = new List<object>();

            var exception = Assert.ThrowsException<BuiltInException>(() => fileManagement.HandleFunctionCall("unknown_function", arguments));
            Assert.AreEqual("Unknown function 'unknown_function'", exception.Description);
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
