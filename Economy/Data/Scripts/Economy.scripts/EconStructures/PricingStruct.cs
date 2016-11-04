namespace Economy.scripts.EconStructures
{
    using System.Xml.Serialization;

    [XmlType("EconPricing")]
    public class PricingStruct
    {
        public int PricePoint;
        public decimal PriceChange;
        public string Description;

        // empty ctor required for serializer.
        public PricingStruct()
        {
        }

        public PricingStruct(int pricePoint, decimal priceChange, string description)
        {
            PricePoint = pricePoint;
            PriceChange = priceChange;
            Description = description;
        }
    }
}
