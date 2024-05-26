namespace Hexa.Prototype
{
    using System.Text;

    public class ProtoString : ProtoType
    {
        public ProtoString(string name) : base(ProtoTypeKind.String, name)
        {
        }

        public override void Resolve(List<ProtoType> types)
        {
        }

        public void WriteString(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName)
        {
            if (options.Unmanaged)
            {
                writer.WriteLine($"int {fieldName}Len = Encoding.UTF8.GetByteCount({fieldName}.AsSpan());");
                writer.WriteLine($"BinaryPrimitives.WriteInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName}Len);");
                writer.IncrementOffset(ref offset, ref dynamic, 4, true);
                writer.WriteLine($"Encoding.UTF8.GetBytes({fieldName}.AsSpan(), span[{(dynamic ? "idx" : offset)}..]);");
                writer.WriteLine($"idx += {fieldName}Len;");
                return;
            }
            writer.WriteLine($"int {fieldName}Len = Encoding.UTF8.GetByteCount({fieldName});");
            writer.WriteLine($"BinaryPrimitives.WriteInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName}Len);");
            writer.IncrementOffset(ref offset, ref dynamic, 4, true);
            writer.WriteLine($"Encoding.UTF8.GetBytes({fieldName}, span[{(dynamic ? "idx" : offset)}..]);");
            writer.WriteLine($"idx += {fieldName}Len;");
        }

        public void ReadString(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName)
        {
            if (options.Unmanaged)
            {
                writer.WriteLine($"int {fieldName}Len = BinaryPrimitives.ReadInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                writer.IncrementOffset(ref offset, ref dynamic, 4, true);

                writer.WriteLine($"{fieldName} = new UnmanagedWString({fieldName}Len / 2);");
                writer.WriteLine($"Encoding.UTF8.TryGetChars(span.Slice({(dynamic ? "idx" : offset)}, {fieldName}Len), {fieldName}.AsSpan(), out _);");
                writer.WriteLine($"idx += {fieldName}Len;");
                return;
            }
            writer.WriteLine($"int {fieldName}Len = BinaryPrimitives.ReadInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
            writer.IncrementOffset(ref offset, ref dynamic, 4, true);
            writer.WriteLine($"{fieldName} = Encoding.UTF8.GetString(span.Slice({(dynamic ? "idx" : offset)}, {fieldName}Len));");
            writer.WriteLine($"idx += {fieldName}Len;");
        }

        public void SizeOfString(ICodeWriter writer, ProtoOptions options, ref int size, ref bool dynamic, string fieldName)
        {
            writer.IncrementOffset(ref size, ref dynamic, 4, true);

            if (options.Unmanaged)
            {
                writer.WriteLine($"idx += Encoding.UTF8.GetByteCount({fieldName}.AsSpan());");
                return;
            }
            writer.WriteLine($"idx += Encoding.UTF8.GetByteCount({fieldName});");
        }
    }
}