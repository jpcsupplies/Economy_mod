namespace Economy.scripts.EconConfig
{
    using System;
    using System.Linq;
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
        public static BankAccountStruct FindOrCreateAccount(ulong steamId, string nickName, int language)
        {
            var account = EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == steamId);
            if (account == null)
            {
                account = CreateNewDefaultAccount(steamId, nickName, language);
                EconomyScript.Instance.Data.Accounts.Add(account);
            }
            return account;
        }

        /// <summary>
        /// Find the account by nickName. Does not create a new account.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static BankAccountStruct FindAccount(string nickName)
        {
            // Try for an exact match first.
            var accounts = EconomyScript.Instance.Data.Accounts.Where(a => a.NickName.Equals(nickName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // only return a result if there was 1 exact match.
            if (accounts.Count == 1)
                return accounts.FirstOrDefault();

            // Try for a partial match if we didn't get any matches before.
            if (accounts.Count == 0)
            {
                accounts = EconomyScript.Instance.Data.Accounts.Where(a => a.NickName.IndexOf(nickName, StringComparison.InvariantCultureIgnoreCase) > 0).ToList();

                if (accounts.Count == 1)
                    return accounts.FirstOrDefault();
            }

            // Either too many players with the same name, or the name doesn't exist.
            return null;
        }

        public static BankAccountStruct FindAccount(ulong steamId)
        {
            return EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == steamId);
        }

        public static BankAccountStruct CreateNewDefaultAccount(ulong steamId, string nickName, int language)
        {
            var create = DateTime.Now; // Keeps the Date and OpenedDate at the same millisecond on creation.
            //are we creating a player or the NPC trader - this may require finer discrimination if we ever have multiple NPCs
            if (steamId != EconomyConsts.NpcMerchantId)
                return new BankAccountStruct() { BankBalance = EconomyConsts.DefaultStartingBalance, Date = create, NickName = nickName, SteamId = steamId, OpenedDate = create, Language = language };
            else
                return new BankAccountStruct() { BankBalance = EconomyConsts.NPCStartingBalance, Date = create, NickName = nickName, SteamId = steamId, OpenedDate = create, Language = language };
        }

        public static void ResetAccount(BankAccountStruct account)
        {
            account.BankBalance = EconomyConsts.DefaultStartingBalance;
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
            var account = EconomyScript.Instance.Data.Accounts.FirstOrDefault(
                    a => a.SteamId == steamId);

            if (account != null)
            {
                account.NickName = nickName;
                account.Date = DateTime.Now;
                account.Language = language;
            }
        }

        #endregion

    }
}
