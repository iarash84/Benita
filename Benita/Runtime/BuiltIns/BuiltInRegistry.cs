using Benita.itpr_df;

namespace Benita;

/// <summary>نام، امضای نوعی و سازندهٔ handler یک تابع داخلی را نگه می‌دارد.</summary>
internal sealed record BuiltInDescriptor(string Name, string ReturnType,
    IReadOnlyList<string> ParameterTypes, Func<IInterpreterClass> HandlerFactory);

/// <summary>منبع واحد مشخصات و پیاده‌سازی تمام توابع داخلی زبان است.</summary>
internal static class BuiltInRegistry
{
    private static readonly IReadOnlyDictionary<string, BuiltInDescriptor> Items = CreateDescriptors()
        .ToDictionary(item => item.Name, StringComparer.Ordinal);

    public static IReadOnlyDictionary<string, BuiltInDescriptor> Descriptors => Items;

    public static bool TryGet(string? name, out BuiltInDescriptor descriptor)
    {
        if (name is not null && Items.TryGetValue(name, out BuiltInDescriptor? value))
        {
            descriptor = value;
            return true;
        }
        descriptor = null!;
        return false;
    }

    private static IEnumerable<BuiltInDescriptor> CreateDescriptors()
    {
        yield return Define<Utility>("print", "void", "string");
        yield return Define<Utility>("input", "string");
        yield return Define<FileManagement>("file_read", "string", "string");
        yield return Define<FileManagement>("file_write", "void", "string", "string");
        yield return Define<FileManagement>("file_exist", "bool", "string");
        yield return Define<FileManagement>("file_delete", "bool", "string");
        yield return Define<ArrayManagement>("array_len", "number", "array");
        yield return Define<ArrayManagement>("array_add", "array", "array", "string");
        yield return Define<ArrayManagement>("array_remove", "array", "array", "number");
        yield return Define<ArrayManagement>("array_contains", "bool", "array", "string");
        yield return Define<ArrayManagement>("array_index_of", "number", "array", "string");
        yield return Define<ArrayManagement>("array_reverse", "array", "array");
        yield return Define<ArrayManagement>("array_clear", "array", "array");
        yield return Define<ArrayManagement>("array_insert", "array", "array", "number", "string");
        yield return Define<ArrayManagement>("array_slice", "array", "array", "number", "number");
        yield return Define<ArrayManagement>("array_concat", "array", "array", "array");
        yield return Define<ArrayManagement>("array_sort", "array", "array");
        yield return Define<Utility>("to_string", "string", "number");
        yield return Define<Utility>("to_number", "number", "string");
        yield return Define<Utility>("round_number", "number", "number");
        yield return Define<Utility>("sqrt_number", "number", "number");
        yield return Define<StringManagement>("string_len", "number", "string");
        yield return Define<StringManagement>("string_char_at", "string", "string", "number");
        yield return Define<StringManagement>("string_substring", "string", "string", "number", "number");
        yield return Define<StringManagement>("string_contains", "bool", "string", "string");
        yield return Define<StringManagement>("string_index_of", "number", "string", "string");
        yield return Define<StringManagement>("string_replace", "string", "string", "string", "string");
        yield return Define<StringManagement>("string_split", "string[]", "string", "string");
        yield return Define<StringManagement>("string_trim", "string", "string");
        yield return Define<StringManagement>("string_to_lower", "string", "string");
        yield return Define<StringManagement>("string_to_upper", "string", "string");
    }

    private static BuiltInDescriptor Define<T>(string name, string result, params string[] parameters)
        where T : IInterpreterClass, new() => new(name, result, parameters, static () => new T());
}
