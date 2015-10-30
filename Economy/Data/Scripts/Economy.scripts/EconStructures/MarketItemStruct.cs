﻿namespace Economy.scripts.EconStructures
{
    using System.Xml.Serialization;

    [XmlType("MarketItem")]
    public class MarketItemStruct
    {
        public string TypeId { get; set; }

        public string SubtypeName { get; set; }

        public decimal Quantity { get; set; }

        public decimal SellPrice { get; set; }

        public decimal BuyPrice { get; set; }

        public bool IsBlacklisted { get; set; }
    }
}