namespace Hexa.NET.Protobuf
{
    using System;

    [AttributeUsage(AttributeTargets.Struct)]
    public class ProtobufRecordAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ProtobufMemberAttribute : Attribute
    {
        public ProtobufMemberAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ProtobufIgnore : Attribute
    {
    }
}