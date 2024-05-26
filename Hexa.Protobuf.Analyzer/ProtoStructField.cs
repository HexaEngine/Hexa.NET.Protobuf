namespace Hexa.Prototype
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Xml;

    public class ProtoStructField
    {
        private readonly string name;

        public ProtoStructField(XmlNode node)
        {
            name = node.Attributes["name"].Value;
            Type = Helper.ResolveEarly(node.Attributes["type"].Value);
        }

        public ProtoStructField(FieldDeclarationSyntax member)
        {
            var variable = member.Declaration.Variables.First();
            name = variable.Identifier.Text;
            Type = Helper.ResolveEarly(member.Declaration.Type.ToString());
        }

        public string Name => name;

        public ProtoType Type { get; set; }
    }
}