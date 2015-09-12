﻿namespace Economy.scripts.Messages
{
    using System.Text;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System.Linq;

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
                foreach (var account in EconomyScript.Instance.BankConfigData.Accounts.OrderBy(s => s.NickName))
                {
                    description.AppendFormat("#{0}: {1} : {2}\r\n", index++, account.NickName, account.BankBalance);
                }

                MessageClientDialogMessage.SendMessage(SenderSteamId, "List Accounts",
                    string.Format("Count: {0}", EconomyScript.Instance.BankConfigData.Accounts.Count),
                    description.ToString());

                // update our own timestamp here
                EconomyScript.Instance.BankConfigData.UpdateLastSeen(SenderSteamId);
            }
        }

        public static void SendMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageListAccounts());
        }
    }
}
