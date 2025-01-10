namespace Test
{
    using Hexa.NET.Protobuf;

    [ProtobufRecord]
    public partial struct Heartbeat
    {
        public long Timestamp;
    }
}