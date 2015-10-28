namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("EconConfig")]
    public class EconConfigStruct
    {
        public List<MarketItemStruct> DefaultPrices;
    }
}
