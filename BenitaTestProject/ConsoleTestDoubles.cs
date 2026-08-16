namespace BenitaTestProject;

/// <summary>خروجی استاندارد را برای assertionهای تست به‌صورت موقت ضبط می‌کند.</summary>
internal sealed class ConsoleOutput : IDisposable
{
    private readonly StringWriter _writer = new();
    private readonly TextWriter _originalOutput = Console.Out;

    public ConsoleOutput() => Console.SetOut(_writer);

    /// <summary>خروجی ضبط‌شده را با line ending ثابت ویندوز برمی‌گرداند.</summary>
    public string GetOutput() => _writer.ToString().ReplaceLineEndings("\r\n");

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _writer.Dispose();
    }
}

/// <summary>ورودی استاندارد کنترل‌شده‌ای را در طول یک تست جایگزین می‌کند.</summary>
internal sealed class ConsoleInput : IDisposable
{
    private readonly StringReader _reader;
    private readonly TextReader _originalInput = Console.In;

    public ConsoleInput(string input)
    {
        _reader = new StringReader(input);
        Console.SetIn(_reader);
    }

    public void Dispose()
    {
        Console.SetIn(_originalInput);
        _reader.Dispose();
    }
}
