namespace Economy.scripts.Messages
{
    using System.Linq;
    using ProtoBuf;
    using EconConfig;
    using EconStructures;

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

            if (ModCommunicationVersion != EconomyConsts.ModCommunicationVersion)
            {
                // TODO: respond to the potentional communication conflict.
                // Could Client be older than Server?
                // It's possible, if the Client has trouble downloading from Steam Community which can happen on occasion.
            }

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
            MessageConnectionResponse.SendMessage(SenderSteamId,
                ModCommunicationVersion < EconomyConsts.ModCommunicationVersion,
                ModCommunicationVersion > EconomyConsts.ModCommunicationVersion,
                new ClientConfig
                {
                    ServerConfig = EconomyScript.Instance.ServerConfig,
                    BankBalance = account.BankBalance,
                    OpenedDate = account.OpenedDate,
                    NewAccount = newAccount,
                    ClientHudSettings = account.ClientHudSettings,
                    Missions = EconomyScript.Instance.Data.Missions.Where(m => m.PlayerId == SenderSteamId).ToList()
                });
        }

        public static void SendMessage(int modCommunicationVersion)
        {
            ConnectionHelper.SendMessageToServer(new MessageConnectionRequest { ModCommunicationVersion = modCommunicationVersion });
        }
    }
}
