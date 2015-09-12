namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageClientTextMessage : MessageBase
    {
        [ProtoMember(1)]
        public string Prefix;

        [ProtoMember(2)]
        public string Content;

        public override void ProcessClient()
        {
            MyAPIGateway.Utilities.ShowMessage(Prefix, Content);
        }

        public override void ProcessServer()
        {
            // never processed on server.
        }

        public static void SendMessage(ulong steamId, string prefix, string content)
        {
            ConnectionHelper.SendMessageToPlayer(steamId, new MessageClientTextMessage { Prefix = prefix, Content = content });
        }
    }
}
