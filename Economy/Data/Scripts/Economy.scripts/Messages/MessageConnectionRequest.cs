namespace Economy.scripts.Messages
{
    using System.Linq;
    using ProtoBuf;
    using EconConfig;

    public class MessageConnectionRequest : MessageBase
    {
        [ProtoMember(1)]
        public int ModCommunicationVersion;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            EconomyScript.Instance.ServerLogger.Write("Player '{0}' connected", SenderDisplayName);

            if (ModCommunicationVersion != EconomyConsts.ModCommunicationVersion)
            {
                // TODO: respond to the potentional communication conflict.
                // Could Client be older than Server?
                // It's possible, if the Client has trouble downloading from Steam Community which can happen on occasion.
            }

            // Is Server version older than what Client is running, or Server version is newer than Client.
            MessageConnectionResponse.SendMessage(SenderSteamId, 
                ModCommunicationVersion < EconomyConsts.ModCommunicationVersion, ModCommunicationVersion > EconomyConsts.ModCommunicationVersion, EconomyScript.Instance.Config.TradeNetworkName);

            var account = EconomyScript.Instance.Data.Accounts.FirstOrDefault(
                a => a.SteamId == SenderSteamId);

            // Create the Bank Account when the player first joins.
            if (account == null)
            {
                EconomyScript.Instance.ServerLogger.Write("Creating new Bank Account for '{0}'", SenderDisplayName);
                account = AccountManager.CreateNewDefaultAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                EconomyScript.Instance.Data.Accounts.Add(account);
            }
            else
            {
                AccountManager.UpdateLastSeen(SenderSteamId, SenderDisplayName, SenderLanguage);
            }
        }

        public static void SendMessage(int modCommunicationVersion)
        {
            ConnectionHelper.SendMessageToServer(new MessageConnectionRequest { ModCommunicationVersion = modCommunicationVersion });
        }
    }
}
