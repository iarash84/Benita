using System.Text;

namespace BenitaTestProject
{
    internal static class SharedFunction
    {
        public static void AssertTextEqualIgnoringLineEndings(string expected, string actual)
        {
            Assert.AreEqual(
                expected.ReplaceLineEndings("\n"),
                actual.ReplaceLineEndings("\n"));
        }

        public static bool AppendToSubstring(string value, ref StringBuilder stringBuilder)
        {
            string sbContent = stringBuilder.ToString();
            if (!sbContent.Contains(value))
            {
                stringBuilder.AppendLine(value);
                return true;
            }
            return false;
        }
    }

    public class ConsoleOutput : IDisposable
    {
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;

        public ConsoleOutput()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public string GetOuput()
        {
            // Use one canonical line ending so output assertions behave identically
            // on Windows, Linux, and macOS runners.
            return _stringWriter.ToString().ReplaceLineEndings("\r\n");
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }
    }

    public class ConsoleInput : IDisposable
    {
        private readonly StringReader _stringReader;
        private readonly TextReader _originalInput;

        public ConsoleInput(string input)
        {
            _stringReader = new StringReader(input);
            _originalInput = Console.In;
            Console.SetIn(_stringReader);
        }

        public void Dispose()
        {
            Console.SetIn(_originalInput);
            _stringReader.Dispose();
        }
    }
}
