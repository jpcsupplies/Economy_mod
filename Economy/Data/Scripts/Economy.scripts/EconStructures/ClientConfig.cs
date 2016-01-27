namespace Economy.scripts.EconStructures
{
    using System;

    public class ClientConfig
    {
        #region properties

        /// <summary>
        /// Name of our money.
        /// </summary>
        public string CurrencyName;

        /// <summary>
        /// Name our Trading Network.
        /// </summary>
        public string TradeNetworkName;

        /// <summary>
        /// Last read Balance.
        /// </summary>
        public decimal BankBalance;

        /// <summary>
        /// Is this a new account that has just been created?
        /// </summary>
        public bool NewAccount;

        /// <summary>
        /// The date the player was first seen and the account created.
        /// </summary>
        public DateTime OpenedDate;

        #endregion
    }
}
