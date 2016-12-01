namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("EconData")]
    public class EconDataStruct
    {
        /// <summary>
        /// The represents the World's credit balance, for unaccounted credits and debits.
        /// Sum with all account balances, should be equal to 0.
        /// </summary>
        public decimal CreditBalance;

        public List<BankAccountStruct> Accounts;

        public List<MarketStruct> Markets;

        public List<OrderBookStruct> OrderBook;

        public List<ShipSaleStruct> ShipSale;
    }
}
