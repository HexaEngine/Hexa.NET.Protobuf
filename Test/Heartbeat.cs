namespace Test
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct Heartbeat
    {
        public long Timestamp;
    }
}