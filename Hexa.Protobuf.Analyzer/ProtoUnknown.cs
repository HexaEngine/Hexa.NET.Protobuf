namespace Hexa.Prototype
{
    public class ProtoUnknown : ProtoType
    {
        public ProtoUnknown(string name) : base(ProtoTypeKind.Unknown, name)
        {
        }

        public override void Resolve(List<ProtoType> types)
        {
        }
    }
}