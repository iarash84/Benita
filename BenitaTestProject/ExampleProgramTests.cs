using Benita;

namespace BenitaTestProject;

[TestClass]
[DoNotParallelize]
public class ExampleProgramTests
{
    private static readonly string ExamplesDirectory = FindExamplesDirectory();

    public static IEnumerable<object[]> ExecutableExamples
    {
        get
        {
            // تعریف و فراخوانی تابع جمع عددی و بازگرداندن نتیجه آن را بررسی می‌کند.
            yield return Case("algorithm-binary-search.ben", null, "4", "-1");
            yield return Case("algorithm-bubble-sort.ben", null, "1", "2", "4", "5", "8");
            yield return Case("algorithm-euclidean-gcd.ben", null, "6");
            yield return Case("algorithm-linear-search.ben", null, "3", "-1");
            yield return Case("algorithm-prime-check.ben", null,
                "2 is prime", "9 is not prime", "17 is prime", "21 is not prime", "29 is prime");
            yield return Case("algorithm-recursive-factorial.ben", null, "720");
            yield return Case("addition-function-call.ben", null, "5");
            // حذف و افزودن عناصر آرایه، محاسبه طول آرایه و پیمایش نتیجه را بررسی می‌کند.
            yield return Case("array-add-remove-and-length.ben", null, "Element at index 0: 10", "Element at index 1: 20", "Element at index 2: 40", "Element at index 3: 50", "Element at index 4: 60", "Element at index 5: 70");
            // تعریف آرایه با و بدون مقدار اولیه، مقداردهی و دسترسی با اندیس را بررسی می‌کند.
            yield return Case("array-declaration-and-access.ben", null, "10");
            // مقداردهی به یک اندیس آرایه و خواندن تمام عناصر در حلقه while را بررسی می‌کند.
            yield return Case("array-update-and-iteration.ben", null, "Element at index 0: 10", "Element at index 1: 20", "Element at index 2: 5", "Element at index 3: 40", "Element at index 4: 50");
            // توابع عددی و رشته‌ای، شرط، حلقه، ورودی کنسول و خروجی ترکیبی را بررسی می‌کند.
            yield return Case("combined-functions-condition-loop-input.ben", "sample input", "Hello, world!", "Sum is even.", "You entered: sample input");
            // پردازش کامنت‌ها، متغیر سراسری و فراخوانی تابعی که بعد از main تعریف شده است را بررسی می‌کند.
            yield return Case("comments-global-variable-function-call.ben", null, "30", "This is global variable");
            // الحاق مقادیر رشته‌ای و عددی در یک عبارت print را بررسی می‌کند.
            yield return Case("concatenated-print-expression.ben", null, "This is a test => 1");
            // تابع کمکی بولی و انتخاب شاخه صحیح در ساختار if/else را بررسی می‌کند.
            yield return Case("even-number-function-if-else.ben", null, "Sum is even.");
            // حلقه کلاسیک و پیمایش مستقیم عناصر آرایه با for-in را بررسی می‌کند.
            yield return Case("for-loop.ben", null, "0", "1", "2", "3", "4", "10", "20", "30");
            // استفاده از متغیر سراسری داخل تابع در کنار متغیرهای محلی و رشته‌ها را بررسی می‌کند.
            yield return Case("global-variable-and-function.ben", null, "30", "Hello, World!");
            // بررسی می‌کند که include_once حتی با دو بار درخواست، کتابخانه توابع را فقط یک بار بارگذاری کند.
            yield return Case("include-once-function.ben", null, "5");
            // تبدیل ورودی کنسول و تولید تکرارشونده دنباله فیبوناچی با حلقه while را بررسی می‌کند.
            yield return Case("iterative-fibonacci-with-input.ben", "5", "Enter The Number Of Terms:", "The Fibonacci Series is:", "0", "1", "1", "2", "3");
            // بررسی می‌کند که نمونه جایگزین فیبوناچی تکرارشونده همان دنباله را تولید کند.
            yield return Case("iterative-fibonacci-with-input-alternate.ben", "5", "Enter The Number Of Terms:", "The Fibonacci Series is:", "0", "1", "1", "2", "3");
            // تقدم عملگرهای منطقی AND و OR و انتخاب شاخه مورد انتظار if/else را بررسی می‌کند.
            yield return Case("logical-operators-if-else.ben", null, "YES");
            // جمع دو متغیر عددی تعریف‌شده را بررسی می‌کند.
            yield return Case("numeric-addition.ben", null, "18");
            // تعریف توابع عددی و رشته‌ای و الحاق رشته‌ها را بررسی می‌کند.
            yield return Case("numeric-and-string-functions.ben", null, "hello world");
            // فراخوانی بازگشتی فیبوناچی، بازگشت زودهنگام و پیمایش با حلقه for را بررسی می‌کند.
            yield return Case("recursive-fibonacci-for-loop.ben", null, "0", "1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765");
            // فراخوانی بازگشتی فیبوناچی با متغیر نتیجه و حلقه while را بررسی می‌کند.
            yield return Case("recursive-fibonacci-with-while-loop.ben", null, "0", "1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765");
            // محاسبه و چاپ یک عبارت ساده جمع عددی را بررسی می‌کند.
            yield return Case("simple-addition-expression.ben", null, "5");
            // خواندن رشته از ورودی استاندارد و چاپ دوباره آن برای کاربر را بررسی می‌کند.
            yield return Case("user-input-and-output.ben", "Benita", "Enter your name ", "your name is ", "Benita");
            // بررسی می‌کند که return خالی پیش از اجرای دستورات بعدی از تابع void خارج شود.
            yield return Case("void-function-early-return.ben", null, "yess", "five");
            // بررسی می‌کند که حلقه while بدنه خود را دقیقاً پنج بار اجرا کند.
            yield return Case("while-loop-print.ben", null, "While Test", "While Test", "While Test", "While Test", "While Test");
            // خروجی عملیات حسابی و افزایش متغیرها داخل حلقه while را بررسی می‌کند.
            yield return Case("while-loop-with-increment.ben", null, "30", "15");
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(ExecutableExamples))]
    public void ExampleProgram_WhenExecuted_ProducesExpectedOutput(
        string fileName,
        string? input,
        string expectedOutput)
    {
        var source = File.ReadAllText(Path.Combine(ExamplesDirectory, fileName));
        var temporaryDirectory = CreateTemporaryExamplesDirectory();
        var originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = temporaryDirectory;
            using var consoleInput = input == null ? null : new ConsoleInput(input);
            using var consoleOutput = new ConsoleOutput();

            new CompilerClass().Exec(source);

            Assert.AreEqual(expectedOutput, NormalizeLineEndings(consoleOutput.GetOuput()));
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [DataTestMethod]
    [DataRow("duplicate-global-variable-error.ben", "already declared")]
    [DataRow("main-return-value-error.ben", "Return type mismatch")]
    public void InvalidExample_WhenAnalyzed_ThrowsExpectedError(string fileName, string expectedMessage)
    {
        var source = File.ReadAllText(Path.Combine(ExamplesDirectory, fileName));

        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Exec(source));

        StringAssert.Contains(exception.Message, expectedMessage);
    }

    [TestMethod]
    public void FileExamples_WhenExecuted_PerformExpectedFileOperations()
    {
        var temporaryDirectory = CreateTemporaryExamplesDirectory();
        var originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = temporaryDirectory;

            ExecuteExample("file-exists-read-write.ben");
            Assert.AreEqual("Hello, world!", File.ReadAllText(Path.Combine(temporaryDirectory, "test.txt")));

            var output = ExecuteExample("file-delete.ben");
            Assert.IsFalse(File.Exists(Path.Combine(temporaryDirectory, "test.txt")));
            StringAssert.Contains(output, "file deleted sucessfully");
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ExamplesDirectory_EveryBenFileIsCoveredByTests()
    {
        var coveredFiles = ExecutableExamples.Select(row => (string)row[0])
            .Concat([
                "add-function-library.ben",
                "duplicate-global-variable-error.ben",
                "file-delete.ben",
                "file-exists-read-write.ben",
                "main-return-value-error.ben"
            ])
            .OrderBy(name => name)
            .ToArray();
        var actualFiles = Directory.GetFiles(ExamplesDirectory, "*.ben")
            .Select(Path.GetFileName)
            .OrderBy(name => name)
            .ToArray();

        CollectionAssert.AreEqual(actualFiles, coveredFiles);
    }

    private static object[] Case(string fileName, string? input, params string[] expectedLines) =>
        [fileName, input!, string.Join("\n", expectedLines) + "\n"];

    private static string ExecuteExample(string fileName)
    {
        using var consoleOutput = new ConsoleOutput();
        new CompilerClass().Exec(File.ReadAllText(Path.Combine(ExamplesDirectory, fileName)));
        return NormalizeLineEndings(consoleOutput.GetOuput());
    }

    private static string CreateTemporaryExamplesDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"BenitaExamples-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        File.Copy(
            Path.Combine(ExamplesDirectory, "add-function-library.ben"),
            Path.Combine(directory, "add-function-library.ben"));
        return directory;
    }

    private static string FindExamplesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "Examples");
            if (Directory.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Examples directory.");
    }

    private static string NormalizeLineEndings(string value) => value.Replace("\r\n", "\n");
}
