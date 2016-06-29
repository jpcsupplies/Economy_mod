namespace Economy.scripts.Messages
{
    using EconConfig;
    using EconStructures;
    using ProtoBuf;

    [ProtoContract]
    public class MessageUpdateClient : MessageBase
    {
        [ProtoMember(1)]
        public ClientUpdateAction ClientUpdateAction;

        [ProtoMember(2)]
        public decimal BankBalance;

        [ProtoMember(3)]
        public int MissionId;

        [ProtoMember(8)]
        public string CurrencyName;

        [ProtoMember(9)]
        public string TradeNetworkName;

        public static void SendAccountMessage(BankAccountStruct account)
        {
            ConnectionHelper.SendMessageToPlayer(account.SteamId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.Account, BankBalance = account.BankBalance, MissionId = account.MissionId });
        }

        public static void SendCurrencyName(ulong steamdid, string currencyName)
        {
            ConnectionHelper.SendMessageToPlayer(steamdid, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.CurrencyName, CurrencyName = currencyName });
        }

        public static void SendTradeNetworkName(ulong steamdid, string tradeNetworkName)
        {
            ConnectionHelper.SendMessageToPlayer(steamdid, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.TradeNetworkName, TradeNetworkName = tradeNetworkName });
        }

        public override void ProcessClient()
        {
            if (EconomyScript.Instance.ClientConfig == null)
            {
                EconomyScript.Instance.ClientLogger.WriteInfo("Error - ClientConfig not yet received from Server!");
                return;
            }

            switch (ClientUpdateAction)
            {
                case ClientUpdateAction.Account:
                    EconomyScript.Instance.ClientConfig.BankBalance = BankBalance;
                    EconomyScript.Instance.ClientConfig.MissionId = MissionId;
                    break;

                case ClientUpdateAction.CurrencyName:
                    EconomyScript.Instance.ClientConfig.CurrencyName = CurrencyName;
                    break;

                case ClientUpdateAction.TradeNetworkName:
                    EconomyScript.Instance.ClientConfig.TradeNetworkName = TradeNetworkName;
                    break;

                    case ClientUpdateAction.TradeZones:
                    // TODO: 
                    break;
            }

            HudManager.UpdateHud();
            // TODO: we may invoke additional things here after the Client has updated their balance.
        }

        public override void ProcessServer()
        {
            // never processed on server
        }
    }
}
