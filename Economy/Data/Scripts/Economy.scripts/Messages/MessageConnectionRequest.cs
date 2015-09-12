using System.Linq;

namespace Economy.scripts.Messages
{
    using System.Collections.Generic;

    public class MessageConnectionRequest : MessageBase
    {
        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            EconomyScript.Instance.ServerLogger.Write("Player '{0}' connected", SenderDisplayName);

            var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                a => a.SteamId == SenderSteamId);

            // Create the Bank Account when the player first joins.
            if (account == null)
            {
                EconomyScript.Instance.ServerLogger.Write("Creating new Bank Account for '{0}'", SenderDisplayName);
                account = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(SenderSteamId, SenderDisplayName);
                EconomyScript.Instance.BankConfigData.Accounts.Add(account);
            }
            else
            {
                EconomyScript.Instance.BankConfigData.UpdateLastSeen(SenderSteamId, SenderDisplayName);
            }
        }
    }
}
