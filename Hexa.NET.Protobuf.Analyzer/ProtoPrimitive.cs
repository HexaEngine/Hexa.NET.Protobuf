namespace Hexa.Prototype
{
    public class ProtoPrimitive : ProtoType
    {
        public ProtoPrimitive(string name) : base(ProtoTypeKind.Primitive, name)
        {
            PrimitiveKind = name switch
            {
                "sbyte" => ProtoPrimitiveKind.SByte,
                "byte" => ProtoPrimitiveKind.Byte,
                "short" => ProtoPrimitiveKind.Short,
                "ushort" => ProtoPrimitiveKind.UShort,
                "int" => ProtoPrimitiveKind.Int,
                "uint" => ProtoPrimitiveKind.UInt,
                "long" => ProtoPrimitiveKind.Long,
                "ulong" => ProtoPrimitiveKind.ULong,
                "float" => ProtoPrimitiveKind.Float,
                "double" => ProtoPrimitiveKind.Double,
                "bool" => ProtoPrimitiveKind.Bool,
                "Guid" => ProtoPrimitiveKind.Guid,
                _ => throw new NotSupportedException(),
            };

            SizeInBytes = name switch
            {
                "sbyte" => 1,
                "byte" => 1,
                "short" => 2,
                "ushort" => 2,
                "int" => 4,
                "uint" => 4,
                "long" => 8,
                "ulong" => 8,
                "float" => 4,
                "double" => 8,
                "bool" => 1,
                "Guid" => 16,
                _ => throw new NotSupportedException(),
            };
        }

        public ProtoPrimitiveKind PrimitiveKind { get; }

        public int SizeInBytes { get; }

        public override void Resolve(IEnumerable<ProtoType> types)
        {
        }

        public void WriteWriteMethod(ICodeWriter writer, ref int offset, ref bool dynamic, string fieldName)
        {
            writer.IncrementOffset(ref offset, ref dynamic, WriteWriteMethod(writer, offset, dynamic, fieldName));
        }

        public int WriteWriteMethod(ICodeWriter writer, int offset, bool dynamic, string fieldName)
        {
            switch (PrimitiveKind)
            {
                case ProtoPrimitiveKind.Guid:
                    writer.WriteLine($"{fieldName}.TryWriteBytes(span.Slice({(dynamic ? "idx" : offset)}, 16));");
                    break;

                case ProtoPrimitiveKind.Byte:
                    writer.WriteLine($"span[{(dynamic ? "idx" : offset)}] = {fieldName};");
                    break;

                case ProtoPrimitiveKind.Short:
                    writer.WriteLine($"BinaryPrimitives.WriteInt16LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.UShort:
                    writer.WriteLine($"BinaryPrimitives.WriteUInt16LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.Int:
                    writer.WriteLine($"BinaryPrimitives.WriteInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.UInt:
                    writer.WriteLine($"BinaryPrimitives.WriteUInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.Long:
                    writer.WriteLine($"BinaryPrimitives.WriteInt64LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.ULong:
                    writer.WriteLine($"BinaryPrimitives.WriteUInt64LittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.Float:
                    writer.WriteLine($"BinaryPrimitives.WriteSingleLittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.Double:
                    writer.WriteLine($"BinaryPrimitives.WriteDoubleLittleEndian(span[{(dynamic ? "idx" : offset)}..], {fieldName});");
                    break;

                case ProtoPrimitiveKind.Bool:
                    writer.WriteLine($"span[{(dynamic ? "idx" : offset)}] = {fieldName} ? (byte)1 : (byte)0;");
                    break;
            }

            return SizeInBytes;
        }

        public void WriteReadMethod(ICodeWriter writer, ref int offset, ref bool dynamic, string fieldName)
        {
            writer.IncrementOffset(ref offset, ref dynamic, WriteReadMethod(writer, offset, dynamic, fieldName, null));
        }

        public void WriteReadMethod(ICodeWriter writer, ref int offset, ref bool dynamic, string fieldName, string? conversion)
        {
            writer.IncrementOffset(ref offset, ref dynamic, WriteReadMethod(writer, offset, dynamic, fieldName, conversion));
        }

        public int WriteReadMethod(ICodeWriter writer, int offset, bool dynamic, string fieldName)
        {
            return WriteReadMethod(writer, offset, dynamic, fieldName, null);
        }

        public int WriteReadMethod(ICodeWriter writer, int offset, bool dynamic, string fieldName, string? conversion)
        {
            switch (PrimitiveKind)
            {
                case ProtoPrimitiveKind.Byte:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}span[{(dynamic ? "idx" : offset)}];");
                    break;

                case ProtoPrimitiveKind.Guid:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}new Guid(span.Slice({(dynamic ? "idx" : offset)}, 16));");
                    break;

                case ProtoPrimitiveKind.UShort:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadUInt16LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Short:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadInt16LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.UInt:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadUInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Int:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadInt32LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.ULong:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadUInt64LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Long:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadInt64LittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Float:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadSingleLittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Double:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}BinaryPrimitives.ReadDoubleLittleEndian(span[{(dynamic ? "idx" : offset)}..]);");
                    break;

                case ProtoPrimitiveKind.Bool:
                    writer.WriteLine($"{fieldName} = {(conversion != null ? $"({conversion})" : string.Empty)}(span[{(dynamic ? "idx" : offset)}] != 0);");
                    break;
            }

            return SizeInBytes;
        }
    }
}