namespace Economy.scripts
{
    /// <summary>
    /// Some of these options will later be configurable in a setting file and/or in game commands but for now set as defaults
    /// </summary>
    public class EconomyConsts
    {
        /// <summary>
        /// The starting balance for all new players.
        /// </summary>
        /// <remarks>This will still be the default value if a Admin does not configure a custom starting balance.</remarks>
        public const decimal DefaultStartingBalance = 100;
    
        /// <summary>
        /// The starting balance for NPC Bankers.
        /// </summary>
        /// <remarks>This will still be the default value if a Admin does not configure a custom starting balance.</remarks>
        public const decimal NPCStartingBalance = 20000;

        /// <summary>
        /// Should players be near each other to trade or should it be unlimited distance
        /// </summary>
        /// <remarks>This sets if players (or traders) should be nearby before being allowed to trade or not</remarks>
        public const bool LimitedRange = false; //default should be true; may be false for testing or gameplay reasons.

        /// <summary>
        /// Should the NPC market be limited or unlimited supply
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
        public const string NpcMerchantName = "NPC";

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
        /// This is used to indicate the base communication version.
        /// </summary>
        /// <remarks>
        /// If we change Message classes or add a new Message class in any way, we need to update this number.
        /// This is because of potentional conflict in communications when we release a new version of the mod.
        /// ie., An established server will be running with version 1. We release a new version with different 
        /// communications classes. A Player will connect to the server, and will automatically download version 2.
        /// We would now have a Client running newer communication classes trying to talk to the Server with older classes.
        /// </remarks>
        public const int ModCommunicationVersion = 20150925; // This will be based on the date of update.

        //
    }
}
