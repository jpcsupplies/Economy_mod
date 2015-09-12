namespace Economy.scripts.Messages
{
    using System;
    using ProtoBuf;
    using System.Linq;

    [ProtoContract]
    public class MessagePlayerSeen : MessageBase
    {
        [ProtoMember(1)]
        public string UserName;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                a => a.NickName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase));

            string reply;

            if (account == null)
                reply = "Player not found";
            else
                reply = "Player " + account.NickName + " Last seen: " + account.Date;

            MessageClientTextMessage.SendMessage(SenderSteamId, "SEEN", reply);

            // update our own timestamp here
            EconomyScript.Instance.BankConfigData.UpdateLastSeen(SenderSteamId);
        }

        public static void SendMessage(string userName)
        {
            ConnectionHelper.SendMessageToServer(new MessagePlayerSeen { UserName = userName });
        }
    }
}
