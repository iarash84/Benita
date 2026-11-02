namespace Benita;

internal abstract class StaticBuiltInClassDefinition : BuiltInClassDefinition
{
    protected StaticBuiltInClassDefinition(string name, IReadOnlyList<BuiltInMethodDescriptor> methods)
        : base(name, staticMethods: methods)
    {
    }

    protected override IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments) =>
        throw new BuiltInException($"Static built-in class '{Name}' cannot be instantiated.");
}

internal sealed class TerminalClass : StaticBuiltInClassDefinition
{
    internal TerminalClass() : base("Terminal", [
        new("print", Types.Void, [Types.Any]),
        new("write", Types.Void, [Types.Any]),
        new("write_line", Types.Void, [Types.Any]),
        new("input", Types.String, []),
        new("input_prompt", Types.String, [Types.String]),
        new("read_number", Types.Number, [Types.String]),
        new("read_boolean", Types.Bool, [Types.String]),
        new("clear", Types.Void, []),
        new("is_input_redirected", Types.Bool, []),
        new("is_output_redirected", Types.Bool, [])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> arguments) => methodName switch
    {
        "print" or "write_line" => WriteLine(arguments),
        "write" => Write(arguments),
        "input" => ReadLine(),
        "input_prompt" => ReadLineWithPrompt(arguments),
        "read_number" => ReadNumber(arguments),
        "read_boolean" => ReadBoolean(arguments),
        "clear" => Clear(),
        "is_input_redirected" => System.Console.IsInputRedirected,
        "is_output_redirected" => System.Console.IsOutputRedirected,
        _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
    };

    private static object? Write(IReadOnlyList<object?> arguments)
    {
        System.Console.Write(arguments[0]);
        return null;
    }

    private static object? WriteLine(IReadOnlyList<object?> arguments)
    {
        System.Console.WriteLine(arguments[0]);
        return null;
    }

    private static string ReadLine() => System.Console.ReadLine() ?? string.Empty;

    private static string ReadLineWithPrompt(IReadOnlyList<object?> arguments)
    {
        System.Console.Write(arguments[0]);
        return ReadLine();
    }

    private static double ReadNumber(IReadOnlyList<object?> arguments)
    {
        string value = ReadLineWithPrompt(arguments);
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double result))
            return result;

        throw new BuiltInException($"Terminal.read_number could not parse '{value}' as a number.");
    }

    private static bool ReadBoolean(IReadOnlyList<object?> arguments)
    {
        string value = ReadLineWithPrompt(arguments);
        if (bool.TryParse(value, out bool result))
            return result;

        throw new BuiltInException($"Terminal.read_boolean could not parse '{value}' as a boolean.");
    }

    private static object? Clear()
    {
        try
        {
            System.Console.Clear();
        }
        catch (IOException exception)
        {
            throw new BuiltInException($"Terminal.clear failed: {exception.Message}", exception);
        }

        return null;
    }
}

internal sealed class ArrayClass : StaticBuiltInClassDefinition
{
    internal ArrayClass() : base("Array", [
        new("length", Types.Number, [Types.AnyArray]),
        new("add", Types.AnyArray, [Types.AnyArray, Types.Any]),
        new("remove", Types.AnyArray, [Types.AnyArray, Types.Number]),
        new("contains", Types.Bool, [Types.AnyArray, Types.Any]),
        new("index_of", Types.Number, [Types.AnyArray, Types.Any]),
        new("reverse", Types.AnyArray, [Types.AnyArray]),
        new("clear", Types.AnyArray, [Types.AnyArray]),
        new("insert", Types.AnyArray, [Types.AnyArray, Types.Number, Types.Any]),
        new("slice", Types.AnyArray, [Types.AnyArray, Types.Number, Types.Number]),
        new("concat", Types.AnyArray, [Types.AnyArray, Types.AnyArray]),
        new("sort", Types.AnyArray, [Types.AnyArray])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> arguments) => methodName switch
    {
        "length" => (double)RequireArray(arguments[0], "Array.length").Length,
        "add" => Add(arguments),
        "remove" => Remove(arguments),
        "contains" => RequireArray(arguments[0], "Array.contains").Cast<object?>().Any(item => RuntimeValueComparer.AreEqual(item, arguments[1])),
        "index_of" => IndexOf(arguments),
        "reverse" => Reverse(arguments),
        "clear" => Array.Empty<object>(),
        "insert" => Insert(arguments),
        "slice" => Slice(arguments),
        "concat" => Concat(arguments),
        "sort" => Sort(arguments),
        _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
    };

    private static Array RequireArray(object? value, string name) => value is Array array
        ? array : throw new BuiltInException($"Argument to {name} must be an array.");

    private static object[] Copy(Array array)
    {
        object[] result = new object[array.Length];
        array.CopyTo(result, 0);
        return result;
    }

    private static object[] Add(IReadOnlyList<object?> args)
    {
        Array array = RequireArray(args[0], "Array.add");
        object[] result = new object[array.Length + 1];
        array.CopyTo(result, 0);
        result[^1] = args[1]!;
        return result;
    }

    private static object[] Remove(IReadOnlyList<object?> args)
    {
        Array array = RequireArray(args[0], "Array.remove");
        int index = RuntimeIndex.Normalize(args[1], array.Length, "index");
        object[] result = new object[array.Length - 1];
        for (int source = 0, target = 0; source < array.Length; source++)
            if (source != index) result[target++] = array.GetValue(source)!;
        return result;
    }

    private static object IndexOf(IReadOnlyList<object?> args)
    {
        Array array = RequireArray(args[0], "Array.index_of");
        for (int index = 0; index < array.Length; index++)
            if (RuntimeValueComparer.AreEqual(array.GetValue(index), args[1])) return (double)index;
        return (double)-1;
    }

    private static object[] Reverse(IReadOnlyList<object?> args)
    {
        object[] result = Copy(RequireArray(args[0], "Array.reverse"));
        Array.Reverse(result);
        return result;
    }

    private static object[] Insert(IReadOnlyList<object?> args)
    {
        Array array = RequireArray(args[0], "Array.insert");
        int index = RuntimeIndex.Normalize(args[1], array.Length, "index", allowEnd: true);
        object[] result = new object[array.Length + 1];
        for (int source = 0, target = 0; target < result.Length; target++)
            result[target] = target == index ? args[2]! : array.GetValue(source++)!;
        return result;
    }

    private static object[] Slice(IReadOnlyList<object?> args)
    {
        Array array = RequireArray(args[0], "Array.slice");
        int start = RuntimeIndex.Normalize(args[1], array.Length, "start", allowEnd: true);
        int length = RuntimeIndex.NonNegativeWhole(args[2], "length");
        if (length > array.Length - start) throw new BuiltInException("Array.slice range is out of bounds.");
        object[] result = new object[length];
        for (int index = 0; index < length; index++) result[index] = array.GetValue(start + index)!;
        return result;
    }

    private static object[] Concat(IReadOnlyList<object?> args)
    {
        Array first = RequireArray(args[0], "Array.concat");
        Array second = RequireArray(args[1], "Array.concat");
        object[] result = new object[first.Length + second.Length];
        first.CopyTo(result, 0);
        second.CopyTo(result, first.Length);
        return result;
    }

    private static object[] Sort(IReadOnlyList<object?> args)
    {
        object[] result = Copy(RequireArray(args[0], "Array.sort"));
        Array.Sort(result, RuntimeValueComparer.Compare);
        return result;
    }
}

internal sealed class TextClass : StaticBuiltInClassDefinition
{
    internal TextClass() : base("Text", [
        new("length", Types.Number, [Types.String]),
        new("char_at", Types.String, [Types.String, Types.Number]),
        new("substring", Types.String, [Types.String, Types.Number, Types.Number]),
        new("contains", Types.Bool, [Types.String, Types.String]),
        new("index_of", Types.Number, [Types.String, Types.String]),
        new("replace", Types.String, [Types.String, Types.String, Types.String]),
        new("split", Types.ArrayOf(Types.String), [Types.String, Types.String]),
        new("trim", Types.String, [Types.String]),
        new("to_lower", Types.String, [Types.String]),
        new("to_upper", Types.String, [Types.String])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> args)
    {
        string value = RuntimeValue.From(args[0]).Require<string>($"String.{methodName}");
        return methodName switch
        {
            "length" => (double)value.Length,
            "char_at" => value[Convert.ToInt32(args[1])].ToString(),
            "substring" => value.Substring(Convert.ToInt32(args[1]), Convert.ToInt32(args[2])),
            "contains" => value.Contains(Convert.ToString(args[1])!, StringComparison.Ordinal),
            "index_of" => (double)value.IndexOf(Convert.ToString(args[1])!, StringComparison.Ordinal),
            "replace" => value.Replace(Convert.ToString(args[1])!, Convert.ToString(args[2])!, StringComparison.Ordinal),
            "split" => value.Split(Convert.ToString(args[1])!).Cast<object>().ToArray(),
            "trim" => value.Trim(),
            "to_lower" => value.ToLowerInvariant(),
            "to_upper" => value.ToUpperInvariant(),
            _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
        };
    }
}

internal sealed class ConvertClass : StaticBuiltInClassDefinition
{
    internal ConvertClass() : base("Convert", [
        new("to_string", Types.String, [Types.Any]),
        new("to_number", Types.Number, [Types.String])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> args) => methodName switch
    {
        "to_string" => Convert.ToString(args[0]) ?? string.Empty,
        "to_number" => double.Parse(Convert.ToString(args[0], System.Globalization.CultureInfo.InvariantCulture)!, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture),
        _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
    };
}

internal sealed class MathClass : StaticBuiltInClassDefinition
{
    internal MathClass() : base("Math", [
        new("round", Types.Number, [Types.Number]),
        new("sqrt", Types.Number, [Types.Number])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> args) => methodName switch
    {
        "round" => Math.Round(Convert.ToDouble(args[0])),
        "sqrt" => Math.Sqrt(Convert.ToDouble(args[0])),
        _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
    };
}

internal sealed class ErrorClass : StaticBuiltInClassDefinition
{
    internal ErrorClass() : base("Error", [new("create", Types.Error, [Types.String, Types.String])]) { }

    protected override object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> args) => methodName switch
    {
        "create" => new ErrorValue(args[0]?.ToString() ?? string.Empty, args[1]?.ToString() ?? string.Empty),
        _ => throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.")
    };
}
