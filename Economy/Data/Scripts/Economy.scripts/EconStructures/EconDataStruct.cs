namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("EconData")]
    public class EconDataStruct
    {
        public List<BankAccountStruct> Accounts;

        public List<MarketStruct> Markets;

        public List<OrderBookStruct> OrderBook;
    }
}
