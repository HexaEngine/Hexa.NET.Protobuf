namespace Hexa.Protobuf
{
    using Hexa.Protobuf.Analyser;
    using Hexa.Prototype;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System.Collections.Generic;
    using System.Text;

    [Generator]
    public class ProtoBufGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            ProtoOptions options = new() { Unmanaged = true };
            List<ProtoType> types = new();

            var syntaxTrees = context.Compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot();
                var structs = root.DescendantNodes().OfType<StructDeclarationSyntax>();
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();

                foreach (var enumDeclaration in enums)
                {
                    var enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration) as INamedTypeSymbol;

                    var underlyingType = enumSymbol.EnumUnderlyingType;
                    var underlyingTypeName = underlyingType.ToDisplayString();

                    ProtoEnum protoEnum = new(enumDeclaration, underlyingTypeName);

                    types.Add(protoEnum);
                }

                foreach (var structDeclaration in structs)
                {
                    var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration) as INamedTypeSymbol;
                    if (structSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "Hexa.Protobuf.ProtobufRecordAttribute"))
                    {
                        ProtoStruct protoStruct = new(structDeclaration)
                        {
                            Namespace = GetNamespace(structDeclaration)
                        };
                        types.Add(protoStruct);
                    }
                }
            }

            for (int i = 0; i < types.Count; i++)
            {
                types[i].Resolve(types);
            }

            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];

                if (type is ProtoStruct protoStruct)
                {
                    StringBuilderCodeWriter writer = new(protoStruct.Namespace, ["System", "System.Buffers.Binary", "System.Text"]);

                    protoStruct.Write(writer, options);

                    writer.EndBlock();

                    context.AddSource($"{protoStruct.Name}_Protobuf", SourceText.From(writer.StringBuilder.ToString(), Encoding.UTF8));
                }
            }
        }

        private string GetNamespace(SyntaxNode syntaxNode)
        {
            var namespaceDeclaration = syntaxNode.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceDeclaration != null ? namespaceDeclaration.Name.ToString() : "GlobalNamespace";
        }
    }
}