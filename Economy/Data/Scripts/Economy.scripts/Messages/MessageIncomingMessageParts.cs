namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using System.Linq;

    [ProtoContract]
    public class MessageIncomingMessageParts : MessageBase
    {
        [ProtoMember(1)]
        public byte[] Content;

        [ProtoMember(2)]
        public bool LastPart;

        public override void ProcessClient()
        {
            ConnectionHelper.ClientMessageCache.AddRange(Content.ToList());

            if (LastPart)
            {
                ConnectionHelper.ProcessData(ConnectionHelper.ClientMessageCache.ToArray());
                ConnectionHelper.ClientMessageCache.Clear();
            }
        }

        public override void ProcessServer()
        {
            ConnectionHelper.ServerMessageCache[SenderSteamId].AddRange(Content.ToList());

            if (LastPart)
            {
                ConnectionHelper.ProcessData(ConnectionHelper.ServerMessageCache[SenderSteamId].ToArray());
                ConnectionHelper.ServerMessageCache[SenderSteamId].Clear();
            }
        }

    }
}
