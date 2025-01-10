namespace Hexa.Prototype
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Xml;

    public class ProtoStruct : ProtoType
    {
        private readonly List<ProtoStructField> fields = [];

        public ProtoStruct(XmlNode node) : base(ProtoTypeKind.Struct, node.Attributes["name"]?.Value)
        {
            foreach (XmlNode fieldNode in node.SelectNodes("field"))
            {
                var field = new ProtoStructField(fieldNode);
                fields.Add(field);
                if (field.Type.Kind == ProtoTypeKind.Array || field.Type.Kind == ProtoTypeKind.String)
                {
                    HasUnmanaged = true;
                }
            }
        }

        public ProtoStruct(StructDeclarationSyntax structDeclaration) : base(ProtoTypeKind.Struct, structDeclaration.Identifier.Text)
        {
            foreach (FieldDeclarationSyntax member in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
            {
                var field = new ProtoStructField(member);
                fields.Add(field);
                if (field.Type.Kind == ProtoTypeKind.Array || field.Type.Kind == ProtoTypeKind.String)
                {
                    HasUnmanaged = true;
                }
            }
        }

        public List<ProtoStructField> Fields => fields;

        public bool HasUnmanaged { get; }

        public string Namespace { get; set; }

        public override void Resolve(IEnumerable<ProtoType> types)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.Type.Kind != ProtoTypeKind.Unknown)
                {
                    if (field.Type.Kind == ProtoTypeKind.Array)
                    {
                        field.Type.Resolve(types);
                    }
                    continue;
                }

                field.Type = Helper.ResolveType(field.Type.Name, types);
            }
        }

        public void Write(ICodeWriter writer, ProtoOptions options)
        {
            writer.BeginBlock($"public partial struct {Name}");

            writer.WriteLine();
            WriteWriteMethod(writer, options);
            writer.WriteLine();
            WriteReadMethod(writer, options);
            writer.WriteLine();
            WriteSizeOfMethod(writer, options);

            if (options.Unmanaged)
            {
                writer.WriteLine();
                WriteFreeMethod(writer);
            }

            writer.EndBlock();
        }

        private void WriteSizeOfMethod(ICodeWriter writer, ProtoOptions options)
        {
            writer.BeginBlock("public readonly int SizeOf()");
            bool dynamic = false;
            int size = 0;
            foreach (ProtoStructField field in fields)
            {
                string fieldName = field.Name;

                switch (field.Type.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.IncrementOffset(ref size, ref dynamic, ((ProtoPrimitive)field.Type).SizeInBytes);
                        break;

                    case ProtoTypeKind.Struct:
                        writer.IncrementOffset(ref size, ref dynamic, 0, true);
                        writer.WriteLine($"idx += {fieldName}.SizeOf();");
                        break;

                    case ProtoTypeKind.Enum:
                        writer.IncrementOffset(ref size, ref dynamic, ((ProtoEnum)field.Type).BaseType.SizeInBytes);
                        break;

                    case ProtoTypeKind.String:
                        ((ProtoString)field.Type).SizeOfString(writer, options, ref size, ref dynamic, fieldName);
                        break;

                    case ProtoTypeKind.Array:
                        ((ProtoArray)field.Type).WriteSizeOfMethod(writer, options, ref size, ref dynamic, fieldName);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            writer.WriteLine($"return {(dynamic ? "idx" : size)};");

            writer.EndBlock();
        }

        private void WriteWriteMethod(ICodeWriter writer, ProtoOptions options)
        {
            writer.BeginBlock("public readonly int Write(Span<byte> span)");

            bool dynamic = false;
            int offset = 0;
            foreach (ProtoStructField field in fields)
            {
                string fieldName = field.Name;

                switch (field.Type.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoPrimitive)field.Type).WriteWriteMethod(writer, offset, dynamic, fieldName));
                        break;

                    case ProtoTypeKind.Struct:
                        writer.IncrementOffset(ref offset, ref dynamic, 0, true);
                        writer.WriteLine($"idx += {fieldName}.Write(span[{(dynamic ? "idx" : offset)}..]);");
                        break;

                    case ProtoTypeKind.Enum:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoEnum)field.Type).BaseType.WriteWriteMethod(writer, offset, dynamic, $"({((ProtoEnum)field.Type).BaseType.Name}){fieldName}"));
                        break;

                    case ProtoTypeKind.String:
                        ((ProtoString)field.Type).WriteString(writer, options, ref offset, ref dynamic, fieldName);
                        break;

                    case ProtoTypeKind.Array:
                        ((ProtoArray)field.Type).WriteWriteMethod(writer, options, ref offset, ref dynamic, fieldName);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            writer.WriteLine($"return {(dynamic ? "idx" : offset)};");

            writer.EndBlock();
        }

        private void WriteReadMethod(ICodeWriter writer, ProtoOptions options)
        {
            writer.BeginBlock("public int Read(ReadOnlySpan<byte> span)");

            bool dynamic = false;
            int offset = 0;
            foreach (ProtoStructField field in fields)
            {
                string fieldName = field.Name;
                switch (field.Type.Kind)
                {
                    case ProtoTypeKind.Primitive:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoPrimitive)field.Type).WriteReadMethod(writer, offset, dynamic, fieldName));
                        break;

                    case ProtoTypeKind.Struct:
                        writer.IncrementOffset(ref offset, ref dynamic, 0, true);
                        writer.WriteLine($"idx += {fieldName}.Read(span[{(dynamic ? "idx" : offset)}..]);");
                        break;

                    case ProtoTypeKind.Enum:
                        writer.IncrementOffset(ref offset, ref dynamic, ((ProtoEnum)field.Type).BaseType.WriteReadMethod(writer, offset, dynamic, fieldName, field.Type.Name));
                        break;

                    case ProtoTypeKind.String:
                        ((ProtoString)field.Type).ReadString(writer, options, ref offset, ref dynamic, fieldName);
                        break;

                    case ProtoTypeKind.Array:
                        ((ProtoArray)field.Type).WriteReadMethod(writer, options, ref offset, ref dynamic, fieldName);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            writer.WriteLine($"return {(dynamic ? "idx" : offset)};");

            writer.EndBlock();
        }

        public void WriteFreeMethod(ICodeWriter writer)
        {
            writer.BeginBlock("public void Free()");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                switch (field.Type.Kind)
                {
                    case ProtoTypeKind.Struct:
                        var structType = (ProtoStruct)field.Type;
                        if (structType.HasUnmanaged)
                        {
                            writer.WriteLine($"{field.Name}.Free();");
                        }
                        break;

                    case ProtoTypeKind.Array:
                    case ProtoTypeKind.String:
                        writer.WriteLine($"{field.Name}.Free();");
                        break;
                }
            }
            writer.EndBlock();
        }
    }
}