namespace Economy.scripts.Messages
{
    using System;
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
            ConnectionHelper.SendMessageToServer(new MessageRewardAccount { Reward = reward });
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
                // create balance if not one already, then add our reward and update client.
                var myaccount = AccountManager.FindOrCreateAccount(SenderSteamId, player.DisplayName, SenderLanguage);

                EconomyScript.Instance.Data.CreditBalance -= Reward;
                myaccount.BankBalance += Reward;
                myaccount.Date = DateTime.Now;

                MessageUpdateClient.SendAccountMessage(myaccount);
                //MessageClientTextMessage.SendMessage(SenderSteamId, "Reward", "Done");
            }
        }
    }
}
