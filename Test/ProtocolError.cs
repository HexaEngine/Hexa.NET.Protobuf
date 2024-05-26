namespace Test
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct ProtocolError
    {
        public ErrorCode ErrorCode;
        public ErrorSeverity Severity;
    }
}