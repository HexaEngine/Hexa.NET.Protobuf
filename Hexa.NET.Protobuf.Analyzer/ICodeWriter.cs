namespace Hexa.Prototype
{
    using System;
    using System.Collections.Generic;

    public interface ICodeWriter
    {
        int IndentLevel { get; }
        long Length { get; }
        int Lines { get; }
        string NewLine { get; }

        void BeginBlock(string content);
        void Dedent(int count = 1);
        void Dispose();
        void EndBlock();
        void Indent(int count = 1);
        IDisposable PushBlock(string marker = "{");
        void Write(char chr);
        void Write(string @string);
        void WriteLine();
        void WriteLine(string @string);
        void WriteLines(IEnumerable<string> lines);
        void WriteLines(string? @string, bool newLineAtEnd = false);
    }
}