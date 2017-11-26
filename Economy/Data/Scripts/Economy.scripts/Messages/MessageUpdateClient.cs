namespace Economy.scripts.Messages
{
    using EconConfig;
    using EconStructures;
    using ProtoBuf;

    [ProtoContract]
    public class MessageUpdateClient : MessageBase
    {
        [ProtoMember(201)]
        public ClientUpdateAction ClientUpdateAction;

        [ProtoMember(202)]
        public decimal BankBalance;

        [ProtoMember(203)]
        public int MissionId;

        [ProtoMember(204)]
        public ServerConfigUpdateStuct ServerConfig { get; set; }

        public static void SendAccountMessage(ClientAccountStruct account)
        {
            ConnectionHelper.SendMessageToPlayer(account.SteamId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.Account, BankBalance = account.BankBalance, MissionId = account.MissionId });
        }

        public static void SendServerConfig(ulong steamdId, EconConfigStruct serverConfig)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.ServerConfig, ServerConfig = serverConfig });
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

                case ClientUpdateAction.ServerConfig:
                    EconomyScript.Instance.ClientConfig.ServerConfig = ServerConfig;
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
