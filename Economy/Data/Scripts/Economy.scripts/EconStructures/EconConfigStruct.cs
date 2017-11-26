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
            // If this class ever has to be serialized to Binary and use ProtoBuf,
            // all these values will need to be set on the properties, and also have a DefaultValueAttribute of the same value.
            CurrencyName = EconomyConsts.CurrencyName;
            TradeNetworkName = EconomyConsts.TradeNetworkName;
            DefaultStartingBalance = EconomyConsts.DefaultStartingBalance;
            LimitedRange = EconomyConsts.DefaultLimitedRange;
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
            EnablePlayerTradezones = true;
            MaximumPlayerTradeZones = 1;
            TradeZoneLicenceCostMin = 20000;
            TradeZoneLicenceCostMax = 20000000;
            TradeZoneRelinkRatio = 0.5m;
            TradeZoneMinRadius = 20;
            TradeZoneMaxRadius = 5000;
            PriceScaling = EconomyConsts.DefaultPriceScaling;
            ShipTrading = true;
            MinimumLcdDisplayInterval = 1;
            EnableMissions = false;
        }

        ///// <summary>
        ///// This will allow the serializer to automatically execute the Action in the same step as Deserialization.
        ///// </summary>
        //[ProtoAfterDeserialization] // not yet whitelisted.
        //void AfterDeserialization() // is not invoked after deserialization from xml
        //{
        //}

        #region properties

        /// <summary>
        /// Name our money.  This is NOT the symbol (ie., $).
        /// </summary>
        public string CurrencyName { get; set; }

        /// <summary>
        /// Name our Trading Network.
        /// </summary>
        public string TradeNetworkName { get; set; }

        /// <summary>
        /// The starting balance for all new players.
        /// </summary>
        public decimal DefaultStartingBalance { get; set; }

        /// <summary>
        /// The cost to create a Player trade zone when the zone size is at its Minimum allowed radius.
        /// </summary>
        public decimal TradeZoneLicenceCostMin { get; set; }

        /// <summary>
        /// The cost to create a Player trade zone when the zone size is at its Maximum allowed radius.
        /// </summary>
        public decimal TradeZoneLicenceCostMax { get; set; }

        /// <summary>
        /// The cost to create a Player trade zone Minimum allowed radius.
        /// </summary>
        public decimal TradeZoneMinRadius { get; set; }

        /// <summary>
        /// The cost to create a Player trade zone Maximum allowed radius.
        /// </summary>
        public decimal TradeZoneMaxRadius { get; set; }

        /// <summary>
        /// The cost ratio for Relink/Reestablishing a broken trade zone.
        /// </summary>
        public decimal TradeZoneRelinkRatio { get; set; }

        /// <summary>
        /// The maximum number of trade zones a player an create.
        /// </summary>
        public int MaximumPlayerTradeZones { get; set; }

        /// <summary>
        /// Should players be near each other to trade or should it be unlimited distance.
        /// </summary>
        public bool LimitedRange { get; set; }

        /// <summary>
        /// Should the NPC market be limited or unlimited supply.
        /// </summary>
        public bool LimitedSupply { get; set; }

        /// <summary>
        /// The starting balance for NPC Bankers.
        /// </summary>
        public decimal NPCStartingBalance { get; set; }

        /// <summary>
        /// The name that will used to identify the Merchant's server account.
        /// </summary>
        public string NpcMerchantName { get; set; }

        /// <summary>
        /// Default range of trade zones.
        /// </summary>
        public double DefaultTradeRange { get; set; }

        /// <summary>
        /// Indicates what language the Servre uses for text in game.
        /// Mapped aginst: VRage.MyLanguagesEnum
        /// Typically retrieved via: MyAPIGateway.Session.Config.Language
        /// </summary>
        public int Language { get; set; }

        /// <summary>
        /// Indicates the LCD panels can be used to display Trade zone information.
        /// </summary>
        public bool EnableLcds { get; set; }

        /// <summary>
        /// Indicates that Npc Tradezones can be used.
        /// They can still be created and managed by the Admin, allowing an admin to take the tradezones offline temporarily to fix any issue.
        /// </summary>
        public bool EnableNpcTradezones { get; set; }  // TODO: split buy and sell?  https://github.com/jpcsupplies/Economy_mod/issues/88

        /// <summary>
        /// Indicates the Player Tradezone can be created and used.
        /// </summary>
        public bool EnablePlayerTradezones { get; set; }  // TODO: split buy and sell?  https://github.com/jpcsupplies/Economy_mod/issues/88

        /// <summary>
        /// Indicates that Players can send payments to one another.
        /// </summary>
        public bool EnablePlayerPayments { get; set; }

        ///// <summary>
        ///// Indicates that Players can Trade with on another.
        ///// </summary>
        //public bool EnablePlayerTrades { get; set; }

        /// <summary>
        /// This sets if prices should react to available supply.
        /// </summary>
        public bool PriceScaling { get; set; }

        /// <summary>
        /// This sets if players can buy and sell ships.
        /// </summary>
        public bool ShipTrading { get; set; }

        /// <summary>
        /// Users can specify the LCD display refresh interval, but this will restrict
        /// the minimum LCD display refresh server wide for all Economy tagged lcds.
        /// This should never be set less than 1.
        /// </summary>
        public decimal MinimumLcdDisplayInterval { get; set; }

        /// <summary>
        /// Indicates that missions can be used.
        /// </summary>
        public bool EnableMissions { get; set; }

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
