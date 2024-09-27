using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benita
{
    public class DebugClass
    {
        private static DebugClass _instance;
        private static readonly object LockObj = new();
        private int _debugId;

        private DebugClass()
        {
        }

        public static DebugClass Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (LockObj)
                {
                    _instance ??= new DebugClass();
                }
                return _instance;
            }
        }


        // Method to output the current state of the interpreter
        public void DebugLog(string message, Dictionary<string, object> variables,
            Dictionary<string, object> outerScopeVariables, Dictionary<string, FunctionNode> functions,
            bool pressKeyWait)
        {

            SetColor(ConsoleColor.Green);
            Console.WriteLine($"[{_debugId++}] [DEBUG] " + message);
            ResetColor();

            if (pressKeyWait)
            {
                SetColor(ConsoleColor.Cyan);
                Console.WriteLine("Formatted Date and Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                ResetColor();

                if (Globals.GlobalVariable.Count > 0)
                {
                    SetColor(ConsoleColor.Yellow);
                    Console.WriteLine("Current GlobalVariable: ");
                    ResetColor();
                    foreach (KeyValuePair<string, object> kvp in Globals.GlobalVariable)
                    {
                        PrintKeyValuePair(kvp);
                    }
                }

                if (variables.Count > 0)
                {
                    SetColor(ConsoleColor.Yellow);
                    Console.WriteLine("Current Variables: ");
                    ResetColor();
                    foreach (KeyValuePair<string, object> kvp in variables)
                    {
                        PrintKeyValuePair(kvp);
                    }
                }

                if (outerScopeVariables.Count > 0)
                {
                    SetColor(ConsoleColor.Yellow);
                    Console.WriteLine("Outer Scope Variables: ");
                    ResetColor();
                    foreach (KeyValuePair<string, object> kvp in outerScopeVariables)
                    {
                        PrintKeyValuePair(kvp);
                    }
                }

                if (functions.Count > 0)
                {
                    SetColor(ConsoleColor.Yellow);
                    Console.WriteLine("Functions: ");
                    ResetColor();
                    foreach (KeyValuePair<string, FunctionNode> kvp in functions)
                    {
                        PrintKeyValuePair(kvp);
                    }
                }

                if (Globals.PackageList.Count > 0)
                {
                    SetColor(ConsoleColor.Yellow);
                    Console.WriteLine("Packages: ");
                    ResetColor();
                    foreach (KeyValuePair<string, PackageNode> kvp in Globals.PackageList)
                    {
                        PrintKeyValuePair(kvp);
                    }
                }
                SetColor(ConsoleColor.Gray);
                Console.WriteLine("---------------------------------------------------------------");
                ResetColor();
                Console.ReadKey();
            }
        }

        private static void PrintKeyValuePair(KeyValuePair<string, object> kvp)
        {
            SetColor(ConsoleColor.White);
            Console.Write($"  {kvp.Key}: ");
            ResetColor();

            if (kvp.Value is Array array) // Check if the value is an array
            {
                SetColor(ConsoleColor.Magenta);
                Console.Write("[");
                foreach (object item in array)
                {
                    Console.Write($"    {item},");
                }
                Console.WriteLine("  ]");
                ResetColor();
            }
            else if (kvp.Value is PackageInstance packageInstance)
            {
                SetColor(ConsoleColor.Magenta);
                Console.WriteLine($"{packageInstance.InstancePackageNode.Name}");
                ResetColor();
            }
            else
            {
                SetColor(ConsoleColor.Magenta);
                Console.WriteLine(kvp.Value);
                ResetColor();
            }
        }

        private static void PrintKeyValuePair(KeyValuePair<string, FunctionNode> kvp)
        {
            SetColor(ConsoleColor.White);
            Console.Write($"  {kvp.Key} (");
            ResetColor();

            if (kvp.Value is { } functionNode)
            {
                SetColor(ConsoleColor.DarkCyan);
                for (int i = 0; i < functionNode.Parameters.Count; i++)
                {
                    Console.Write($"{functionNode.Parameters[i].Type} {functionNode.Parameters[i].Name}");
                    if (i < functionNode.Parameters.Count - 1)
                    {
                        Console.Write(", ");
                    }
                }
                SetColor(ConsoleColor.White);
                Console.Write(")");
                SetColor(ConsoleColor.DarkCyan);
                Console.WriteLine($" -> {functionNode.ReturnType}");
                ResetColor();
            }
        }

        private static void PrintKeyValuePair(KeyValuePair<string, PackageNode> kvp)
        {
            SetColor(ConsoleColor.White);
            Console.WriteLine($"    PackageName = {kvp.Key},");
            ResetColor();

            if (kvp.Value is { } packageNode)
            {
                foreach (var member in packageNode.Members)
                {
                    if (member is PackageVariableDeclarationNode field)
                    {
                        SetColor(ConsoleColor.DarkGreen);
                        Console.WriteLine($"        VariableDeclaration:  {field.Type} {field.Name}");
                        ResetColor();
                    }
                }

                foreach (var member in packageNode.Members)
                {
                    if (member is PackageFunctionNode method)
                    {
                        SetColor(ConsoleColor.DarkGreen);
                        Console.Write($"        Function:   {method.Name} (");
                        ResetColor();

                        if (kvp.Value is { } functionNode)
                        {
                            SetColor(ConsoleColor.DarkYellow);
                            for (int i = 0; i < method.Parameters.Count; i++)
                            {
                                Console.Write($"{method.Parameters[i].Type} {method.Parameters[i].Name}");
                                if (i < method.Parameters.Count - 1)
                                {
                                    Console.Write(", ");
                                }
                            }
                            SetColor(ConsoleColor.DarkGreen);
                            Console.Write(")");
                            SetColor(ConsoleColor.DarkYellow);
                            Console.WriteLine($" -> {method.ReturnType}");
                            ResetColor();
                        }
                    }
                }
            }
        }

        private static void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        private static void ResetColor()
        {
            Console.ResetColor();
        }
    }
}
