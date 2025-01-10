namespace Hexa.Prototype
{
    public static class Helper
    {
        public static ProtoType ResolveType(string name, IEnumerable<ProtoType> types)
        {
            foreach (ProtoType type in types)
            {
                if (type.Name == name)
                {
                    return type;
                }
            }
            throw new Exception($"Couldn't resolve type '{name}'");
        }

        public static ProtoType ResolveEarly(string name)
        {
            if (name.Contains("[]"))
            {
                return new ProtoArray(name);
            }
            else
            {
                return name switch
                {
                    "Guid" or "sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "bool" => new ProtoPrimitive(name),
                    "string" => new ProtoString(name),
                    _ => new ProtoUnknown(name),
                };
            }
        }

        public static void IncrementOffset(this ICodeWriter writer, ref int offset, ref bool dynamic, int increment, bool setDynamic = false)
        {
            if (!dynamic && !setDynamic)
            {
                offset += increment;
                return;
            }

            if (setDynamic && !dynamic)
            {
                offset += increment;
                writer.WriteLine($"int idx = {offset};");
                dynamic = true;
                return;
            }

            writer.WriteLine($"idx += {increment};");
        }
    }
}