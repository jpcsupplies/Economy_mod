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
            if (Prefix == "mission")
            {
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("we got this from process client");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(Prefix, Content);
            }
        }

        public override void ProcessServer()
        {
            // never processed on server.
        }

        public static void SendMessage(ulong steamId, string prefix, string content, params object[] args )
        {
            string message;
            if (args == null || args.Length == 0)
                message = content;
            else
                message = string.Format(content, args);
            if (prefix == "mission")
            {
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("we got this from send message");
            }
            else
            {
                ConnectionHelper.SendMessageToPlayer(steamId, new MessageClientTextMessage { Prefix = prefix, Content = message });
            }
        }
    }
}
