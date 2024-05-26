namespace Test
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct RateLimit
    {
        public long Timestamp;
        public long RateLimitReset;
    }
}