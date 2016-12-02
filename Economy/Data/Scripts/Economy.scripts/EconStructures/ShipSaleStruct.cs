namespace Economy.scripts.EconStructures
{
    using System;
    using System.Xml.Serialization;

    [XmlType("ShipSale")]
    public class ShipSaleStruct
    {
        /// <summary>
        /// The date time the ship was put for sale.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// PlayerId of seller.
        /// </summary>
        public long TraderId { get; set; }

        /// <summary>
        /// SteamId of seller.
        /// </summary>
        public ulong TraderSteamId { get; set; }

        /// <summary>
        /// Ship Entity Id.
        /// </summary>
        public long ShipId { get; set; }

        /// <summary>
        /// Sell price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Optional ID of trade station or another player to which a trade is targeted.
        /// As a string, determining the proper datatype is wholly determined by TradeState.
        /// </summary>
        public string OptionalId { get; set; }

    }
}
