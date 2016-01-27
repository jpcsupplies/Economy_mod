namespace Economy.scripts.Messages
{
    using EconStructures;
    using ProtoBuf;

    [ProtoContract]
    public class MessageUpdateClient : MessageBase
    {
        [ProtoMember(1)]
        public decimal BankBalance;

        public static void SendMessage(BankAccountStruct account)
        {
            ConnectionHelper.SendMessageToPlayer(account.SteamId, new MessageUpdateClient { BankBalance = account.BankBalance });
        }

        public override void ProcessClient()
        {
            if (EconomyScript.Instance.ClientConfig == null)
            {
                EconomyScript.Instance.ClientLogger.WriteInfo("Error - ClientConfig not yet received from Server!");
                return;
            }

            EconomyScript.Instance.ClientConfig.BankBalance = BankBalance;

            // TODO: we may invoke additional things here after the Client has updated their balance.
            // Like, updating the Hud if it was properly encapsulated.
        }

        public override void ProcessServer()
        {
            // never processed on server
        }
    }
}
