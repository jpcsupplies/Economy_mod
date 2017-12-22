namespace Economy.scripts.Messages
{
    using System.Linq;
    using ProtoBuf;
    using EconConfig;
    using EconStructures;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;

    [ProtoContract]
    public class MessageConnectionRequest : MessageBase
    {
        [ProtoMember(201)]
        public int ModCommunicationVersion;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("Player '{0}' connected", SenderDisplayName);

            IMyPlayer player = MyAPIGateway.Players.GetPlayer(SenderSteamId);
            if (player == null)
            {
                EconomyScript.Instance.ServerLogger.WriteVerbose($"Player '{SenderDisplayName}' {SenderSteamId} connected with invalid SteamId or before server has player information ready.");
                // If the Server isn't ready yet, the Client will send another Request automatically.
                // If the SteamId is invalid, we'll keep ignoring it anyhow.
                return;
            }

            uint userSecurity = player.UserSecurityLevel();

            if (ModCommunicationVersion != EconomyConsts.ModCommunicationVersion)
            {
                EconomyScript.Instance.ServerLogger.WriteVerbose($"Player '{SenderDisplayName}' {SenderSteamId} connected with invalid version {ModCommunicationVersion}. Should be {EconomyConsts.ModCommunicationVersion}.");

                // TODO: respond to the potentional communication conflict.
                // Could Client be older than Server?
                // It's possible, if the Client has trouble downloading from Steam Community which can happen on occasion.

                MessageConnectionResponse.SendMessage(SenderSteamId, ModCommunicationVersion, EconomyConsts.ModCommunicationVersion, userSecurity);
                return;
            }

            // Client connection is valid at this stage, so we can log it.
            EconomyScript.Instance.ServerLogger.WriteVerbose($"Player '{SenderDisplayName}' {SenderSteamId} connected. Version {ModCommunicationVersion}");

            var account = EconomyScript.Instance.Data.Clients.FirstOrDefault(
                a => a.SteamId == SenderSteamId);

            bool newAccount = false;
            // Create the Bank Account when the player first joins.
            if (account == null)
            {
                EconomyScript.Instance.ServerLogger.WriteInfo("Creating new Bank Account for '{0}'", SenderDisplayName);
                account = AccountManager.CreateNewDefaultAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                EconomyScript.Instance.Data.Clients.Add(account);
                EconomyScript.Instance.Data.CreditBalance -= account.BankBalance;
                newAccount = true;
            }
            else
            {
                AccountManager.UpdateLastSeen(SenderSteamId, SenderDisplayName, SenderLanguage);
            }


            // Is Server version older than what Client is running, or Server version is newer than Client.
            MessageConnectionResponse.SendMessage(SenderSteamId, ModCommunicationVersion, EconomyConsts.ModCommunicationVersion, userSecurity);

            ClientConfig clientConfig = new ClientConfig
            {
                ServerConfig = EconomyScript.Instance.ServerConfig,
                BankBalance = account.BankBalance,
                OpenedDate = account.OpenedDate,
                NewAccount = newAccount,
                ClientHudSettings = account.ClientHudSettings,
                Missions = EconomyScript.Instance.Data.Missions.Where(m => m.PlayerId == SenderSteamId).ToList(),
                Markets = EconomyScript.Instance.Data.Markets.Where(m => m.Open
                    && ((EconomyScript.Instance.ServerConfig.EnableNpcTradezones && m.MarketId == EconomyConsts.NpcMerchantId)
                    || (EconomyScript.Instance.ServerConfig.EnablePlayerTradezones && m.MarketId != EconomyConsts.NpcMerchantId))).ToList()
            };

            EconomyScript.Instance.ServerLogger.WriteInfo($"ClientConfig response: Opened:{clientConfig.OpenedDate}  Balance:{clientConfig.BankBalance}  Hud:{clientConfig.ClientHudSettings.ShowHud}  Missions:{clientConfig.Missions?.Count ?? -1}");

            MessageUpdateClient.SendClientConfig(SenderSteamId, clientConfig);
        }

        public static void SendMessage(int modCommunicationVersion)
        {
            ConnectionHelper.SendMessageToServer(new MessageConnectionRequest { ModCommunicationVersion = modCommunicationVersion });
        }
    }
}
