namespace Economy.scripts.Messages
{
    using System.Linq;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageRewardAccount : MessageBase
    {
        [ProtoMember(1)]
        public decimal Reward;

        public static void SendMessage(decimal reward)
        {
            ConnectionHelper.SendMessageToServer(new MessageRewardAccount { Reward = reward});
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            if (player != null) 
            {
                // we look up our bank record based on our Steam Id/
                var myaccount = EconomyScript.Instance.Data.Accounts.FirstOrDefault(
                    a => a.SteamId == SenderSteamId);
                // wait do we even have an account yet? 
                if (myaccount != null)
                {
            //        AccountManager.PayReward(myaccount,Reward); //reward plugs in here
            //        MessageUpdateClient.SendAccountMessage(myaccount);
                }
                else
                {
                    //ok  everything else checks out, lets construct our bank record with a new balance
                    myaccount = AccountManager.CreateNewDefaultAccount(SenderSteamId, player.DisplayName, SenderLanguage);

                    //ok lets apply it
                    EconomyScript.Instance.Data.Accounts.Add(myaccount);
                    EconomyScript.Instance.Data.CreditBalance -= myaccount.BankBalance;
                }
                //create balance if not one already, then add our reward and update client.
                AccountManager.PayReward(myaccount, Reward ); //reward plugs in here
                MessageUpdateClient.SendAccountMessage(myaccount);
                //MessageClientTextMessage.SendMessage(SenderSteamId, "RESET", "Done");
            }
        }
    }
}
