namespace Benita
{
    internal static class Globals
    {
        public static Dictionary<string?, object> GlobalVariable = [];
        public static Dictionary<string?, FunctionNode?> GlobalFunctions = [];
        public static Dictionary<string?, PackageNode> PackageList = [];
    }
}
