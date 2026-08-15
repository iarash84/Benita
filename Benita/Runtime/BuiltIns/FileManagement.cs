namespace Benita.itpr_df
{
    /// <summary>توابع داخلی خواندن، نوشتن و مدیریت فایل را اجرا می‌کند.</summary>
    public class FileManagement : IInterpreterClass
    {
        /// <summary>عملیات فایل متناظر با نام دریافتی را اجرا می‌کند.</summary>
        public object HandleFunctionCall(string? functionName, List<object> arguments)
        {
            switch (functionName)
            {
                case "file_read":
                    return FileRead(arguments);
                case "file_write":
                    return FileWrite(arguments);
                case "file_exist":
                    return FileExist(arguments);
                case "file_delete":
                    return FileDelete(arguments);
                default:
                    throw new Exception($"Unknown function '{functionName}'");
            }
        }

        private object FileRead(List<object> arguments)
        {
            var filePath = (string)arguments[0];
            return File.ReadAllText(filePath);
        }

        private object FileWrite(List<object> arguments)
        {
            var filePath = (string)arguments[0];
            var content = (string)arguments[1];
            File.WriteAllText(filePath, content);
            return null;
        }

        private object FileExist(List<object> arguments)
        {
            var filePath = (string)arguments[0];
            return File.Exists(filePath);
        }

        private object FileDelete(List<object> arguments)
        {
            var filePath = (string)arguments[0];
            File.Delete(filePath);
            return !File.Exists(filePath);
        }
    }
}
