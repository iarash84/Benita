namespace Benita
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-help")
            {
                ShowHelp();
                return;
            }

            bool printTokens = args.Contains("-t");
            bool printSource = args.Contains("-s");
            bool printAst = args.Contains("-a");

            args = args.Where(arg => arg != "-t" && arg != "-s" && arg != "-a").ToArray();

            string action = args[0].ToLower();

            var compiler = new CompilerClass();

            switch (action)
            {
                case "edr":
                    var editor = new Editor();
                    editor.Run(printTokens, printAst, printSource);
                    break;
                case "exc":
                    ExecuteFile(args, printTokens, printAst, printSource, compiler);
                    Console.WriteLine("Program executed successfully.");
                    break;
                case "dxc":
                    ExecuteFile(args, printTokens, printAst, printSource, compiler, true);
                    Console.WriteLine("Program executed successfully.");
                    break;
                case "ccg":
                    GenerateCppCode(args, printTokens, printAst, printSource, compiler);
                    break;
                default:
                    Console.WriteLine("Error: Unknown action - " + action);
                    ShowHelp();
                    break;
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static void ExecuteFile(string[] args, bool printTokens, bool printAst, bool printSource, CompilerClass compiler, bool debugModeAvailable = false)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: File path is required.");
                return;
            }

            string filePath = args[1];
            if (!ValidateFile(filePath)) return;

            string fileContent = File.ReadAllText(filePath);
            compiler.Exec(fileContent, printTokens, printAst, printSource, debugModeAvailable);
        }

        static void GenerateCppCode(string[] args, bool printTokens, bool printAst, bool printSource, CompilerClass compiler)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: File path is required.");
                return;
            }

            string filePath = args[1];
            if (!ValidateFile(filePath)) return;

            string fileContent = File.ReadAllText(filePath);
            string outputFilePath = args.Length > 2 ? args[2] : string.Empty;
            var generatedCode = compiler.GenerateCppCode(fileContent, printTokens, printAst, printSource);

            if (!string.IsNullOrEmpty(outputFilePath))
                File.WriteAllText(outputFilePath, generatedCode);
            else
                Console.WriteLine(generatedCode);

            Console.WriteLine($"C++ code generated at: {outputFilePath}");
        }

        static bool ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: File not found - " + filePath);
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".ben")
            {
                Console.WriteLine("Error: Invalid file extension. Only .ben files are allowed.");
                return false;
            }

            return true;
        }

        static void ShowHelp()
        {
            Console.WriteLine("__________              .__  __           .____                           ");
            Console.WriteLine("\\______   \\ ____   ____ |__|/  |______    |    |   _____    ____    ____  ");
            Console.WriteLine(" |    |  _// __ \\ /    \\|  \\   __\\__  \\   |    |   \\__  \\  /    \\  / ___\\ ");
            Console.WriteLine(" |    |   \\  ___/|   |  \\  ||  |  / __ \\_ |    |___ / __ \\|   |  \\/ /_/  >");
            Console.WriteLine(" |______  /\\___  >___|  /__||__| (____  / |_______ (____  /___|  /\\___  / ");
            Console.WriteLine("        \\/     \\/     \\/              \\/          \\/    \\/     \\//_____/ ");
            Console.WriteLine("  (c) Adm, 2024");
            Console.WriteLine("  Version 0.4.3");
            Console.WriteLine("Usage: Program <action> <filePath> [outputFilePath] [-p] [-t]");
            Console.WriteLine("Actions:");
            Console.WriteLine("  exc        - Execute the code in the file.");
            Console.WriteLine("  dxc       - Execute the code in the file in debug mode.");
            Console.WriteLine("  ccg        - Generate C++ code from the file content and save to output file.");
            Console.WriteLine("  edr        - Open the text editor.");
            Console.WriteLine("  help       - Show this help message.");
            Console.WriteLine("Options:");
            Console.WriteLine("  -a         - Print the AST.");
            Console.WriteLine("  -t         - Print the tokens.");
            Console.WriteLine("  -s         - Print the Source.");
        }
    }
}
