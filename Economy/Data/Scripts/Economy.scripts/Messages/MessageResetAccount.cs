namespace Economy.scripts.Messages
{
    using System.Linq;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageResetAccount : MessageBase
    {
        public static void SendMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageResetAccount());
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            if (player != null && player.IsAdmin()) // hold on there, are we an admin first?
            {
                // we look up our bank record based on our Steam Id/
                var myaccount = EconomyScript.Instance.Data.Accounts.FirstOrDefault(
                    a => a.SteamId == SenderSteamId);
                // wait do we even have an account yet? Cant remove whats not there!
                if (myaccount != null)
                {
                    AccountManager.ResetAccount(myaccount);
                    MessageUpdateClient.SendAccountMessage(myaccount);
                }
                else
                {
                    //ok cause i am an admin and everything else checks out, lets construct our bank record with a new balance
                    myaccount = AccountManager.CreateNewDefaultAccount(SenderSteamId, player.DisplayName, SenderLanguage);

                    //ok lets apply it
                    EconomyScript.Instance.Data.Accounts.Add(myaccount);
                    EconomyScript.Instance.Data.CreditBalance -= myaccount.BankBalance;
                }

                MessageClientTextMessage.SendMessage(SenderSteamId, "RESET", "Done");
            }
        }
    }
}
