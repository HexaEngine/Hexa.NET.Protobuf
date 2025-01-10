namespace Hexa.Prototype
{
    using System.Xml;

    public class ProtoOptions
    {
        public ProtoOptions(XmlNode root)
        {
            if (root.Attributes != null)
            {
                var unmanagedOption = root.Attributes["unmanaged"];
                if (unmanagedOption != null)
                {
                    Unmanaged = bool.Parse(unmanagedOption.Value);
                }
            }
        }

        public ProtoOptions()
        {
        }

        public bool Unmanaged { get; set; }

        public bool Partial { get; set; }
    }
}