namespace Test
{
    using Hexa.NET.Protobuf;

    [ProtobufRecord]
    public partial struct ServerHello
    {
        public ulong GameVersion;
        public uint HeartbeatRate;
        public uint RateLimit;
        public uint PayloadLimit;
    }
}