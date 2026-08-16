namespace Benita
{
    /// <summary>نقطهٔ ورود برنامه و مسئول تفسیر فرمان‌ها و گزینه‌های خط فرمان است.</summary>
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (BenitaException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
            catch (IOException exception)
            {
                Console.Error.WriteLine($"BEN0002: File operation failed: {exception.Message}");
                return 1;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(new RuntimeException(exception.Message, exception).Message);
                return 1;
            }
        }

        private static void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            if (args[0] is "-help" or "help")
            {
                ShowHelp();
                return;
            }

            bool printTokens = args.Contains("-t");
            bool printSource = args.Contains("-s");
            bool printAst = args.Contains("-a");
            bool optimizeAst = args.Contains("--optimize");

            args = args.Where(arg => arg != "-t" && arg != "-s" && arg != "-a" && arg != "--optimize").ToArray();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string action = args[0].ToLower();

            var compiler = new CompilerClass();

            switch (action)
            {
                case "repl":
                    new Repl().Run(printTokens, printAst, printSource, optimizeAst);
                    return;
                case "exc":
                    ExecuteFile(args, printTokens, printAst, printSource, compiler, optimizeAst: optimizeAst);
                    Console.WriteLine("Program executed successfully.");
                    break;
                case "dxc":
                    ExecuteFile(args, printTokens, printAst, printSource, compiler, true, optimizeAst);
                    Console.WriteLine("Program executed successfully.");
                    break;
                case "check":
                    CheckFile(args, printTokens, printAst, printSource, compiler);
                    break;
                default:
                    throw new ArgumentException("Unknown action - " + action);
            }
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static void ExecuteFile(string[] args, bool printTokens, bool printAst, bool printSource, CompilerClass compiler,
            bool debugModeAvailable = false, bool optimizeAst = false)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("A .ben file path is required for the execute command.");
            }

            string filePath = args[1];
            ValidateFile(filePath);

            string fileContent = File.ReadAllText(filePath);
            compiler.Exec(fileContent, printTokens, printAst, printSource, debugModeAvailable, filePath, optimizeAst);
        }

        static void CheckFile(string[] args, bool printTokens, bool printAst, bool printSource, CompilerClass compiler)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("A .ben file path is required for the check command.");
            }

            string filePath = args[1];
            ValidateFile(filePath);

            string fileContent = File.ReadAllText(filePath);
            compiler.Check(fileContent, printTokens, printAst, printSource, filePath);
            Console.WriteLine($"Check passed: {filePath}");
        }

        static void ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            if (!string.Equals(Path.GetExtension(filePath), ".ben", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid file extension. Only .ben files are allowed.",
                    nameof(filePath));
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
            Console.WriteLine($"  Version {AppVersion.Current}");
            Console.WriteLine("Usage: Program [action] [filePath] [options]");
            Console.WriteLine("Actions:");
            Console.WriteLine("  exc        - Execute the code in the file.");
            Console.WriteLine("  dxc       - Execute the code in the file in debug mode.");
            Console.WriteLine("  check      - Check syntax and semantics without executing the program.");
            Console.WriteLine("  repl       - Start the interactive REPL.");
            Console.WriteLine("  help       - Show this help message.");
            Console.WriteLine("Options:");
            Console.WriteLine("  -a         - Print the AST.");
            Console.WriteLine("  -t         - Print the tokens.");
            Console.WriteLine("  -s         - Print the Source.");
            Console.WriteLine("  --optimize - Optimize the AST before execution.");
        }
    }
}
