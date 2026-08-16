using Benita.itpr_df;

namespace Benita;

/// <summary>نام، امضای نوعی و سازندهٔ handler یک تابع داخلی را نگه می‌دارد.</summary>
internal sealed record BuiltInDescriptor(string Name, TypeSymbol ReturnType,
    IReadOnlyList<TypeSymbol> ParameterTypes, Func<IInterpreterClass> HandlerFactory);

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
        yield return Define<Utility>("print", Types.Void, Types.Any);
        yield return Define<Utility>("input", Types.String);
        yield return Define<FileManagement>("file_read", Types.String, Types.String);
        yield return Define<FileManagement>("file_write", Types.Void, Types.String, Types.String);
        yield return Define<FileManagement>("file_exist", Types.Bool, Types.String);
        yield return Define<FileManagement>("file_delete", Types.Bool, Types.String);
        yield return Define<ArrayManagement>("array_len", Types.Number, Types.AnyArray);
        yield return Define<ArrayManagement>("array_add", Types.AnyArray, Types.AnyArray, Types.Any);
        yield return Define<ArrayManagement>("array_remove", Types.AnyArray, Types.AnyArray, Types.Number);
        yield return Define<ArrayManagement>("array_contains", Types.Bool, Types.AnyArray, Types.Any);
        yield return Define<ArrayManagement>("array_index_of", Types.Number, Types.AnyArray, Types.Any);
        yield return Define<ArrayManagement>("array_reverse", Types.AnyArray, Types.AnyArray);
        yield return Define<ArrayManagement>("array_clear", Types.AnyArray, Types.AnyArray);
        yield return Define<ArrayManagement>("array_insert", Types.AnyArray, Types.AnyArray, Types.Number, Types.Any);
        yield return Define<ArrayManagement>("array_slice", Types.AnyArray, Types.AnyArray, Types.Number, Types.Number);
        yield return Define<ArrayManagement>("array_concat", Types.AnyArray, Types.AnyArray, Types.AnyArray);
        yield return Define<ArrayManagement>("array_sort", Types.AnyArray, Types.AnyArray);
        yield return Define<Utility>("to_string", Types.String, Types.Number);
        yield return Define<Utility>("to_number", Types.Number, Types.String);
        yield return Define<Utility>("round_number", Types.Number, Types.Number);
        yield return Define<Utility>("sqrt_number", Types.Number, Types.Number);
        yield return Define<ErrorManagement>("error", Types.Error, Types.String, Types.String);
        yield return Define<StringManagement>("string_len", Types.Number, Types.String);
        yield return Define<StringManagement>("string_char_at", Types.String, Types.String, Types.Number);
        yield return Define<StringManagement>("string_substring", Types.String, Types.String, Types.Number, Types.Number);
        yield return Define<StringManagement>("string_contains", Types.Bool, Types.String, Types.String);
        yield return Define<StringManagement>("string_index_of", Types.Number, Types.String, Types.String);
        yield return Define<StringManagement>("string_replace", Types.String, Types.String, Types.String, Types.String);
        yield return Define<StringManagement>("string_split", Types.ArrayOf(Types.String), Types.String, Types.String);
        yield return Define<StringManagement>("string_trim", Types.String, Types.String);
        yield return Define<StringManagement>("string_to_lower", Types.String, Types.String);
        yield return Define<StringManagement>("string_to_upper", Types.String, Types.String);
    }

    private static BuiltInDescriptor Define<T>(string name, TypeSymbol result, params TypeSymbol[] parameters)
        where T : IInterpreterClass, new() => new(name, result, parameters, static () => new T());
}
