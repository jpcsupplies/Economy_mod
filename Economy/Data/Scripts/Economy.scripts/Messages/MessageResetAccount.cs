namespace Economy.scripts.Messages
{
    using System.Linq;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageResetAccount : MessageBase
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
                // we look up our bank record based on our Steam Id/
                var myaccount = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                    a => a.SteamId == SenderSteamId);
                // wait do we even have an account yet? Cant remove whats not there!
                if (myaccount != null)
                {
                    EconomyScript.Instance.BankConfigData.ResetAccount(myaccount);
                }
                else
                {
                    //ok cause i am an admin and everything else checks out, lets construct our bank record with a new balance
                    myaccount = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(SenderSteamId, player.DisplayName, SenderLanguage);

                    //ok lets apply it
                    EconomyScript.Instance.BankConfigData.Accounts.Add(myaccount);
                }

                MessageClientTextMessage.SendMessage(SenderSteamId, "RESET", "Done");
            }
        }

        public static void SendMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageResetAccount());
        }
    }
}
