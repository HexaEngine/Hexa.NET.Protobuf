namespace Hexa.Protobuf.Analyser
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProtobufRecordPartialKeywordAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = "Missing 'partial' keyword in Protobuf record";
        private static readonly LocalizableString MessageFormat = "Protobuf records should be declared with the 'partial' keyword";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            "PROTOBUF_RECORD_PARTIAL",
            Title,
            MessageFormat,
            "Protobuf",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A Protobuf record should be declared with the 'partial' keyword.");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeProtobufRecordDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeProtobufRecordDeclaration(SyntaxNodeAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var structDeclaration = context.Node as StructDeclarationSyntax;
            var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration);
            if (structDeclaration == null || !structSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "Hexa.Protobuf.ProtobufRecordAttribute"))
            {
                return;
            }

            if (!structDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, structDeclaration.Identifier.GetLocation()));
            }
        }
    }
}