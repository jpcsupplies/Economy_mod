namespace Economy.scripts
{
    using System;

    /// <summary>
    /// Some of these options will later be configurable in a setting file and/or in game commands but for now set as defaults
    /// </summary>
    public class EconomyConsts
    {
        /// <summary>
        /// This is used to indicate the base communication version.
        /// </summary>
        /// <remarks>
        /// If we change Message classes or add a new Message class in any way, we need to update this number.
        /// This is because of potentional conflict in communications when we release a new version of the mod.
        /// ie., An established server will be running with version 1. We release a new version with different 
        /// communications classes. A Player will connect to the server, and will automatically download version 2.
        /// We would now have a Client running newer communication classes trying to talk to the Server with older classes.
        /// </remarks>
        public const int ModCommunicationVersion = 20190712; // This will be based on the date of update.


        internal const string WorldStorageConfigFilename = "EconomyConfig.xml";
        internal const string WorldStorageDataFilename = "EconomyData.xml";
        internal const string WorldStoragePricescaleFilename = "EconPriceScale.xml";


        //milestone level A=Alpha B=Beta, dev = development test version or Milestone eg 1.0A Milestone, 1.1A Dev etc
        public const string MajorVer = "Econ 3.399A +SC";

        //Name our money
        public const string CurrencyName = "Credits";

        //Name our Trading Network
        public const string TradeNetworkName = "Blue Mining Inc Trade Network";

        /// <summary>
        /// The is the Id which this mod registers iteself for sending and receiving messages through SE. 
        /// </summary>
        /// <remarks>
        /// This Id needs to be unique with SE and other mods, otherwise it can send/receive  
        /// messages to/from the other registered mod by mistake, and potentially cause SE to crash.
        /// This has been generated randomly.
        /// </remarks>
        public const ushort ConnectionId = 46912;

        /// <summary>
        /// This Id is used specifically for registering for inter Mod messageing, and is used to talk to the Economy Mod.
        /// </summary>
        public const long EconInterModId = 913846912;

        /// <summary>
        /// The default % you need to own to sell a ship.
        /// </summary>
        public const decimal ShipOwned = 80;

        /// <summary>
        /// The starting balance for all new players.
        /// </summary>
        /// <remarks>This will still be the default value if a Admin does not configure a custom starting balance.</remarks>
        public const decimal DefaultStartingBalance = 100;

        /// <summary>
        /// The starting balance for NPC Bankers.
        /// </summary>
        /// <remarks>This will still be the default value if a Admin does not configure a custom starting balance.</remarks>
        public const decimal NPCStartingBalance = 200000;

        /// <summary>Should we have sliding price scales?</summary>
        /// <remarks>This sets if prices should react to available supply.</remarks>
        public const bool DefaultPriceScaling = true;  //default will be true - this should be set/saved in server config 

        /// <summary>
        /// Should players be near each other to trade or should it be unlimited distance.
        /// </summary>
        /// <remarks>This sets if players (or traders) should be nearby before being allowed to trade or not</remarks>
        public const bool DefaultLimitedRange = false; //default should be true; may be false for testing or gameplay reasons.

        /// <summary>
        /// Default range of trade zones.
        /// This is the radius of a sphere. 
        /// </summary>
        public const double DefaultTradeRange = 2500;

        /// <summary>
        /// Should the NPC market be limited or unlimited supply.
        /// </summary>
        /// <remarks>This will be a bool that configures if buying and selling from 0.0.0 trade region 
        /// should be unlimited supply of goods and funds or limited to what has been bought, sold and 
        /// earn by the NPC</remarks>
        public const bool LimitedSupply = true;

        /// <summary>
        /// The internal id we used to identify the Merchant's server account.
        /// </summary> 
        public const ulong NpcMerchantId = 1234;

        /// <summary>
        /// The name that will used to identify the Merchant's server account.
        /// </summary>
        public const string NpcMerchantName = "_Default_NPC_Merchant_";

        public const string NpcMarketName = "Central Market";

        /// <summary>
        /// The default value for timeouts.
        /// </summary>
        public readonly static TimeSpan TradeTimeout = new TimeSpan(0, 2, 0);

        /// <summary>
        /// The default age to allow accounts to be before expiry.
        /// </summary>
        public readonly static TimeSpan AccountExpiry = new TimeSpan(60, 0, 0, 0);

        /// <summary>
        /// The tags that are checked in Text Panels to determine if they are to be used by the Economy Mod.
        /// </summary>
        public readonly static string[] LCDTags = new string[] { "[Economy]", "(Economy)" };

        /// <summary>
        /// Should the NPC market be randomly restocked with simulated trade traffic
        /// </summary>
        /// <remarks>This is a bool which enables or disables the behavior that when an NPC resource is 
        /// exhausted, randomly simulate a random quantity of this resource being sold to the NPC every 5 minutes
        /// It will also randomly sell random quantities of overstocked resources if NPC funds are running low</remarks>
        // public const bool ReSupply = True;

        /// <summary>
        /// Resupply threshold
        /// </summary>
        /// <remarks>This is a quantity representing the over and understock thresholds for Resupply</remarks>
        // public const integer OverStocked = 60000;
        // public const integer UnderStocked = 5;

        /// <summary>
        /// Resupply multiplier
        /// </summary>
        /// <remarks>This is the max amount and multiplier to apply to random number generation for 
        /// resupply of depleted materials.  Eg 50 would represent any random number from 1 to 50
        /// a multiplier of 5 would then multiply that amount.  Eg 2 would make the random chosen number
        /// of 23 into 46, resulting in a sell of 46 items to the NPC character</remarks>
        // public const integer Restock = 50;
        // public const integer multiplier = 2;
    }

    public enum SellAction : byte
    {
        /// <summary>
        /// Creating the Sell order.
        /// </summary>
        Create = 0,

        /// <summary>
        /// Accepting the sell order.
        /// </summary>
        Accept = 1,

        /// <summary>
        /// Seller has cancelled the sell order.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Buyer has rejected the sell order.
        /// </summary>
        Deny = 3,

        /// <summary>
        /// Items to be collected from the sell order (can be returned to the seller, or buyer).
        /// </summary>
        Collect = 4
    }

    [Flags]
    public enum SetMarketItemType : byte
    {
        Quantity = 1,
        BuyPrice = 2,
        SellPrice = 4,
        Blacklisted = 8,
        StockLimit = 16
    }

    /// <summary>
    /// Names need to be explicitly set, as they will be written to the Data file.
    /// Otherwise if we change the names, they will break.
    /// </summary>
    public enum MarketZoneType : byte
    {
        /// <summary>
        /// A fixed sphere shaped region in space that does not move or change size.
        /// </summary>
        FixedSphere = 0,

        /// <summary>
        /// A fixed box shaped region in space that does not move or change size.
        /// </summary>
        FixedBox = 1,

        /// <summary>
        /// A sphere shaped region in space that is centered about an Entity. It does not change size.
        /// </summary>
        EntitySphere = 2,
    }

    /// <summary>
    /// Commands to be used when managing Npc Market zones.
    /// </summary>
    public enum NpcMarketManage : byte
    {
        Add = 0,
        Delete = 1,
        List = 2,
        Rename = 3,
        Move = 4,
        Limit = 5
    }

    /// <summary>
    /// Commands to be used when managing Player Market zones.
    /// </summary>
    public enum PlayerMarketManage : byte
    {
        List = 0,
        Register = 1,
        ConfirmRegister = 2,
        Relink = 3,
        ConfirmRelink = 4,
        Unregister = 5,
        Open = 6,
        Close = 7,
        Load = 8,
        Save = 9,
        FactionMode = 10,
        BuyPrice = 11,
        SellPrice = 12,
        Stock = 13,
        Unstock = 14,
        Restrict = 15,
        Limit = 16,
        Blacklist = 17,
        Export = 18,
    }

    /// <summary>
    /// Names need to be explicitly set, as they will be written to the Data file.
    /// Otherwise if we change the names, they will break.
    /// </summary>
    public enum TradeState
    {
        /// <summary>
        /// Indeterminate, not yet fixed.
        /// </summary>
        None,

        /// <summary>
        /// Trader is wanting to Buy something.
        /// </summary>
        Buy,

        /// <summary>
        /// market buy offer - goes to 0,0,0 stock exchange or a registered market 
        /// (eg faction/merchant/station etc)- its an open ended offer available to anyone 
        /// to fill within range of the desired market territory
        /// </summary>
        BuyWanted,

        /// <summary>
        /// Trader is selling something.
        /// </summary>
        Sell,

        /// <summary>
        /// We are selling/offering directly to a particular player.
        /// </summary>
        SellDirectPlayer,

        /// <summary>
        /// market sell offer - goes to 0,0,0 stock exchange or a registered market 
        /// (eg faction/merchant/station etc)- its an open ended offer available to anyone 
        /// to buy within range of the desired market territory
        /// </summary>
        SellOffer,

        /// <summary>
        /// The Sell was accepted, and the items held for collection.
        /// </summary>
        SellAccepted,

        /// <summary>
        /// The Sell was rejected, and the items held for return.
        /// </summary>
        SellRejected,

        /// <summary>
        /// The Sell has timeout but the player has not yet retrieved their goods.
        /// </summary>
        SellTimedout,

        /// <summary>
        /// transaction/funds have been frozen - either the admin has suspended trading or the 
        /// server is shutting down and/or both the sender and receiver is offline - basically timer frozen
        /// </summary>
        Frozen,

        /// <summary>
        /// funds/items being held due to buyer/seller or payer or payee being offline; but the 
        /// timer has expired.
        /// </summary>
        Holding,
    }

    public enum AttachedGrids
    {
        /// <summary>
        /// All attached grids will be found.
        /// </summary>
        All,

        /// <summary>
        /// Only grids statically attached to that grid, such as by piston or rotor will be found.
        /// </summary>
        Static
    }

    public enum ClientUpdateAction : byte
    {
        ClientConfig = 0,
        Account = 1,
        ServerConfig = 2,
        Missions = 3,
        AllTradeZones = 4,
    }

    public enum MissionType : int
    {
        None,
        Mine,
        Weld,
        JoinFaction,

        TravelToArea,

        DisplayAccountBalance,
        SellOre,
        BuySomething,
        PayPlayer,
        TradeWithPlayer,
        ShipWorth,
        BuySellShip,
        DeliverItemToTradeZone,

        KillPlayer,
        DeactivateBlock,
        ActivateBlock,
        DestroyBlock,
        CaptureBlock,
    }

    public enum PricingBias : byte
    {
        Buy,
        Sell
    }

    /// <summary>
    /// Commands to be used when managing missions.
    /// </summary>
    public enum PlayerMissionManage : byte
    {
        Test = 0,
        AddSample = 1,
        AddMission = 2,
        SyncMission = 3,
        DeleteMission = 4,
        MissionComplete = 5
    }

    public enum MissionAssignmentType : byte
    {
        /// <summary>
        /// Anyone can pick up the mission and do it, including players not yet on server.
        /// </summary>
        Open = 0,

        /// <summary>
        /// Only the assigned players can do the mission.
        /// </summary>
        AssignedPlayers = 1,

        /// <summary>
        /// Only players in the assigned factions can do the mission.
        /// </summary>
        AssignedFactions = 2
    }

    public enum MissionWinRule : byte
    {
        /// <summary>
        /// All players can finish the mission and recieve the reward.
        /// </summary>
        AllPlayers = 0,

        /// <summary>
        /// Only the first player will recieve the reward.
        /// </summary>
        FirstPlayer = 1
    }

    public class ChatCommandSecurity
    {
        /// <summary>
        /// The normal average player can access these command
        /// </summary>
        public const uint User = 0;

        /// <summary>
        /// Can edit scripts when the scripter role is enabled
        /// </summary>
        public const uint Scripter = 50;

        /// <summary>
        /// Can kick and ban players, has access to 'Show All Players' option in Admin Tools menu
        /// </summary>
        public const uint Moderator = 80;

        /// <summary>
        /// Has access to Space Master tools
        /// </summary>
        public const uint SpaceMaster = 100;

        /// <summary>
        /// Has access to Admin tools
        /// </summary>
        public const uint Admin = 200;

        /// <summary>
        /// Admins listed in server config, cannot be demoted
        /// </summary>
        public const uint Owner = 500;
    }
}
