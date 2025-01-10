namespace Test
{
    using Hexa.NET.Protobuf;

    [ProtobufRecord]
    public partial struct ClientHello
    {
        public ulong GameVersion;
    }
}