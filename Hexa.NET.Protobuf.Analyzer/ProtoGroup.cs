namespace Hexa.Prototype
{
    using System.Xml;

    public class ProtoGroup
    {
        public ProtoGroup(XmlNode node)
        {
            EnumName = node.Attributes["enumName"]?.Value;
            Types = new();
        }

        public string? EnumName { get; set; }

        public List<ProtoType> Types { get; set; }
    }
}