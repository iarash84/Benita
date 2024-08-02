namespace Benita.itpr_df
{
    public class ArrayManagement : IInterpreterClass
    {
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
    }
}
