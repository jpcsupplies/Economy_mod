namespace Economy.scripts.EconConfig
{
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

        // TODO: other Buy states.

        /// <summary>
        /// Trader is selling something.
        /// </summary>
        Sell,

        /// <summary>
        /// Trade directly to a player
        /// </summary>
        SellDirectPlayer,

        /// <summary>
        /// The Sell was rejected, and the items held for return.
        /// </summary>
        SellRejected,

        /// <summary>
        /// The Sell has timeout but the player has not yet retrieved their goods.
        /// </summary>
        SellTimedout,

        // whatever these are, their purpose needs to be described exactly.
        //Frozen, ?
        //Holding, ?
        //Offer, ?
        //ToPlayer, ?
    }
}
