namespace Hexa.Prototype
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Xml;

    public class ProtoEnum : ProtoType
    {
        private readonly List<ProtoEnumItem> items = [];

        public ProtoEnum(XmlNode node) : base(ProtoTypeKind.Enum, node.Attributes["name"]?.Value)
        {
            BaseType = new(node.Attributes["base"]?.Value ?? "int");
            foreach (XmlNode enumNode in node.SelectNodes("item"))
            {
                items.Add(new ProtoEnumItem(enumNode));
            }
        }

        public ProtoEnum(EnumDeclarationSyntax enumDeclaration, string underlyingType) : base(ProtoTypeKind.Enum, enumDeclaration.Identifier.Text)
        {
            BaseType = new(underlyingType);

            foreach (EnumMemberDeclarationSyntax enumNode in enumDeclaration.Members)
            {
                items.Add(new ProtoEnumItem(enumNode));
            }
        }

        public ProtoPrimitive BaseType { get; }

        public List<ProtoEnumItem> Items => items;

        public override void Resolve(IEnumerable<ProtoType> types)
        {
            // nothing to do here.
        }
    }
}