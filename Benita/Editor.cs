namespace Benita
{
    public class Editor
    {
        public void Run(bool printTokens, bool printAst, bool printSource)
        {
            bool continueEditing = true;

            CompilerClass compiler = new CompilerClass();
            while (continueEditing)
            {
                var lines = new List<string>();
                int currentLine = 0;
                int currentColumn = 0;

                Console.Clear();
                Console.WriteLine("Enter text (type 'RUN' on a new line to save and exit):");

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        if (lines.Count == currentLine)
                        {
                            lines.Add(string.Empty);
                        }

                        if (lines[currentLine] == "RUN")
                        {
                            break;
                        }

                        lines.Insert(currentLine + 1, string.Empty);
                        currentLine++;
                        currentColumn = 0;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (currentColumn > 0)
                        {
                            lines[currentLine] = lines[currentLine].Remove(currentColumn - 1, 1);
                            currentColumn--;
                        }
                        else if (currentLine > 0)
                        {
                            currentColumn = lines[currentLine - 1].Length;
                            lines[currentLine - 1] += lines[currentLine];
                            lines.RemoveAt(currentLine);
                            currentLine--;
                        }
                    }
                    else if (key.Key == ConsoleKey.Delete)
                    {
                        if (currentColumn < lines[currentLine].Length)
                        {
                            lines[currentLine] = lines[currentLine].Remove(currentColumn, 1);
                        }
                        else if (currentLine < lines.Count - 1)
                        {
                            lines[currentLine] += lines[currentLine + 1];
                            lines.RemoveAt(currentLine + 1);
                        }
                    }
                    else if (key.Key == ConsoleKey.LeftArrow)
                    {
                        if (currentColumn > 0)
                        {
                            currentColumn--;
                        }
                        else if (currentLine > 0)
                        {
                            currentLine--;
                            currentColumn = lines[currentLine].Length;
                        }
                    }
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        if (currentColumn < lines[currentLine].Length)
                        {
                            currentColumn++;
                        }
                        else if (currentLine < lines.Count - 1)
                        {
                            currentLine++;
                            currentColumn = 0;
                        }
                    }
                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (currentLine > 0)
                        {
                            currentLine--;
                            currentColumn = Math.Min(currentColumn, lines[currentLine].Length);
                        }
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (currentLine < lines.Count - 1)
                        {
                            currentLine++;
                            currentColumn = Math.Min(currentColumn, lines[currentLine].Length);
                        }
                    }
                    else
                    {
                        if (lines.Count == currentLine)
                        {
                            lines.Add(string.Empty);
                        }

                        lines[currentLine] = lines[currentLine].Insert(currentColumn, key.KeyChar.ToString());
                        currentColumn++;
                    }

                    Console.Clear();
                    foreach (var line in lines)
                    {
                        PrintSyntaxHighlight(line);
                    }

                    Console.SetCursorPosition(currentColumn, currentLine);
                }

                var sourceCode = string.Join(Environment.NewLine, lines);
                sourceCode = RemoveFromEnd(sourceCode, "RUN");

                Console.WriteLine();
                Console.WriteLine("The result of executing of code :");

                compiler.Exec(sourceCode, printTokens, printAst, printSource);

                Console.WriteLine();
                Console.WriteLine("Do you want to write another code? (y/n):");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    continueEditing = false;
                }
            }
        }

        private void PrintSyntaxHighlight(string line)
        {
            // Define colors
            var colors = new Dictionary<string, ConsoleColor>
            {
                { "keyword", ConsoleColor.Blue },
                { "type", ConsoleColor.DarkRed },
                { "comment", ConsoleColor.DarkGreen },
                { "defaultFunction", ConsoleColor.Yellow },
                { "default", ConsoleColor.Gray },
                { "operator", ConsoleColor.Cyan }
            };

            string[] keywords =
                { "include_once", "_main_", "pkg", "func", "if", "while", "for", "return", "true", "false", "else" };
            string[] types = { "number", "string", "bool", "void", "let" };
            string[] defaultFunctions =
            {
                "array_len", "array_add", "array_remove", "file_read", "file_write", "file_exist", "file_delete",
                "print", "input", "to_string", "to_number"
            };
            string[] operatorStrings = { "(", ")", "{", "}", "->", ";" };

            int index = 0;
            while (index < line.Length)
            {
                // Handle comments
                if (line.Substring(index).StartsWith("//"))
                {
                    PrintColored(line.Substring(index), colors["comment"]);
                    return;
                }

                if (line.Substring(index).StartsWith("/*"))
                {
                    int endIndex = line.IndexOf("*/", index, StringComparison.Ordinal) + 2;
                    if (endIndex > 1)
                    {
                        PrintColored(line.Substring(index, endIndex - index), colors["comment"]);
                        index = endIndex;
                        continue;
                    }
                }

                // Handle keywords, types, default functions, and operators
                if (TryPrintToken(line, ref index, keywords, colors["keyword"]) ||
                    TryPrintToken(line, ref index, types, colors["type"]) ||
                    TryPrintToken(line, ref index, defaultFunctions, colors["defaultFunction"]) ||
                    TryPrintToken(line, ref index, operatorStrings, colors["operator"]))
                {
                    continue;
                }

                // Print remaining characters as default text
                PrintColored(line[index].ToString(), colors["default"]);
                index++;
            }

            Console.WriteLine();
        }

        private void PrintColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private bool TryPrintToken(string line, ref int index, string[] tokens, ConsoleColor color)
        {
            foreach (var token in tokens)
            {
                if (line.Substring(index).StartsWith(token))
                {
                    PrintColored(token, color);
                    index += token.Length;
                    return true;
                }
            }
            return false;
        }

        private string RemoveFromEnd(string str, string suffix)
        {
            return str.EndsWith(suffix) ? str.Substring(0, str.Length - suffix.Length) : str;
        }
    }
}
