namespace Benita;

/// <summary>برابری و ترتیب مقادیر runtime را بر اساس نوع‌های Benita و مستقل از representation CLR محاسبه می‌کند.</summary>
internal static class RuntimeValueComparer
{
    internal static bool AreEqual(object? left, object? right)
    {
        if (left is not null && right is not null && IsNumber(left) && IsNumber(right))
            return Convert.ToDouble(left) == Convert.ToDouble(right);
        return Equals(left, right);
    }

    internal static int Compare(object? left, object? right)
    {
        if (left is not null && right is not null && IsNumber(left) && IsNumber(right))
            return Convert.ToDouble(left).CompareTo(Convert.ToDouble(right));
        if (left is IComparable comparable)
            return comparable.CompareTo(right);
        throw new InvalidOperationException("Array elements must be comparable.");
    }

    private static bool IsNumber(object value) => Type.GetTypeCode(value.GetType()) is
        TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or
        TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
        TypeCode.Single or TypeCode.Double or TypeCode.Decimal;
}
