namespace Economy.scripts.EconStructures
{
    using System;
    using System.Collections.Generic;
    using Messages;
    using MissionStructures;
    using ProtoBuf;

    [ProtoContract]
    public class ClientConfig
    {
        #region properties

        /// <summary>
        /// Holds a readonly version of the server config so that we have 
        /// the client check if some commands are enabled without having to pass it off to the server first.
        /// </summary>
        [ProtoMember(1)]
        public ServerConfigUpdateStuct ServerConfig { get; set; }

        /// <summary>
        /// Last read Balance.
        /// </summary>
        [ProtoMember(2)]
        public decimal BankBalance;

        /// <summary>
        /// Is this a new account that has just been created?
        /// </summary>
        [ProtoMember(3)]
        public bool NewAccount;

        /// <summary>
        /// The date the player was first seen and the account created.
        /// </summary>
        [ProtoMember(4)]
        public DateTime OpenedDate;

        #endregion

        #region hudconfig

        /// <summary>
        /// Identifier which indicates the current mission the player is on.
        /// 0 will represent no current mission.
        /// </summary>
        [ProtoMember(6)]
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
        [ProtoMember(7)]
        public string HudReadout { get; set; }

        /// <summary>
        /// String to hold Hud objective, to be updated only when required.
        /// </summary>
        [ProtoMember(8)]
        public string HudObjective { get; set; }

        /// <summary>
        /// String to hold the Hud Faction name.
        /// </summary>
        [ProtoMember(9)]
        public string FactionName { get; set; }

        /// <summary>
        /// String to hold the TradeZone name/s that the player is in.
        /// </summary>
        [ProtoMember(10)]
        public string TradeZoneName { get; set; }

        //Hud configuration - needs to be set by client values here are placeholders for testing.
        //If these are being pulled from a client file this will probably cause problems
        //probably should be saved client side
        //probably should get defaults from Economyconsts
        [ProtoMember(11)]
        public ClientHudSettingsStruct ClientHudSettings { get; set; }

        // if Hud is off, also dont check xyz, cargo and region in updates
        [ProtoMember(12)]
        public string LazyMissionText = "Mission: Survive | Deadline: Unlimited";

        [ProtoMember(13)]
        public int CompletedMissions = 0;

        [ProtoMember(14)]
        public bool SeenBriefing = false;


        #endregion hudconfig

        /// <summary>
        /// Client side temporary store of active missions for this player.
        /// </summary>
        /// <remarks>ProtoBuf treats empty collections as null, so they need to be constructed by default,
        /// otherwise an empty collection will not deserialize.</remarks>
        [ProtoMember(15)]
        public List<MissionBaseStruct> Missions { get; set; } = new List<MissionBaseStruct>();

        /// <summary>
        /// Client side temporary store of active markets for this player.
        /// </summary>
        /// <remarks>ProtoBuf treats empty collections as null, so they need to be constructed by default,
        /// otherwise an empty collection will not deserialize.</remarks>
        [ProtoMember(16)]
        public List<MarketStruct> Markets { get; set; } = new List<MarketStruct>();

        /// <summary>
        /// Array to hold the current TradeZones that the player is in.
        /// </summary>
        [ProtoMember(17)]
        public List<ulong> TradeZones { get; set; } = new List<ulong>();
    }
}
