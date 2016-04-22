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
            AccountExpiry = EconomyConsts.AccountExpiry;
            NPCStartingBalance = EconomyConsts.NPCStartingBalance;
            NpcMerchantName = EconomyConsts.NpcMerchantName;
            DefaultTradeRange = EconomyConsts.DefaultTradeRange;
            Language = (int)VRage.MyLanguagesEnum.English;
            EnableLcds = true;
            EnablePlayerPayments = true;
            EnableNpcTradezones = true;
            EnablePlayerTradezones = false; // TODO: default it off until it is working.
            MaximumPlayerTradeZones = 1;
            TradeZoneLicenceCostMin = 2000;
            TradeZoneLicenceCostMax = 20000000;
            TradeZoneRelinkRatio = 0.5m;
            TradeZoneMinRadius = 1;
            TradeZoneMaxRadius = 5000;
        }

        #region properties

        /// <summary>
        /// Name our money.  This is NOT the symbol (ie., $).
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
        /// The cost to create a Player trade zone when the zone size is at its Minimum allowed radius.
        /// </summary>
        public decimal TradeZoneLicenceCostMin;

        /// <summary>
        /// The cost to create a Player trade zone when the zone size is at its Maximum allowed radius.
        /// </summary>
        public decimal TradeZoneLicenceCostMax;

        /// <summary>
        /// The cost to create a Player trade zone Minimum allowed radius.
        /// </summary>
        public decimal TradeZoneMinRadius;

        /// <summary>
        /// The cost to create a Player trade zone Maximum allowed radius.
        /// </summary>
        public decimal TradeZoneMaxRadius;

        /// <summary>
        /// The cost ratio for Relink/Reestablishing a broken trade zone.
        /// </summary>
        public decimal TradeZoneRelinkRatio;

        /// <summary>
        /// The maximum number of trade zones a player an create.
        /// </summary>
        public int MaximumPlayerTradeZones;

        /// <summary>
        /// Should players be near each other to trade or should it be unlimited distance.
        /// </summary>
        public bool LimitedRange;

        /// <summary>
        /// Should the NPC market be limited or unlimited supply.
        /// </summary>
        public bool LimitedSupply;

        /// <summary>
        /// The starting balance for NPC Bankers.
        /// </summary>
        public decimal NPCStartingBalance;

        /// <summary>
        /// The name that will used to identify the Merchant's server account.
        /// </summary>
        public string NpcMerchantName;

        /// <summary>
        /// Default range of trade zones.
        /// </summary>
        public double DefaultTradeRange;

        /// <summary>
        /// Indicates what language the Servre uses for text in game.
        /// Mapped aginst: VRage.MyLanguagesEnum
        /// Typically retrieved via: MyAPIGateway.Session.Config.Language
        /// </summary>
        public int Language;

        /// <summary>
        /// Indicates the LCD panels can be used to display Trade zone information.
        /// </summary>
        public bool EnableLcds;

        /// <summary>
        /// Indicates that Npc Tradezones can be used.
        /// They can still be created and managed by the Admin, allowing an admin to take the tradezones offline temporarily to fix any issue.
        /// </summary>
        public bool EnableNpcTradezones;  // TODO: split buy and sell?  https://github.com/jpcsupplies/Economy_mod/issues/88

        /// <summary>
        /// Indicates the Player Tradezone can be created and used.
        /// </summary>
        public bool EnablePlayerTradezones;  // TODO: split buy and sell?  https://github.com/jpcsupplies/Economy_mod/issues/88

        /// <summary>
        /// Indicates that Players can send payments to one another.
        /// </summary>
        public bool EnablePlayerPayments;

        /// <summary>
        /// Indicates that Players can Trade with on another.
        /// </summary>
        //public bool EnablePlayerTrades;

        #endregion

        #region TradeTimeout

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
                    EconomyScript.Instance.ServerLogger.WriteWarning("TradeTimeout has been reset, as the stored value '{0}' was invalid.", value);
                }
            }
        }

        #endregion

        #region AccountExpiry

        // Local Variable
        private TimeSpan _accountExpiry;

        /// <summary>
        /// The time to timeout a player to player trade offer.
        /// </summary>
        /// <remarks>The TimeSpan data type cannot be natively serialized, so we cheat by using 
        /// a proxy property to store it in another acceptable (and human readable) format.</remarks>
        [XmlIgnore]
        public TimeSpan AccountExpiry
        {
            get { return _accountExpiry; }
            set { _accountExpiry = value; }
        }

        /// <summary>
        /// Serialization property for AccountExpiry.
        /// </summary>
        [XmlElement("AccountExpiry")]
        public string AccountExpiryTicks
        {
            get { return _accountExpiry.ToString("c"); }
            set
            {
                try
                {
                    _accountExpiry = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _accountExpiry = EconomyConsts.AccountExpiry;
                    EconomyScript.Instance.ServerLogger.WriteWarning("AccountExpiry has been reset, as the stored value '{0}' was invalid.", value);
                }
            }
        }

        #endregion

        public List<MarketItemStruct> DefaultPrices;

        public decimal CalculateZoneCost(decimal radius, bool relink)
        {
            // linear cost on radius.
            return (relink ? TradeZoneRelinkRatio : 1m) * (((radius - TradeZoneMinRadius) / (TradeZoneMaxRadius - TradeZoneMinRadius) * (TradeZoneLicenceCostMax - TradeZoneLicenceCostMin)) + TradeZoneLicenceCostMin);
        }
    }
}
