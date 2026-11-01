namespace Benita;

/// <summary>یک نوع آرگومان در سازنده یا متد کلاس داخلی را با نوع زبان Benita توصیف می‌کند.</summary>
public sealed record BuiltInParameter(string Name, TypeSymbol Type);

/// <summary>امضای یک متد قابل فراخوانی از نمونهٔ کلاس داخلی را نگه می‌دارد.</summary>
public sealed record BuiltInMethodDescriptor(
    string Name,
    TypeSymbol ReturnType,
    IReadOnlyList<TypeSymbol> ParameterTypes);

/// <summary>نوع و قابلیت‌های دسترسی یک ویژگی کلاس داخلی را نگه می‌دارد.</summary>
public sealed record BuiltInPropertyDescriptor(
    string Name,
    TypeSymbol Type,
    bool CanRead = true,
    bool CanWrite = false);

/// <summary>نمونهٔ قابل نگهداری در مقدارهای Runtime کلاس داخلی است.</summary>
public interface IBuiltInClassInstance
{
    string ClassName { get; }
    object? InvokeMethod(string methodName, IReadOnlyList<object?> arguments);
    object? GetProperty(string propertyName);
    void SetProperty(string propertyName, object? value);

    /// <summary>یک کپی مستقل از state نمونه برای task و rollback نشست می‌سازد.</summary>
    IBuiltInClassInstance Clone();
}

/// <summary>
/// تعریف قابل ثبت یک کلاس داخلی؛ metadata را نگه می‌دارد و state قابل تغییر باید در نمونه باشد.
/// </summary>
public abstract class BuiltInClassDefinition
{
    protected BuiltInClassDefinition(
        string name,
        IReadOnlyList<BuiltInParameter>? constructorParameters = null,
        IReadOnlyList<BuiltInMethodDescriptor>? methods = null,
        IReadOnlyList<BuiltInPropertyDescriptor>? properties = null,
        IReadOnlyList<BuiltInMethodDescriptor>? staticMethods = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A built-in class must have a name.", nameof(name));

        Name = name;
        ConstructorParameters = Array.AsReadOnly((constructorParameters ?? []).ToArray());
        foreach (BuiltInParameter parameter in ConstructorParameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Name))
                throw new ArgumentException($"A constructor parameter in '{Name}' must have a name.");
        }
        Methods = CreateUniqueMap((methods ?? []).Select(method => method with
        {
            ParameterTypes = Array.AsReadOnly(method.ParameterTypes.ToArray())
        }).ToArray(), method => method.Name, "method");
        StaticMethods = CreateUniqueMap((staticMethods ?? []).Select(method => method with
        {
            ParameterTypes = Array.AsReadOnly(method.ParameterTypes.ToArray())
        }).ToArray(), method => method.Name, "static method");
        Properties = CreateUniqueMap(properties ?? [], property => property.Name, "property");

        string? duplicateMember = Methods.Keys.Intersect(Properties.Keys, StringComparer.Ordinal).FirstOrDefault();
        if (duplicateMember is not null)
            throw new ArgumentException($"Built-in class '{Name}' declares member '{duplicateMember}' more than once.");
    }

    public string Name { get; }
    public IReadOnlyList<BuiltInParameter> ConstructorParameters { get; }
    public IReadOnlyDictionary<string, BuiltInMethodDescriptor> Methods { get; }
    public IReadOnlyDictionary<string, BuiltInMethodDescriptor> StaticMethods { get; }
    public IReadOnlyDictionary<string, BuiltInPropertyDescriptor> Properties { get; }

    /// <summary>با بررسی عمومی تعداد و نوع آرگومان‌ها نمونه می‌سازد.</summary>
    public IBuiltInClassInstance Create(IReadOnlyList<object?> arguments)
    {
        ValidateArguments($"{Name} constructor", arguments, ConstructorParameters.Select(parameter => parameter.Type).ToArray());
        try
        {
            IBuiltInClassInstance instance = CreateInstance(arguments);
            if (instance is null || instance.ClassName != Name)
                throw new BuiltInException($"Constructor for '{Name}' returned an invalid instance.");
            return instance;
        }
        catch (BuiltInException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BuiltInException(exception.Message, exception);
        }
    }

    internal object? Invoke(IBuiltInClassInstance instance, string methodName, IReadOnlyList<object?> arguments)
    {
        if (!Methods.TryGetValue(methodName, out BuiltInMethodDescriptor? method))
            throw new BuiltInException($"Class '{Name}' has no method named '{methodName}'.");
        ValidateInstance(instance);
        ValidateArguments($"{Name}.{methodName}", arguments, method.ParameterTypes);
        try
        {
            object? result = instance.InvokeMethod(methodName, arguments);
            ValidateResult($"{Name}.{methodName}", result, method.ReturnType);
            return result;
        }
        catch (BuiltInException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BuiltInException(exception.Message, exception);
        }
    }

    internal object? GetProperty(IBuiltInClassInstance instance, string propertyName)
    {
        if (!Properties.TryGetValue(propertyName, out BuiltInPropertyDescriptor? property))
            throw new BuiltInException($"Class '{Name}' has no property named '{propertyName}'.");
        if (!property.CanRead)
            throw new BuiltInException($"Property '{Name}.{propertyName}' is not readable.");
        ValidateInstance(instance);
        try
        {
            object? value = instance.GetProperty(propertyName);
            ValidateResult($"{Name}.{propertyName}", value, property.Type);
            return value;
        }
        catch (BuiltInException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BuiltInException(exception.Message, exception);
        }
    }

    internal void SetProperty(IBuiltInClassInstance instance, string propertyName, object? value)
    {
        if (!Properties.TryGetValue(propertyName, out BuiltInPropertyDescriptor? property))
            throw new BuiltInException($"Class '{Name}' has no property named '{propertyName}'.");
        if (!property.CanWrite)
            throw new BuiltInException($"Property '{Name}.{propertyName}' is read-only.");
        ValidateInstance(instance);
        ValidateArguments($"{Name}.{propertyName}", [value], [property.Type]);
        try
        {
            instance.SetProperty(propertyName, value);
        }
        catch (BuiltInException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BuiltInException(exception.Message, exception);
        }
    }

    protected abstract IBuiltInClassInstance CreateInstance(IReadOnlyList<object?> arguments);

    internal object? InvokeStatic(string methodName, IReadOnlyList<object?> arguments)
    {
        if (!StaticMethods.TryGetValue(methodName, out BuiltInMethodDescriptor? method))
            throw new BuiltInException($"Class '{Name}' has no static method named '{methodName}'.");
        ValidateArguments($"{Name}.{methodName}", arguments, method.ParameterTypes);
        try
        {
            object? result = InvokeStaticMethod(methodName, arguments);
            ValidateResult($"{Name}.{methodName}", result, method.ReturnType);
            return result;
        }
        catch (BuiltInException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BuiltInException(exception.Message, exception);
        }
    }

    protected virtual object? InvokeStaticMethod(string methodName, IReadOnlyList<object?> arguments) =>
        throw new BuiltInException($"Class '{Name}' does not expose static methods.");

    private void ValidateInstance(IBuiltInClassInstance instance)
    {
        if (instance.ClassName != Name)
            throw new BuiltInException($"Expected an instance of '{Name}'.");
    }

    private static void ValidateArguments(
        string callableName,
        IReadOnlyList<object?> arguments,
        IReadOnlyList<TypeSymbol> parameterTypes)
    {
        if (arguments.Count != parameterTypes.Count)
            throw new BuiltInException(
                $"'{callableName}' expects {parameterTypes.Count} argument(s), but received {arguments.Count}.");

        for (int index = 0; index < arguments.Count; index++)
        {
            TypeSymbol actualType = RuntimeValue.From(arguments[index]).Type;
            if (actualType == Types.Unknown || !TypeFacts.IsAssignableTo(actualType, parameterTypes[index]))
                throw new BuiltInException(
                    $"Type mismatch in argument {index + 1} of '{callableName}'. Expected '{parameterTypes[index]}' but got '{actualType}'.");
        }

    }

    private static void ValidateResult(string callableName, object? value, TypeSymbol expectedType)
    {
        if (expectedType == Types.Void)
        {
            if (value is not null)
                throw new BuiltInException($"'{callableName}' must not return a value.");
            return;
        }

        TypeSymbol actualType = RuntimeValue.From(value).Type;
        if (actualType == Types.Unknown || !TypeFacts.IsAssignableTo(actualType, expectedType))
            throw new BuiltInException(
                $"'{callableName}' returned '{actualType}', but its declared return type is '{expectedType}'.");
    }

    private static IReadOnlyDictionary<string, T> CreateUniqueMap<T>(
        IReadOnlyList<T> items,
        Func<T, string> getName,
        string memberKind)
    {
        Dictionary<string, T> result = new(StringComparer.Ordinal);
        foreach (T item in items)
        {
            string itemName = getName(item);
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException($"A built-in {memberKind} must have a name.");
            if (!result.TryAdd(itemName, item))
                throw new ArgumentException($"Built-in {memberKind} '{itemName}' is declared more than once.");
        }
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, T>(result);
    }
}

/// <summary>رجیستری صریح کلاس‌های داخلی برای یک پیکربندی زبان است.</summary>
public sealed class BuiltInClassRegistry
{
    private readonly Dictionary<string, BuiltInClassDefinition> _classes = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, BuiltInClassDefinition> Classes =>
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, BuiltInClassDefinition>(_classes);

    public void Register(BuiltInClassDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (TypeFacts.FromName(definition.Name) is not NamedTypeSymbol)
            throw new ArgumentException($"'{definition.Name}' is reserved by the language type system.", nameof(definition));
        if (!_classes.TryAdd(definition.Name, definition))
            throw new InvalidOperationException($"Built-in class '{definition.Name}' is already registered.");
    }

    public bool TryGet(string? name, out BuiltInClassDefinition definition)
    {
        if (name is not null && _classes.TryGetValue(name, out BuiltInClassDefinition? value))
        {
            definition = value;
            return true;
        }
        definition = null!;
        return false;
    }

    public static BuiltInClassRegistry CreateDefault()
    {
        BuiltInClassRegistry registry = new();
        registry.Register(new ConsoleClass());
        registry.Register(new ArrayClass());
        registry.Register(new StringClass());
        registry.Register(new ConvertClass());
        registry.Register(new MathClass());
        registry.Register(new ErrorClass());
        registry.Register(new NativeCounterClass());
        registry.Register(new DateTimeClass());
        registry.Register(new FileClass());
        return registry;
    }
}

internal static class BuiltInRuntimeTypes
{
    internal static TypeSymbol FromValue(object? value) => value switch
    {
        double => Types.Number,
        string => Types.String,
        bool => Types.Bool,
        ErrorValue => Types.Error,
        IBuiltInClassInstance instance => TypeFacts.FromName(instance.ClassName),
        PackageInstance package => TypeFacts.FromName(package.InstancePackageNode.Name),
        Array => Types.AnyArray,
        _ => Types.Unknown
    };
}
