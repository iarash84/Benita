using Benita.itpr_df;

namespace Benita
{
    /// <summary>
    /// Creates the interpreter implementation associated with each built-in function.
    /// </summary>
    internal static class FactoryClass
    {
        private static readonly Dictionary<string, Type> FunctionMappings = new()
        {
            { "array_len", typeof(ArrayManagement) },
            { "array_add", typeof(ArrayManagement) },
            { "array_remove", typeof(ArrayManagement) },
            { "file_read", typeof(FileManagement) },
            { "file_write", typeof(FileManagement) },
            { "file_exist", typeof(FileManagement) },
            { "file_delete", typeof(FileManagement) },
            { "print", typeof(Utility) },
            { "input", typeof(Utility) },
            { "to_string", typeof(Utility) },
            { "to_number", typeof(Utility) },
            { "round_number", typeof(Utility) },
            { "sqrt_number", typeof(Utility) },
            { "string_len", typeof(StringManagement) },
            { "string_char_at", typeof(StringManagement) },
            { "string_substring", typeof(StringManagement) },
            { "string_contains", typeof(StringManagement) },
            { "string_index_of", typeof(StringManagement) },
            { "string_replace", typeof(StringManagement) },
            { "string_split", typeof(StringManagement) },
            { "string_trim", typeof(StringManagement) },
            { "string_to_lower", typeof(StringManagement) },
            { "string_to_upper", typeof(StringManagement) }
        };

        public static IInterpreterClass? GetInterpreterClass(string? functionName)
        {
            if (functionName is null || !FunctionMappings.TryGetValue(functionName, out Type? implementationType))
                return null;

            return (IInterpreterClass)Activator.CreateInstance(implementationType)!;
        }
    }
}
