using Benita.Cg_df;
using Benita.itpr_df;

namespace Benita
{
    /// <summary>
    /// The FactoryClass is responsible for providing instances of interpreter and code generator classes based on function names.
    /// </summary>
    internal class FactoryClass
    {
        /// <summary>
        /// A dictionary mapping function names to their respective interpreter and code generator types.
        /// </summary>
        private static readonly Dictionary<string?, (Type, Type)> FunctionMappings = new Dictionary<string?, (Type, Type)>
        {
            { "array_len", (typeof(ArrayManagement),typeof(CgArrayManagement)) },
            { "array_add", (typeof(ArrayManagement),typeof(CgArrayManagement)) },
            { "array_remove", (typeof(ArrayManagement),typeof(CgArrayManagement)) },
            { "file_read", (typeof(FileManagement),typeof(CgFileManagement)) },
            { "file_write", (typeof(FileManagement),typeof(CgFileManagement)) },
            { "file_exist", (typeof(FileManagement),typeof(CgFileManagement)) },
            { "file_delete", (typeof(FileManagement),typeof(CgFileManagement)) },
            { "print", (typeof(Utility),typeof(CgUtility)) },
            { "input", (typeof(Utility),typeof(CgUtility)) },
            { "to_string", (typeof(Utility),typeof(CgUtility)) },
            { "to_number", (typeof(Utility),typeof(CgUtility)) }
        };

        /// <summary>
        /// Gets an instance of the interpreter class associated with the given function name.
        /// </summary>
        /// <param name="functionName">The name of the function to get the interpreter class for.</param>
        /// <returns>An instance of the interpreter class, or null if the function name is not found.</returns>
        public static IInterpreterClass? GetInterpreterClass(string? functionName)
        {
            if (FunctionMappings.TryGetValue(functionName, out var factoryType))
            {
                return (IInterpreterClass)Activator.CreateInstance(factoryType.Item1)!;
            }
            return null;
        }

        /// <summary>
        /// Gets an instance of the code generator class associated with the given function name.
        /// </summary>
        /// <param name="functionName">The name of the function to get the code generator class for.</param>
        /// <returns>An instance of the code generator class, or null if the function name is not found.</returns>
        public static ICodeGeneratorClass? GetCodeGeneratorClass(string? functionName)
        {
            if (FunctionMappings.TryGetValue(functionName, out var factoryType))
            {
                return (ICodeGeneratorClass)Activator.CreateInstance(factoryType.Item2)!;
            }
            return null;
        }
    }
}
