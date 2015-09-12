namespace Economy.scripts.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.ModAPI;

    public class BankConfig
    {
        public List<BankAccountStruct> Accounts;

        #region Generic helpers

        /// <summary>
        /// Load the relevant bank balance data - check player even has an account yet; make one if not
        /// </summary>
        /// <param name="steamId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public BankAccountStruct FindOrCreateAccount(ulong steamId, string nickName)
        {
            var account = Accounts.FirstOrDefault(a => a.SteamId == steamId);
            if (account == null)
            {
                account = CreateNewDefaultAccount(steamId, nickName);
                Accounts.Add(account);
            }
            return account;
        }

        /// <summary>
        /// Find the account by nickName. Does not create a new account.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public BankAccountStruct FindAccount(string nickName)
        {
            // Try for an exact match first.
            var accounts = Accounts.Where(a => a.NickName.Equals(nickName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // only return a result if there was 1 exact match.
            if (accounts.Count == 1)
                return accounts.FirstOrDefault();

            // Try for a partial match if we didn't get any matches before.
            if (accounts.Count == 0)
            {
                accounts = Accounts.Where(a => a.NickName.IndexOf(nickName, StringComparison.InvariantCultureIgnoreCase) > 0).ToList();

                if (accounts.Count == 1)
                    return accounts.FirstOrDefault();
            }

            // Either too many players with the same name, or the name doesn't exist.
            return null;
        }

        public BankAccountStruct CreateNewDefaultAccount(ulong steamId, string nickName)
        {
            return new BankAccountStruct() { BankBalance = 100, Date = DateTime.Now, NickName = nickName, SteamId = steamId };
        }

        public void ResetAccount(BankAccountStruct account)
        {
            account.BankBalance = 100;
            account.Date = DateTime.Now;
        }

        public void UpdateLastSeen(ulong steamId)
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(steamId);
            if (player != null)
            {
                UpdateLastSeen(steamId, player.DisplayName);
            }
        }

        public void UpdateLastSeen(ulong steamId, string nickName)
        {
            var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                    a => a.SteamId == steamId);

            if (account != null)
            {
                account.NickName = nickName;
                account.Date = DateTime.Now;
            }
        }

        #endregion
    }
}
