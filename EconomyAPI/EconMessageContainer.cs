namespace EconomyAPI
{
    using ProtoBuf;

    /// <summary>
    /// This class is a quick workaround to get an abstract class deserialized. It is to be removed when using a byte serializer.
    /// </summary>
    [ProtoContract]
    public class EconMessageContainer
    {
        [ProtoMember(1)]
        public EconInterModBase Content;
    }
}
