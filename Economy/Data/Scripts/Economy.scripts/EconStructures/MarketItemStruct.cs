namespace Economy.scripts.EconStructures
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    [XmlType("MarketItem")]
    public class MarketItemStruct
    {
        public MarketItemStruct()
        {
            // Default limit for New Market Items will equal to decimal.MaxValue.
            // Someone is sure to abuse the logic, so a maxiumum stock limit must be established.
            StockLimit = decimal.MaxValue;
        }

        public string TypeId { get; set; }

        public string SubtypeName { get; set; }

        public decimal Quantity { get; set; }

        public decimal SellPrice { get; set; }

        public decimal BuyPrice { get; set; }

        public bool IsBlacklisted { get; set; }

        [DefaultValue(typeof(decimal), "79228162514264337593543950335")] // decimal.MaxValue
        public decimal StockLimit { get; set; }
    }
}
