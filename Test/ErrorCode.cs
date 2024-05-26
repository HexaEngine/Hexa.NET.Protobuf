namespace Test
{
    public enum ErrorCode : uint
    {
        None = 0,
        UnknownRecordType = 1,
        SequenceError = 2,
        ProtocolVersionMismatch = 3,
        GameVersionMismatch = 4,
    }
}