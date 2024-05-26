namespace Test
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct ClientHello
    {
        public ulong GameVersion;
    }
}