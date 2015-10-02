namespace Economy.scripts.Messages
{
    using System.Linq;
    using System.Text;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageListAccounts : MessageBase
    {
        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            if (player != null && player.IsAdmin()) // hold on there, are we an admin first?
            {
                var description = new StringBuilder();
                var index = 1;
                //probably should add column headings too.. ie number, opened, name, balance, lastseen etc
                foreach (var account in EconomyScript.Instance.Data.Accounts.OrderBy(s => s.NickName))
                {
                    description.AppendFormat("#{0}: {1} : {2} : {3}\r\n", index++, account.NickName, account.BankBalance, account.Date);
                }

                MessageClientDialogMessage.SendMessage(SenderSteamId, "List Accounts",
                    string.Format("Count: {0}", EconomyScript.Instance.Data.Accounts.Count),
                    description.ToString());

                // update our own timestamp here
                AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            }
        }

        public static void SendMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageListAccounts());
        }
    }
}
