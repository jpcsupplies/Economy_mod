namespace Economy.scripts.Messages
{
    using EconConfig;
    using EconStructures;
    using MissionStructures;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System.Collections.Generic;
    using System.Linq;

    [ProtoContract]
    public class MessageUpdateClient : MessageBase
    {
        #region properties

        [ProtoMember(201)]
        public ClientUpdateAction ClientUpdateAction;

        [ProtoMember(202)]
        public ClientConfig ClientConfig;

        [ProtoMember(203)]
        public decimal BankBalance;

        [ProtoMember(204)]
        public int MissionId;

        [ProtoMember(205)]
        public ServerConfigUpdateStuct ServerConfig { get; set; }

        [ProtoMember(206)]
        public List<MissionBaseStruct> Missions { get; set; } = new List<MissionBaseStruct>();

        [ProtoMember(207)]
        public List<MarketStruct> Markets { get; set; } = new List<MarketStruct>();

        #endregion

        #region Send Message

        public static void SendClientConfig(ulong steamdId, ClientConfig clientConfig)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.ClientConfig, ClientConfig = clientConfig });
        }

        public static void SendAccountMessage(ClientAccountStruct account)
        {
            ConnectionHelper.SendMessageToPlayer(account.SteamId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.Account, BankBalance = account.BankBalance, MissionId = account.MissionId });
        }

        public static void SendServerConfig(EconConfigStruct serverConfig)
        {
            ConnectionHelper.SendMessageToAllPlayers(new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.ServerConfig, ServerConfig = serverConfig });
        }

        public static void SendServerConfig(ulong steamdId, EconConfigStruct serverConfig)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.ServerConfig, ServerConfig = serverConfig });
        }

        public static void SendServerMissions(ulong steamdId)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageUpdateClient { ClientUpdateAction = ClientUpdateAction.Missions, Missions = EconomyScript.Instance.Data.Missions.Where(m => m.PlayerId == steamdId).ToList() });
        }

        #endregion

        // TODO: Send updates for 1 specific tradezone to reduce traffic. Update, Add, Delete.

        /// <summary>
        /// Send all open market details to all players.
        /// </summary>
        public static void SendServerTradeZones()
        {
            ConnectionHelper.SendMessageToAllPlayers(new MessageUpdateClient
            {
                ClientUpdateAction = ClientUpdateAction.AllTradeZones,
                Markets = EconomyScript.Instance.Data.Markets.Where(m => m.Open
                    && ((EconomyScript.Instance.ServerConfig.EnableNpcTradezones && m.MarketId == EconomyConsts.NpcMerchantId)
                    || (EconomyScript.Instance.ServerConfig.EnablePlayerTradezones && m.MarketId != EconomyConsts.NpcMerchantId))).ToList()
            });
        }

        public override void ProcessClient()
        {
            if (ClientUpdateAction == ClientUpdateAction.ClientConfig)
            {
                // protection for new clients out of sync with older server net yet updated.
                if (ClientConfig.ClientHudSettings == null)
                    ClientConfig.ClientHudSettings = new ClientHudSettingsStruct();

                EconomyScript.Instance.ClientConfig = ClientConfig;
                EconomyScript.Instance.ClientLogger.WriteInfo($"ClientConfig received; Opened:{ClientConfig.OpenedDate}  Balance:{ClientConfig.BankBalance}  Hud:{ClientConfig.ClientHudSettings.ShowHud}  Missions:{ClientConfig.Missions?.Count ?? -1}  Markets:{ClientConfig.Markets?.Count ?? -1}");

                #region Initialise trade network hud

                // Set's up the hud for use.
                MyAPIGateway.Utilities.GetObjectiveLine().Title = EconomyScript.Instance.ClientConfig.ServerConfig.TradeNetworkName;
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("");

                if (!ClientConfig.ClientHudSettings.ShowHud)
                    MyAPIGateway.Utilities.ShowMessage("Economy", "Trade Network Connected! Type '/hud on' to display status.");
                MyAPIGateway.Utilities.ShowMessage("Economy", "Welcome to the {0} System!", EconomyScript.Instance.ClientConfig.ServerConfig.TradeNetworkName);
                MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/ehelp' for more informations about available commands");

                // if we need to switch to next mission use this MyAPIGateway.Utilities.GetObjectiveLine().AdvanceObjective();
                //text of current objective useful for showmissionscreen string etc MyAPIGateway.Utilities.GetObjectiveLine().CurrentObjective;
                // bool?? MyAPIGateway.Utilities.GetObjectiveLine().Equals;
                // turn hud off MyAPIGateway.Utilities.GetObjectiveLine().Hide();
                //change objectives text in array MyAPIGateway.Utilities.GetObjectiveLine().Objectives[element]="blah";
                // increment decrement not sure how MyAPIGateway.Utilities.GetObjectiveLine().Visible;
                // probably easier to clear() then repopulate, i cant see how to decriment properly 
                //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.add|remove etc

                // initilize and turn on hud as required.
                HudManager.UpdateHud();
                if (ClientConfig.ClientHudSettings.ShowHud) { MyAPIGateway.Utilities.GetObjectiveLine().Show(); }

                #endregion Initialise trade network hud

                return;
            }

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

                case ClientUpdateAction.Missions:
                    EconomyScript.Instance.ClientLogger.WriteInfo("ClientUpdate received; Missions:{0}", Missions?.Count ?? -1);
                    // TODO: needs an object lock to prevent sync conflict.
                    EconomyScript.Instance.ClientConfig.Missions = Missions;
                    break;

                case ClientUpdateAction.AllTradeZones:
                    EconomyScript.Instance.ClientLogger.WriteInfo("ClientUpdate received; Markets:{0}", Markets?.Count ?? -1);
                    // TODO: needs an object lock to prevent sync conflict.
                    EconomyScript.Instance.ClientConfig.Markets = Markets;
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
