using System.Text;

namespace Benita;

/// <summary>ورودی تک‌خطی REPL را با ویرایش پایه، تاریخچه و رنگ‌آمیزی هم‌زمان مدیریت می‌کند.</summary>
internal sealed class ConsoleLineEditor
{
    private static readonly HashSet<string> Keywords =
        ["include_once", "_main_", "pkg", "func", "if", "else", "while", "for", "in", "return", "new", "true", "false", "break", "continue", "match"];
    private static readonly HashSet<string> Types = ["number", "string", "bool", "void", "let"];
    private static readonly HashSet<string> BuiltIns =
        ["print", "input", "to_string", "to_number", "round_number", "sqrt_number",
         "array_len", "array_add", "array_remove", "array_contains", "array_index_of",
         "array_reverse", "array_clear", "array_insert", "array_slice", "array_concat", "array_sort",
         "string_len", "string_char_at", "string_substring", "string_contains", "string_index_of",
         "string_replace", "string_split", "string_trim", "string_to_lower", "string_to_upper",
         "file_read", "file_write", "file_exist", "file_delete"];

    private readonly List<string> _history;

    /// <summary>ویرایشگر را به تاریخچهٔ نشست متصل می‌کند.</summary>
    public ConsoleLineEditor(List<string> history) => _history = history;

    /// <summary>یک خط را با پشتیبانی از cursor، حذف، history و Ctrl+C دریافت می‌کند.</summary>
    public string? ReadLine(string prompt, bool continuation)
    {
        Write(prompt, ConsoleColor.DarkGray);
        int startLeft = Console.CursorLeft;
        int startTop = Console.CursorTop;
        var buffer = new StringBuilder();
        int cursor = 0, renderedLength = 0, historyIndex = _history.Count;

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                Console.WriteLine();
                return buffer.ToString();
            }
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.C)
            {
                Console.SetCursorPosition(startLeft + buffer.Length, startTop);
                Write("^C", ConsoleColor.DarkGray);
                Console.WriteLine();
                return continuation ? ":cancel" : ":exit";
            }

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow when cursor > 0: cursor--; break;
                case ConsoleKey.RightArrow when cursor < buffer.Length: cursor++; break;
                case ConsoleKey.Home: cursor = 0; break;
                case ConsoleKey.End: cursor = buffer.Length; break;
                case ConsoleKey.Backspace when cursor > 0: buffer.Remove(--cursor, 1); break;
                case ConsoleKey.Delete when cursor < buffer.Length: buffer.Remove(cursor, 1); break;
                case ConsoleKey.UpArrow when !continuation && _history.Count > 0:
                    historyIndex = Math.Max(0, historyIndex - 1);
                    ReplaceBuffer(buffer, HistoryLine(historyIndex));
                    cursor = buffer.Length;
                    break;
                case ConsoleKey.DownArrow when !continuation && historyIndex < _history.Count:
                    historyIndex++;
                    ReplaceBuffer(buffer, historyIndex == _history.Count ? string.Empty : HistoryLine(historyIndex));
                    cursor = buffer.Length;
                    break;
                default:
                    if (!char.IsControl(key.KeyChar) && startLeft + buffer.Length < Console.BufferWidth - 1)
                    {
                        buffer.Insert(cursor, key.KeyChar);
                        cursor++;
                    }
                    break;
            }

            Redraw(buffer.ToString(), cursor, startLeft, startTop, ref renderedLength);
        }
    }

    private string HistoryLine(int index) => _history[index].ReplaceLineEndings(" ");

    private static void ReplaceBuffer(StringBuilder buffer, string value)
    {
        buffer.Clear();
        buffer.Append(value);
    }

    private static void Redraw(string text, int cursor, int left, int top, ref int renderedLength)
    {
        Console.SetCursorPosition(left, top);
        WriteHighlighted(text);
        Console.ResetColor();
        if (renderedLength > text.Length) Console.Write(new string(' ', renderedLength - text.Length));
        renderedLength = text.Length;
        Console.SetCursorPosition(left + cursor, top);
    }

    private static void WriteHighlighted(string text)
    {
        for (int index = 0; index < text.Length;)
        {
            if (index + 1 < text.Length && text[index] == '/' && text[index + 1] == '/')
            {
                Write(text[index..], ConsoleColor.DarkGreen);
                return;
            }
            if (text[index] == '"')
            {
                int end = index + 1;
                bool escaped = false;
                while (end < text.Length)
                {
                    char current = text[end++];
                    if (!escaped && current == '"') break;
                    escaped = !escaped && current == '\\';
                    if (current != '\\') escaped = false;
                }
                Write(text[index..end], ConsoleColor.DarkYellow);
                index = end;
                continue;
            }
            if (char.IsLetter(text[index]) || text[index] == '_')
            {
                int end = index + 1;
                while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_')) end++;
                string word = text[index..end];
                ConsoleColor color = Keywords.Contains(word) ? ConsoleColor.Blue
                    : Types.Contains(word) ? ConsoleColor.Cyan
                    : BuiltIns.Contains(word) ? ConsoleColor.Yellow
                    : ConsoleColor.Gray;
                Write(word, color);
                index = end;
                continue;
            }
            if (char.IsDigit(text[index]))
            {
                int end = index + 1;
                while (end < text.Length && (char.IsDigit(text[end]) || text[end] == '.')) end++;
                Write(text[index..end], ConsoleColor.Magenta);
                index = end;
                continue;
            }
            Write(text[index].ToString(), "{}[]();,+-*/%=!<>".Contains(text[index]) ? ConsoleColor.DarkCyan : ConsoleColor.Gray);
            index++;
        }
    }

    private static void Write(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
}
