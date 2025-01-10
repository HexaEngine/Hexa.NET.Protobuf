namespace Hexa.Protobuf
{
    using Hexa.Protobuf.Analyser;
    using Hexa.Prototype;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Generator(LanguageNames.CSharp)]
    public class ProtoBufGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            var syntaxTrees = initContext.SyntaxProvider
          .CreateSyntaxProvider(
              predicate: static (node, _) => node is StructDeclarationSyntax || node is EnumDeclarationSyntax,
              transform: static (context, _) => context.Node.SyntaxTree)
          .Where(static tree => tree != null);

            var enumsAndStructs = syntaxTrees.SelectMany((tree, _) =>
            {
                var root = tree.GetRoot();
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                var structs = root.DescendantNodes().OfType<StructDeclarationSyntax>();

                // Convert both to SyntaxNode, which is a common type, before concatenating
                return enums.Cast<SyntaxNode>().Concat(structs.Cast<SyntaxNode>());
            }).Where(static node => node is EnumDeclarationSyntax || node is StructDeclarationSyntax);

            var combinedProvider = syntaxTrees.Combine(initContext.CompilationProvider);

            var protoTypes = combinedProvider
                .SelectMany(GetProtoTypes)
                .Where(protoType => protoType != null);

            var resolvedProtoTypes = protoTypes
                .Collect()
                .Select((protoTypesList, token) =>
                {
                    foreach (var protoType in protoTypesList)
                    {
                        if (token.IsCancellationRequested) return protoTypesList;
                        protoType.Resolve(protoTypesList);
                    }
                    return protoTypesList;
                });

            initContext.RegisterSourceOutput(resolvedProtoTypes, (spc, protoTypesList) =>
            {
                foreach (var protoType in protoTypesList)
                {
                    if (protoType is ProtoStruct protoStruct)
                    {
                        var writer = new StringBuilderCodeWriter(protoStruct.Namespace, ["System", "System.Buffers.Binary", "System.Text"]);
                        protoStruct.Write(writer, new ProtoOptions { Unmanaged = true });
                        writer.EndBlock();
                        spc.AddSource($"{protoStruct.Name}_Protobuf", SourceText.From(writer.StringBuilder.ToString(), Encoding.UTF8));
                    }
                }
            });
        }

        private IEnumerable<ProtoType> GetProtoTypes((SyntaxTree tree, Compilation compilation) combined, CancellationToken token)
        {
            var (tree, compilation) = combined;
            var root = tree.GetRoot();
            var semanticModel = compilation.GetSemanticModel(tree);
            // Process the enums and structs from the syntax tree
            foreach (var node in root.DescendantNodes())
            {
                if (token.IsCancellationRequested) yield break;

                if (node is EnumDeclarationSyntax enumDeclaration)
                {
                    // Access semantic model for the enum
                    var enumSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(enumDeclaration)!;
                    var underlyingType = enumSymbol.EnumUnderlyingType!;
                    var underlyingTypeName = underlyingType.ToDisplayString();
                    yield return new ProtoEnum(enumDeclaration, underlyingTypeName);
                }
                else if (node is StructDeclarationSyntax structDeclaration)
                {
                    // Process struct with the ProtoRecord attribute
                    if (HasProtoRecordAttribute(structDeclaration, semanticModel))
                    {
                        yield return new ProtoStruct(structDeclaration)
                        {
                            Namespace = GetNamespace(structDeclaration)
                        };
                    }
                }
            }
        }

        private bool HasProtoRecordAttribute(StructDeclarationSyntax structDeclaration, SemanticModel semanticModel)
        {
            var structSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(structDeclaration)!;
            return structSymbol.GetAttributes().Any(ad => ad.AttributeClass!.ToDisplayString() == "Hexa.NET.Protobuf.ProtobufRecordAttribute");
        }

        /*
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
        */

        private string GetNamespace(SyntaxNode syntaxNode)
        {
            var namespaceDeclaration = syntaxNode.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceDeclaration != null ? namespaceDeclaration.Name.ToString() : "GlobalNamespace";
        }
    }
}