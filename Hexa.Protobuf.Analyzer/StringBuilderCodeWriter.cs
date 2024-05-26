namespace Hexa.Protobuf.Analyser
{
    using Hexa.Prototype;
    using System.Text;

    public sealed class StringBuilderCodeWriter : IDisposable, ICodeWriter
    {
        private readonly string[] _indentStrings;
        private readonly string @namespace;
        private readonly IEnumerable<string> usings;

        private readonly StringBuilder _writer;

        private int lines;
        private int blocks = 0;
        private int indentLevel;

        private string _indentString = "";
        private bool _shouldIndent = true;

        public int IndentLevel { get => indentLevel; }

        public string NewLine { get => "\n"; }

        public string FileName { get; }

        public StringBuilderCodeWriter(string @namespace, IEnumerable<string> usings)
        {
            this.@namespace = @namespace;
            this.usings = usings;
            _indentStrings = new string[10];
            for (int i = 0; i < _indentStrings.Length; i++)
            {
                _indentStrings[i] = new string('\t', i);
            }

            _writer = new StringBuilder();

            BeginBlock($"namespace {@namespace}");

            foreach (string ns in usings)
            {
                WriteLine($"using {ns};");
            }

            if (usings.Any())
            {
                _writer.AppendLine();
            }
        }

        public StringBuilder StringBuilder => _writer;

        public long Length => _writer.Length;

        public int Lines => lines;

        public void Dispose()
        {
            EndBlock();
        }

        public void Write(char chr)
        {
            WriteIndented(chr);
        }

        public void Write(string @string)
        {
            WriteIndented(@string);
        }

        public void WriteLine()
        {
            _writer.AppendLine();
            _shouldIndent = true;
        }

        public void WriteLine(string @string)
        {
            WriteIndented(@string);
            _writer.AppendLine();
            _shouldIndent = true;
            lines++;
        }

        public void WriteLines(string? @string, bool newLineAtEnd = false)
        {
            if (@string == null)
                return;

            if (@string.Contains('\n'))
            {
                var lines = @string.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    WriteIndented(lines[i]);
                    _shouldIndent = true;
                    this.lines++;
                }
            }
            _shouldIndent = true;
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                WriteLine(line);
            }
        }

        public void BeginBlock(string content)
        {
            WriteLine(content);
            WriteLine("{");
            Indent(1);
            blocks++;
        }

        public void EndBlock()
        {
            if (blocks <= 0)
                return;
            blocks--;
            Dedent(1);
            WriteLine("}");
        }

        public IDisposable PushBlock(string marker = "{") => new CodeBlock(this, marker);

        public void Indent(int count = 1)
        {
            indentLevel += count;

            if (IndentLevel < _indentStrings.Length)
            {
                _indentString = _indentStrings[IndentLevel];
            }
            else
            {
                _indentString = new string('\t', IndentLevel);
            }
        }

        public void Dedent(int count = 1)
        {
            if (count > indentLevel)
                throw new ArgumentException("count out of range.", nameof(count));

            indentLevel -= count;
            if (indentLevel < _indentStrings.Length)
            {
                _indentString = _indentStrings[indentLevel];
            }
            else
            {
                _indentString = new string('\t', indentLevel);
            }
        }

        private void WriteIndented(char chr)
        {
            if (_shouldIndent)
            {
                _writer.Append(_indentString);
                _shouldIndent = false;
            }

            _writer.Append(chr);
        }

        private void WriteIndented(string @string)
        {
            if (_shouldIndent)
            {
                _writer.Append(_indentString);
                _shouldIndent = false;
            }

            _writer.Append(@string);
        }

        public readonly struct CodeBlock : IDisposable
        {
            private readonly ICodeWriter _writer;

            public CodeBlock(ICodeWriter writer, string content)
            {
                _writer = writer;
                _writer.BeginBlock(content);
            }

            public void Dispose()
            {
                _writer.EndBlock();
            }
        }
    }
}