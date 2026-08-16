namespace Benita;

/// <summary>مقادیر number مربوط به index و length را بدون rounding ضمنی CLR اعتبارسنجی می‌کند.</summary>
internal static class RuntimeIndex
{
    internal static int Normalize(object value, int length, string name,
        bool allowNegative = false, bool allowEnd = false)
    {
        double number = Convert.ToDouble(value);
        string displayName = char.ToUpperInvariant(name[0]) + name[1..];
        if (!double.IsFinite(number) || number != Math.Truncate(number) ||
            number < int.MinValue || number > int.MaxValue)
            throw new ArgumentOutOfRangeException(name,
                $"{displayName} must be a whole number within the supported range.");

        int index = (int)number;
        if (allowNegative && index < 0) index += length;
        int maximum = allowEnd ? length : length - 1;
        if (index < 0 || index > maximum)
            throw new ArgumentOutOfRangeException(name, $"{displayName} is out of range.");
        return index;
    }

    internal static int NonNegativeWhole(object value, string name)
    {
        double number = Convert.ToDouble(value);
        string displayName = char.ToUpperInvariant(name[0]) + name[1..];
        if (!double.IsFinite(number) || number < 0 || number != Math.Truncate(number) ||
            number > int.MaxValue)
            throw new ArgumentOutOfRangeException(name,
                $"{displayName} must be a non-negative whole number within the supported range.");
        return (int)number;
    }
}
