namespace Hexa.Prototype
{
    using System.Collections.Generic;

    public class ProtoArray : ProtoType
    {
        public ProtoArray(string name) : base(ProtoTypeKind.Array, name.Split(':')[0])
        {
            int idx = name.IndexOf('[');
            string typeName = name.Substring(0, idx);
            ArrayItemType = Helper.ResolveEarly(typeName);
            int idx2 = name.IndexOf(':');
            if (idx2 != -1)
            {
                string size = name.Substring(idx2 + 1);

                if (size.StartsWith("!"))
                {
                    FixedSize = true;
                    size = size.Substring(1);
                }

                if (int.TryParse(size, out var value))
                {
                    Size = value;
                }
                else
                {
                    ArraySizeType = new ProtoPrimitive(size);
                }
            }
        }

        public ProtoType ArrayItemType { get; set; }

        public ProtoPrimitive ArraySizeType { get; set; }

        public bool FixedSize { get; }

        public int Size { get; } = -1;

        public override void Resolve(IEnumerable<ProtoType> types)
        {
            ArrayItemType = Helper.ResolveType(ArrayItemType.Name, types);
        }

        public void WriteWriteMethod(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName)
        {
            WriteWriteMethod(writer, options, ref offset, ref dynamic, fieldName, 0);
        }

        public void WriteWriteMethod(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName, int depth)
        {
            if (!FixedSize)
            {
                ArraySizeType.WriteWriteMethod(writer, ref offset, ref dynamic, $"{fieldName}.Length");
                writer.IncrementOffset(ref offset, ref dynamic, 0, true);

                writer.BeginBlock($"for (int i{depth} = 0; i{depth} < {fieldName}.Length; i{depth}++)");

                switch (ArrayItemType.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoPrimitive)ArrayItemType).WriteWriteMethod(writer, offset, dynamic, $"{fieldName}[i{depth}]"));
                        break;

                    case ProtoTypeKind.Struct:
                        writer.WriteLine($"idx += {fieldName}[i{depth}].Write(span[{(dynamic ? "idx" : offset)}..]);");
                        break;

                    case ProtoTypeKind.Enum:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoEnum)ArrayItemType).BaseType.WriteWriteMethod(writer, offset, dynamic, $"({((ProtoEnum)ArrayItemType).BaseType.Name}){fieldName}[i{depth}]"));
                        break;

                    case ProtoTypeKind.String:
                        ((ProtoString)ArrayItemType).WriteString(writer, options, ref offset, ref dynamic, $"{fieldName}[i{depth}]");
                        break;

                    case ProtoTypeKind.Array:
                        break;

                    default:
                        throw new NotSupportedException();
                }

                writer.EndBlock();
            }
        }

        public void WriteReadMethod(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName)
        {
            WriteReadMethod(writer, options, ref offset, ref dynamic, fieldName, 0);
        }

        public void WriteReadMethod(ICodeWriter writer, ProtoOptions options, ref int offset, ref bool dynamic, string fieldName, int depth)
        {
            if (!FixedSize)
            {
                ArraySizeType.WriteReadMethod(writer, ref offset, ref dynamic, $"{ArraySizeType.Name} {fieldName}Len");
                if (options.Unmanaged)
                {
                    writer.WriteLine($"{fieldName} = new UnmanagedArray<{ArrayItemType.Name}>({fieldName}Len);");
                }
                else
                {
                    writer.WriteLine($"{fieldName} = new {ArrayItemType.Name}[{fieldName}Len];");
                }

                writer.IncrementOffset(ref offset, ref dynamic, 0, true);

                writer.BeginBlock($"for (int i{depth} = 0; i{depth} < {fieldName}.Length; i{depth}++)");

                switch (ArrayItemType.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoPrimitive)ArrayItemType).WriteReadMethod(writer, offset, dynamic, $"{fieldName}[i{depth}]"));
                        break;

                    case ProtoTypeKind.Struct:
                        writer.WriteLine($"idx += {fieldName}[i{depth}].Read(span[{(dynamic ? "idx" : offset)}..]);");
                        break;

                    case ProtoTypeKind.Enum:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoEnum)ArrayItemType).BaseType.WriteReadMethod(writer, offset, dynamic, $"{fieldName}[i{depth}]", ((ProtoEnum)ArrayItemType).Name));
                        break;

                    case ProtoTypeKind.String:
                        ((ProtoString)ArrayItemType).ReadString(writer, options, ref offset, ref dynamic, $"{fieldName}[i{depth}]");
                        break;

                    case ProtoTypeKind.Array:
                        break;

                    default:
                        throw new NotSupportedException();
                }

                writer.EndBlock();
            }
        }

        public void WriteSizeOfMethod(ICodeWriter writer, ProtoOptions options, ref int size, ref bool dynamic, string fieldName)
        {
            WriteSizeOfMethod(writer, options, ref size, ref dynamic, fieldName, 0);
        }

        public void WriteSizeOfMethod(ICodeWriter writer, ProtoOptions options, ref int size, ref bool dynamic, string fieldName, int depth)
        {
            if (!FixedSize)
            {
                writer.IncrementOffset(ref size, ref dynamic, ArraySizeType.SizeInBytes, true);

                switch (ArrayItemType.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.WriteLine($"idx += {((ProtoPrimitive)ArrayItemType).SizeInBytes} * {fieldName}.Length;");
                        break;

                    case ProtoTypeKind.Struct:
                        writer.BeginBlock($"for (int i{depth} = 0; i{depth} < {fieldName}.Length; i{depth}++)");
                        writer.WriteLine($"idx += {fieldName}[i{depth}].SizeOf();");
                        writer.EndBlock();
                        break;

                    case ProtoTypeKind.Enum:
                        writer.WriteLine($"idx += {((ProtoEnum)ArrayItemType).BaseType.SizeInBytes} * {fieldName}.Length;");
                        break;

                    case ProtoTypeKind.String:
                        writer.BeginBlock($"for (int i{depth} = 0; i{depth} < {fieldName}.Length; i{depth}++)");
                        writer.IncrementOffset(ref size, ref dynamic, 4, true);
                        writer.WriteLine($"idx += Encoding.UTF8.GetByteCount({fieldName});");
                        writer.EndBlock();
                        break;

                    case ProtoTypeKind.Array:
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}