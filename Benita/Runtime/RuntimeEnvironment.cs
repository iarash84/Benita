namespace Benita;

/// <summary>محیط اجرای زنجیره‌ای برای نگهداری مقدارهای local و دسترسی به scopeهای والد.</summary>
public sealed class RuntimeEnvironment
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

    public RuntimeEnvironment(RuntimeEnvironment? parent = null, string? name = null)
    {
        Parent = parent;
        Name = name ?? "runtime-scope";
    }

    public RuntimeEnvironment? Parent { get; }
    public string Name { get; }
    public IReadOnlyDictionary<string, object?> Values => _values;

    /// <summary>فقط در scope جاری مقدار جدید ثبت می‌کند.</summary>
    public void Declare(string name, object? value)
    {
        if (!_values.TryAdd(name, value))
            throw new InvalidOperationException($"Runtime name '{name}' is already declared in scope '{Name}'.");
    }

    /// <summary>نام را از scope جاری به سمت والدها resolve می‌کند.</summary>
    public bool TryGet(string name, out object? value)
    {
        if (_values.TryGetValue(name, out value))
            return true;
        if (Parent is not null)
            return Parent.TryGet(name, out value);
        value = null;
        return false;
    }

    /// <summary>مقدار موجود را در نزدیک‌ترین scope دارای نام به‌روزرسانی می‌کند.</summary>
    public bool TryAssign(string name, object? value)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = value;
            return true;
        }
        return Parent?.TryAssign(name, value) ?? false;
    }

    /// <summary>مقدار را در scope جاری بدون جست‌وجوی والد جایگزین یا اضافه می‌کند.</summary>
    public void SetLocal(string name, object? value) => _values[name] = value;

    public bool ContainsLocal(string name) => _values.ContainsKey(name);

    public bool RemoveLocal(string name) => _values.Remove(name);

    public void Clear() => _values.Clear();
}
