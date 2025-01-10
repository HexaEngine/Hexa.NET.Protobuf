namespace Test
{
    using Hexa.NET.Protobuf;

    [ProtobufRecord]
    public partial struct ProtocolError
    {
        public ErrorCode ErrorCode;
        public ErrorSeverity Severity;
    }
}