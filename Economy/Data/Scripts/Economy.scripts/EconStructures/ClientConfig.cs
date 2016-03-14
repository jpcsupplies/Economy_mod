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

        /// <summary>
        /// Identifier which indicates the current mission the player is on.
        /// 0 will represent no current mission.
        /// </summary>
        public int MissionId { get; set; }

        #endregion

        #region hudconfig
        //Hud configuration - needs to be set by client values here are placeholders for testing.
        //If these are being pulled from a client file this will probably cause problems
        //probably should be saved client side
        //probably should get defaults from Economyconsts
        public bool ShowBalance=true;
        public bool ShowRegion=false;
        public bool ShowXYZ=false;
        public bool ShowContractCount=false;
        public bool ShowCargoSpace=false;
        public bool ShowFaction=true;
        public bool ShowHud=false; // if this is false also dont check xyz, cargo and region in updates
        public string LazyMissionText = "Mission: Survive | Deadline: Unlimited";

        #endregion hudconfig
    }
}
