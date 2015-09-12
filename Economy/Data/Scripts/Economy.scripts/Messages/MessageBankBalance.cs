namespace Economy.scripts.Messages
{
    using System;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System.Linq;

    [ProtoContract]
    public class MessageBankBalance : MessageBase
    {
        [ProtoMember(1)]
        public string UserName;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            EconomyScript.Instance.BankConfigData.UpdateLastSeen(SenderSteamId);

            EconomyScript.Instance.ServerLogger.Write("Balance Request for '{0}' from '{1}'", UserName, SenderSteamId);

            if (string.IsNullOrEmpty(UserName)) //did we just type bal? show our balance  
            {
                // lets grab the current player data from our bankfile ready for next step
                // we look up our Steam Id/
                var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                    a => a.SteamId == SenderSteamId);

                // check if we actually found it, add default if not
                if (account == null)
                {
                    EconomyScript.Instance.ServerLogger.Write("Creating new Bank Account for '{0}'", SenderDisplayName);
                    account = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(SenderSteamId, SenderDisplayName);
                    EconomyScript.Instance.BankConfigData.Accounts.Add(account);
                }

                MessageClientTextMessage.SendMessage(SenderSteamId, "BALANCE",
                    "Your bank balance is " + account.BankBalance.ToString("0.######"));
            }
            else // if username is supplied, we want to know someone elses balance.
            {
                var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                if (player != null && player.IsAdmin()) // hold on there, are we an admin first?
                {
                    var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                        a => a.NickName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase));

                    string reply;
                    if (account == null)
                        reply = string.Format("Player '{0}' not found Balance: 0", UserName);
                    else
                        reply = string.Format("Player '{0}' Balance: {1}", account.NickName, account.BankBalance);

                    MessageClientTextMessage.SendMessage(SenderSteamId, "BALANCE", reply);
                }
            }
        }

        public static void SendMessage(string userName)
        {
            ConnectionHelper.SendMessageToServer(new MessageBankBalance { UserName = userName });
        }
    }
}
