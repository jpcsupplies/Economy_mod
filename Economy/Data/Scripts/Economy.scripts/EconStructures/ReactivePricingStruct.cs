namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("EconReactivePricing")]
    public class ReactivePricingStruct
    {
        [XmlElement("EconReactivePrices")]
        public List<PricingStruct> Prices;

        public ReactivePricingStruct()
        {
            Prices = new List<PricingStruct>();
        }
    }
}
