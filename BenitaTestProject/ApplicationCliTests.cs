using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
public class ApplicationCliTests
{
    [TestMethod]
    public void Main_MissingExecutionFileReturnsFailureWithoutSuccessMessage()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        TextWriter originalOutput = Console.Out;
        TextWriter originalError = Console.Error;
        try
        {
            Console.SetOut(output);
            Console.SetError(error);

            int exitCode = Program.Main(["exc", Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ben")]);

            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(error.ToString(), "BEN0002");
            Assert.IsFalse(output.ToString().Contains("successfully", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
            output.Dispose();
            error.Dispose();
        }
    }

    [TestMethod]
    public void Main_UnknownActionReturnsFailure()
    {
        var error = new StringWriter();
        TextWriter originalError = Console.Error;
        try
        {
            Console.SetError(error);

            int exitCode = Program.Main(["unknown-action"]);

            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(error.ToString(), "Unknown action");
        }
        finally
        {
            Console.SetError(originalError);
            error.Dispose();
        }
    }
}
