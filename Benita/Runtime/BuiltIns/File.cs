namespace Benita;

/// <summary>کلاس داخلی برای عملیات متنی روی فایل بدون نگهداری file handle دائمی.</summary>
internal sealed class FileClass : BuiltInClassDefinition
{
    internal FileClass()
        : base(
            "File",
            [new BuiltInParameter("path", Types.String)],
            [
                new BuiltInMethodDescriptor("readText", Types.String, []),
                new BuiltInMethodDescriptor("writeText", Types.Void, [Types.String]),
                new BuiltInMethodDescriptor("appendText", Types.Void, [Types.String]),
                new BuiltInMethodDescriptor("exists", Types.Bool, []),
                new BuiltInMethodDescriptor("delete", Types.Void, []),
                new BuiltInMethodDescriptor("copyTo", Types.Void, [Types.String]),
                new BuiltInMethodDescriptor("moveTo", Types.Void, [Types.String])
            ],
            [
                new BuiltInPropertyDescriptor("path", Types.String),
                new BuiltInPropertyDescriptor("length", Types.Number)
            ])
    {
    }

    protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
        new FileInstance(
            RuntimeValue.From(arguments[0]).Require<string>("File constructor"));
}

internal sealed class FileInstance : IBuiltInClassInstance
{
    private readonly string _path;

    internal FileInstance(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new BuiltInException("File path must not be empty.");

        _path = path;
    }

    public string ClassName => "File";

    public object? InvokeMethod(string methodName, IReadOnlyList<object?> arguments) => methodName switch
    {
        "readText" => ReadText(),
        "writeText" => WriteText(arguments),
        "appendText" => AppendText(arguments),
        "exists" => File.Exists(_path),
        "delete" => Delete(),
        "copyTo" => CopyTo(arguments),
        "moveTo" => MoveTo(arguments),
        _ => throw new BuiltInException($"Class '{ClassName}' has no method named '{methodName}'.")
    };

    public object GetProperty(string propertyName) => propertyName switch
    {
        "path" => _path,
        "length" => GetLength(),
        _ => throw new BuiltInException($"Class '{ClassName}' has no property named '{propertyName}'.")
    };

    public void SetProperty(string propertyName, object? value) =>
        throw new BuiltInException($"Property '{ClassName}.{propertyName}' is read-only.");

    public IBuiltInClassInstance Clone() => new FileInstance(_path);

    private string ReadText() => File.ReadAllText(_path);

    private object? WriteText(IReadOnlyList<object?> arguments)
    {
        File.WriteAllText(_path, RuntimeValue.From(arguments[0]).Require<string>("File.writeText"));
        return null;
    }

    private object? AppendText(IReadOnlyList<object?> arguments)
    {
        File.AppendAllText(_path, RuntimeValue.From(arguments[0]).Require<string>("File.appendText"));
        return null;
    }

    private object? Delete()
    {
        File.Delete(_path);
        return null;
    }

    private object? CopyTo(IReadOnlyList<object?> arguments)
    {
        string destination = RuntimeValue.From(arguments[0]).Require<string>("File.copyTo");
        File.Copy(_path, destination);
        return null;
    }

    private object? MoveTo(IReadOnlyList<object?> arguments)
    {
        string destination = RuntimeValue.From(arguments[0]).Require<string>("File.moveTo");
        File.Move(_path, destination);
        return null;
    }

    private double GetLength()
    {
        if (!File.Exists(_path))
            throw new BuiltInException($"Cannot read length because file '{_path}' does not exist.");

        return new FileInfo(_path).Length;
    }
}
