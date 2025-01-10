namespace Hexa.Prototype
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Xml;

    public class ProtoEnumItem
    {
        public ProtoEnumItem(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            Value = node.Attributes["value"].Value;
        }

        public ProtoEnumItem(EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            Name = enumMemberDeclaration.Identifier.Text;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}