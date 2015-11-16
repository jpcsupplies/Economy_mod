namespace Economy.scripts.EconStructures
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Serialization;

    [XmlType("EconConfig")]
    public class EconConfigStruct
    {
        public EconConfigStruct()
        {
            // Set defaults.
            CurrencyName = EconomyConsts.CurrencyName;
            TradeNetworkName = EconomyConsts.TradeNetworkName;
            DefaultStartingBalance = EconomyConsts.DefaultStartingBalance;
            LimitedRange = EconomyConsts.LimitedRange;
            LimitedSupply = EconomyConsts.LimitedSupply;
            TradeTimeout = EconomyConsts.TradeTimeout;
        }

        /// <summary>
        /// Name our money.
        /// </summary>
        public string CurrencyName;

        /// <summary>
        /// Name our Trading Network.
        /// </summary>
        public string TradeNetworkName;

        /// <summary>
        /// The starting balance for all new players.
        /// </summary>
        public decimal DefaultStartingBalance;

        /// <summary>
        /// Should players be near each other to trade or should it be unlimited distance.
        /// </summary>
        public bool LimitedRange;

        /// <summary>
        /// Should the NPC market be limited or unlimited supply.
        /// </summary>
        public bool LimitedSupply;

        // Local Variable
        private TimeSpan _tradeTimeout;

        /// <summary>
        /// The time to timeout a player to player trade offer.
        /// </summary>
        /// <remarks>The TimeSpan data type cannot be natively serialized, so we cheat by using 
        /// a proxy property to store it in another acceptable (and human readable) format.</remarks>
        [XmlIgnore]
        public TimeSpan TradeTimeout
        {
            get { return _tradeTimeout; }
            set { _tradeTimeout = value; }
        }

        /// <summary>
        /// Serialization property for TradeTimeout.
        /// </summary>
        [XmlElement("TradeTimeout")]
        public string TradeTimeoutTicks
        {
            get { return _tradeTimeout.ToString("c"); }
            set
            {
                try
                {
                    _tradeTimeout = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _tradeTimeout = EconomyConsts.TradeTimeout;
                    EconomyScript.Instance.ServerLogger.Write("TradeTimeout has been reset, as the stored value '{0}' was invalid.", value);
                }
            }
        }

        public List<MarketItemStruct> DefaultPrices;
    }
}
