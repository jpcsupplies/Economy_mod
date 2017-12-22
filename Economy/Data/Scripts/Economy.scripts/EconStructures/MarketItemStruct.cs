namespace Economy.scripts.EconStructures
{
    using ProtoBuf;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [XmlType("MarketItem")]
    [ProtoContract]
    public class MarketItemStruct
    {
        public MarketItemStruct()
        {
            // Default limit for New Market Items will equal to decimal.MaxValue.
            // Someone is sure to abuse the logic, so a maxiumum stock limit must be established.
            StockLimit = decimal.MaxValue;
        }

        [ProtoMember(1)]
        public string TypeId { get; set; }

        [ProtoMember(2)]
        public string SubtypeName { get; set; }

        [ProtoMember(3)]
        public decimal Quantity { get; set; }

        [ProtoMember(4)]
        public decimal SellPrice { get; set; }

        [ProtoMember(5)]
        public decimal BuyPrice { get; set; }

        [ProtoMember(6)]
        public bool IsBlacklisted { get; set; }

        [DefaultValue(typeof(decimal), "79228162514264337593543950335")] // decimal.MaxValue
        [ProtoMember(7)]
        public decimal StockLimit { get; set; }
    }
}
