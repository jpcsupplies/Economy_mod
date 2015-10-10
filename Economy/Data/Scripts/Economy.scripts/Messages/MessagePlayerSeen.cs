namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using ProtoBuf;

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
            var account = EconomyScript.Instance.Data.Accounts.FirstOrDefault(
                a => a.NickName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase));

            string reply;

            if (account == null)
                reply = "Player not found";
            else
                reply = "Player " + account.NickName + " Last seen: " + account.Date;
                //reply = string.Format("Player {0} Last seen: {1:%d} days {1:hh\\:mm\\:ss}", account.NickName, (account.Date - DateTime.Now));
            // TODO: https://github.com/jpcsupplies/Economy_mod/issues/54

            MessageClientTextMessage.SendMessage(SenderSteamId, "SEEN", reply);

            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
        }

        public static void SendMessage(string userName)
        {
            ConnectionHelper.SendMessageToServer(new MessagePlayerSeen { UserName = userName });
        }
    }
}
