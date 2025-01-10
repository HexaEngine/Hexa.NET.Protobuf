namespace Test
{
    using Hexa.NET.Protobuf;

    [ProtobufRecord]
    public partial struct RateLimit
    {
        public long Timestamp;
        public long RateLimitReset;
    }
}