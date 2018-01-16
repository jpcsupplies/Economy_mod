namespace Economy.scripts.EconStructures
{
    using System;
    using System.Xml.Serialization;

    [XmlType("OrderBook")]
    public class OrderBookStruct
    {
        /// <summary>
        /// The date time the trade was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Market from which a player bought an item.
        /// TODO: determine if this is needed for Player-to-player trade instead of the current OptionalId.
        /// </summary>
        public ulong MarketId { get; set; }

        /// <summary>
        /// SteamId of seller, or wanted to buy.
        /// </summary>
        public ulong TraderId { get; set; }

        /// <summary>
        /// Goods that are been sold, or wanted to buy.
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Goods that are been sold, or wanted to buy.
        /// </summary>
        public string SubtypeName { get; set; }

        /// <summary>
        /// The type of trade or its state. buy, sell, frozen, rejected.
        /// </summary>
        public TradeState TradeState { get; set; }

        /// <summary>
        /// Number of units of goods to be sold, or wanted to buy.
        /// When wanted to buy, this may reflect the number left, as the wanted goods may be partially filled by different individuals.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Sell price, or desired buy price per unit of Quantity.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Optional ID of trade station or another player to which a trade is targeted.
        /// As a string, determining the proper datatype is wholly determined by TradeState.
        /// </summary>
        public string OptionalId { get; set; }

        // TODO: https://github.com/jpcsupplies/Economy_mod/issues/46
        // https://github.com/jpcsupplies/Economy_mod/wiki/5:-Data-Structures-and-logic-considerations
    }
}
