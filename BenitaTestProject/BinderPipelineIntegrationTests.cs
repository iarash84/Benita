using Benita;

namespace BenitaTestProject;

/// <summary>
/// بررسی می‌کند که Binder به‌عنوان context کمکی وارد pipeline معنایی شده
/// و سناریوهای سادهٔ global/function همچنان معتبر هستند.
/// </summary>
[TestClass]
public class BinderPipelineIntegrationTests
{
    [TestMethod]
    public void CompilerPipeline_BindsGlobalIdentifierAndFunctionCall()
    {
        const string source = "number value = 7; func read() -> number { return value; } _main_() { read(); }";

        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void CompilerPipeline_PreservesSemanticDiagnosticsForUnknownVariable()
    {
        const string source = "_main_() { missing = 1; }";

        SemanticException exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check(source));

        StringAssert.Contains(exception.Description, "Undeclared variable 'missing'");
    }
}
