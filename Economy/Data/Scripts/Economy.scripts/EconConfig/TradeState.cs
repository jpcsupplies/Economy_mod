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
}
