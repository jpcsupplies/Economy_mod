namespace Economy.scripts.EconStructures
{
    using System;
    using System.Collections.Generic;
    using MissionStructures;

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

        #region hudconfig

        /// <summary>
        /// Identifier which indicates the current mission the player is on.
        /// 0 will represent no current mission.
        /// </summary>
        public int MissionId { get; set; }

        /// <summary>
        /// String to hold Hud text, to be updated only when required.
        /// String element positions:
        /// {0} BankBalance, 
        /// {1} CurrencyName
        /// {2} position.X
        /// {3} position.Y
        /// {4} position.Z
        /// </summary>
        public string HudReadout { get; set; }

        /// <summary>
        /// String to hold Hud objective, to be updated only when required.
        /// </summary>
        public string HudObjective { get; set; }




        //Hud configuration - needs to be set by client values here are placeholders for testing.
        //If these are being pulled from a client file this will probably cause problems
        //probably should be saved client side
        //probably should get defaults from Economyconsts
        public ClientHudSettingsStruct ClientHudSettings { get; set; }
        // if Hud is off, also dont check xyz, cargo and region in updates
        public string LazyMissionText = "Mission: Survive | Deadline: Unlimited";
        public int CompletedMissions = 0;
        public bool SeenBriefing = false;

        #endregion hudconfig

        /// <summary>
        /// Client side temporary store of active missions for this player.
        /// </summary>
        public List<MissionBaseStruct> Missions;
    }
}
