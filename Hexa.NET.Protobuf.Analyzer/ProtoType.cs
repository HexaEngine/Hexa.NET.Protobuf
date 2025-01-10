namespace Hexa.Prototype
{
    public abstract class ProtoType
    {
        private readonly ProtoTypeKind kind;
        private readonly string name;

        public ProtoType(ProtoTypeKind kind, string name)
        {
            this.kind = kind;
            this.name = name;
        }

        public ProtoTypeKind Kind => kind;

        public string Name => name;

        public abstract void Resolve(IEnumerable<ProtoType> types);
    }
}