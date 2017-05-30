namespace Economy.scripts.EconConfig
{
    using System;
    using System.Linq;
    using Economy.scripts.EconStructures;
    using Sandbox.ModAPI;

    public static class AccountManager
    {
        #region Account helpers

        /// <summary>
        /// Load the relevant bank balance data - check player even has an account yet; make one if not
        /// </summary>
        /// <param name="steamId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static ClientAccountStruct FindOrCreateAccount(ulong steamId, string nickName, int language)
        {
            var account = EconomyScript.Instance.Data.Clients.FirstOrDefault(a => a.SteamId == steamId);
            if (account == null)
            {
                account = CreateNewDefaultAccount(steamId, nickName, language);
                EconomyScript.Instance.Data.Clients.Add(account);
                EconomyScript.Instance.Data.CreditBalance -= account.BankBalance;
            }
            return account;
        }

        /// <summary>
        /// Find the account by nickName. Does not create a new account.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static ClientAccountStruct FindAccount(string nickName)
        {
            // Try for an exact match first.
            var accounts = EconomyScript.Instance.Data.Clients.Where(a => a.NickName.Equals(nickName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // only return a result if there was 1 exact match.
            if (accounts.Count == 1)
                return accounts.FirstOrDefault();

            // Try for a partial match if we didn't get any matches before.
            if (accounts.Count == 0)
            {
                accounts = EconomyScript.Instance.Data.Clients.Where(a => a.NickName.IndexOf(nickName, StringComparison.InvariantCultureIgnoreCase) > 0).ToList();

                if (accounts.Count == 1)
                    return accounts.FirstOrDefault();
            }

            // Either too many players with the same name, or the name doesn't exist.
            return null;
        }

        public static ClientAccountStruct FindAccount(ulong steamId)
        {
            return EconomyScript.Instance.Data.Clients.FirstOrDefault(a => a.SteamId == steamId);
        }

        public static ClientAccountStruct CreateNewDefaultAccount(ulong steamId, string nickName, int language)
        {
            var create = DateTime.Now; // Keeps the Date and OpenedDate at the same millisecond on creation.
            //are we creating a player or the NPC trader - this may require finer discrimination if we ever have multiple NPCs
            if (steamId != EconomyConsts.NpcMerchantId)
                return new ClientAccountStruct { BankBalance = EconomyScript.Instance.ServerConfig.DefaultStartingBalance, Date = create, NickName = nickName, SteamId = steamId, OpenedDate = create, Language = language };
            else
                return new ClientAccountStruct { BankBalance = EconomyScript.Instance.ServerConfig.NPCStartingBalance, Date = create, NickName = nickName, SteamId = steamId, OpenedDate = create, Language = language };
        }

        public static void ResetAccount(ClientAccountStruct account)
        {
            EconomyScript.Instance.Data.CreditBalance += account.BankBalance;
            EconomyScript.Instance.Data.CreditBalance -= EconomyScript.Instance.ServerConfig.DefaultStartingBalance;
            account.BankBalance = EconomyScript.Instance.ServerConfig.DefaultStartingBalance;
            account.Date = DateTime.Now;
        }

        public static void UpdateLastSeen(ulong steamId, int language)
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(steamId);
            if (player != null)
            {
                UpdateLastSeen(steamId, player.DisplayName, language);
            }
        }

        public static void UpdateLastSeen(ulong steamId, string nickName, int language)
        {
            var account = EconomyScript.Instance.Data.Clients.FirstOrDefault(
                    a => a.SteamId == steamId);

            if (account != null)
            {
                account.NickName = nickName;
                account.Date = DateTime.Now;
                account.Language = language;
            }
        }

        // TODO: this will also need to be called from a scheduler routine, that runs perhaps once an hour to once a day.
        public static void CheckAccountExpiry(EconDataStruct data)
        {
            // Instance and Config are both defined before Data, so this is safe to call here.
            var expireDate = DateTime.Now - EconomyScript.Instance.ServerConfig.AccountExpiry;
            var newAccountExpireDate = DateTime.Now.AddDays(-1);

            var deadAccounts = data.Clients.Where(a =>
                (a.SteamId != EconomyConsts.NpcMerchantId
                && data.Markets.All(m => m.MarketId != a.SteamId))   // Exclude accounts which run markets.
                && (a.Date < expireDate
                || a.Date == a.OpenedDate && a.BankBalance == EconomyScript.Instance.ServerConfig.DefaultStartingBalance && a.Date < newAccountExpireDate)
            ).ToArray();

            foreach (var account in deadAccounts)
            {
                EconomyScript.Instance.ServerLogger.WriteInfo("Removing Dead Account '{0}' with {1} {2}.", account.NickName, account.BankBalance, EconomyScript.Instance.ServerConfig.CurrencyName);
                data.Clients.Remove(account);

                // EconomyScript.Instance.Data would not have been set for the first run though, so we use 'data' instead.
                // Will return positive and negative balance back to the CreditBalance.
                // TODO: do something with the BankBalance if in a faction.
                data.CreditBalance += account.BankBalance;
            }
        }

        #endregion

    }
}
