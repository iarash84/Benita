namespace Benita.itpr_df
{
    /// <summary>توابع داخلی جست‌وجو، تغییر ساختار و مرتب‌سازی آرایه‌ها را اجرا می‌کند.</summary>
    public class ArrayManagement : IInterpreterClass
    {
        /// <summary>تابع آرایه متناظر با نام دریافتی را اجرا می‌کند.</summary>
        public object HandleFunctionCall(string? functionName, List<object> arguments)
        {
            switch (functionName)
            {
                case "array_len":
                    return ArrayLen(arguments);
                case "array_add":
                    return ArrayAdd(arguments);
                case "array_remove":
                    return ArrayRemove(arguments);
                case "array_contains": return ArrayContains(arguments);
                case "array_index_of": return ArrayIndexOf(arguments);
                case "array_reverse": return ArrayReverse(arguments);
                case "array_clear": return ArrayClear(arguments);
                case "array_insert": return ArrayInsert(arguments);
                case "array_slice": return ArraySlice(arguments);
                case "array_concat": return ArrayConcat(arguments);
                case "array_sort": return ArraySort(arguments);
                default:
                    throw new Exception($"Unknown function '{functionName}'");
            }
        }

        private object ArrayLen(List<object> arguments)
        {
            var array = arguments[0];
            if (array is Array arr)
            {
                return arr.Length;
            }
            throw new Exception("Argument to array_len must be an array");
        }

        private object ArrayAdd(List<object> arguments)
        {
            var array = arguments[0];
            var value = arguments[1];
            if (array is Array arr)
            {
                object[] newArray = new object[arr.Length + 1];
                arr.CopyTo(newArray, 0);
                newArray[arr.Length] = value;
                return newArray;
            }
            throw new Exception("Argument to array_add must be an array");
        }

        private object ArrayRemove(List<object> arguments)
        {
            var array = arguments[0];
            var index = Convert.ToInt32(arguments[1]);
            if (array is Array arr)
            {
                if (index < 0 || index >= arr.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                object[] newArray = new object[arr.Length - 1];
                int newArrayIndex = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == index) continue;
                    newArray[newArrayIndex++] = arr.GetValue(i);
                }

                return newArray;
            }
            throw new Exception("Argument to array_remove must be an array");
        }

        private static object ArrayContains(List<object> arguments) =>
            RequireArray(arguments[0], "array_contains").Cast<object?>().Any(item => Equals(item, arguments[1]));

        private static object ArrayIndexOf(List<object> arguments)
        {
            var array = RequireArray(arguments[0], "array_index_of");
            for (int index = 0; index < array.Length; index++)
                if (Equals(array.GetValue(index), arguments[1])) return index;
            return -1;
        }

        private static object ArrayReverse(List<object> arguments)
        {
            var result = Copy(RequireArray(arguments[0], "array_reverse"));
            Array.Reverse(result);
            return result;
        }

        private static object ArrayClear(List<object> arguments)
        {
            RequireArray(arguments[0], "array_clear");
            return Array.Empty<object>();
        }

        private static object ArrayInsert(List<object> arguments)
        {
            var array = RequireArray(arguments[0], "array_insert");
            int index = Convert.ToInt32(arguments[1]);
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            var result = new object[array.Length + 1];
            for (int source = 0, target = 0; target < result.Length; target++)
                result[target] = target == index ? arguments[2] : array.GetValue(source++)!;
            return result;
        }

        private static object ArraySlice(List<object> arguments)
        {
            var array = RequireArray(arguments[0], "array_slice");
            int start = Convert.ToInt32(arguments[1]);
            int length = Convert.ToInt32(arguments[2]);
            if (start < 0 || length < 0 || start > array.Length - length) throw new ArgumentOutOfRangeException(nameof(start), "Slice range is out of bounds.");
            var result = new object[length];
            for (int index = 0; index < length; index++) result[index] = array.GetValue(start + index)!;
            return result;
        }

        private static object ArrayConcat(List<object> arguments)
        {
            var first = RequireArray(arguments[0], "array_concat");
            var second = RequireArray(arguments[1], "array_concat");
            var result = new object[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }

        private static object ArraySort(List<object> arguments)
        {
            var result = Copy(RequireArray(arguments[0], "array_sort"));
            Array.Sort(result, static (left, right) => left is IComparable comparable ? comparable.CompareTo(right) : throw new InvalidOperationException("Array elements must be comparable."));
            return result;
        }

        private static Array RequireArray(object value, string name) => value is Array array ? array : throw new Exception($"Argument to {name} must be an array");

        private static object[] Copy(Array array)
        {
            var result = new object[array.Length];
            array.CopyTo(result, 0);
            return result;
        }
    }
}
