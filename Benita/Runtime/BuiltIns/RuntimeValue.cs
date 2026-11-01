namespace Benita;

/// <summary>
/// قرارداد مستقل مقدار runtime در مرز built-inها. این abstraction به‌جای افشای
/// جزئیات مقدارهای داخلی مفسر، نوع زبان و مقدار قابل انتقال را مشخص می‌کند.
/// پیاده‌سازی فعلی از object به‌عنوان bridge استفاده می‌کند تا با runtime موجود سازگار بماند.
/// </summary>
public readonly record struct RuntimeValue(TypeSymbol Type, object? Value)
{
    public static RuntimeValue From(object? value) => new(BuiltInRuntimeTypes.FromValue(value), value);

    public T Require<T>(string operation)
    {
        if (Value is T typedValue)
            return typedValue;
        throw new BuiltInException(
            $"'{operation}' received a runtime value incompatible with '{typeof(T).Name}'.");
    }
}

/// <summary>نوعی مستقل از implementation برای توصیف امضای زبان در adapterها.</summary>
public readonly record struct RuntimeTypeDescriptor(string Name)
{
    public static RuntimeTypeDescriptor From(TypeSymbol type) => new(type.Name);
}
