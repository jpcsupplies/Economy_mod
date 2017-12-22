namespace Economy.scripts.EconStructures
{
    using ProtoBuf;

    /// <summary>
    /// A cut down version of <see cref="EconConfigStruct"/> to hold properties that must be synced to the Client.
    /// </summary>
    [ProtoContract]
    public class ServerConfigUpdateStuct
    {
        /// <summary>
        /// Name our money.  This is NOT the symbol (ie., $).
        /// </summary>
        [ProtoMember(1)]
        public string CurrencyName { get; set; }

        /// <summary>
        /// Name our Trading Network.
        /// </summary>
        [ProtoMember(2)]
        public string TradeNetworkName { get; set; }

        /// <summary>
        /// Indicates that missions can be used.
        /// </summary>
        [ProtoMember(3)]
        public bool EnableMissions { get; set; }

        /// <summary>
        /// Indicates the Player Tradezone can be created and used.
        /// </summary>
        [ProtoMember(4)]
        public bool EnablePlayerTradezones { get; set; }

        /// <summary>
        /// Indicates that Npc Tradezones can be used.
        /// </summary>
        [ProtoMember(5)]
        public bool EnableNpcTradezones { get; set; }

        public static implicit operator ServerConfigUpdateStuct(EconConfigStruct serverConfig)
        {
            return new ServerConfigUpdateStuct
            {
                CurrencyName = serverConfig.CurrencyName,
                TradeNetworkName = serverConfig.TradeNetworkName,
                EnableMissions = serverConfig.EnableMissions,
                EnableNpcTradezones = serverConfig.EnableNpcTradezones,
                EnablePlayerTradezones = serverConfig.EnablePlayerTradezones,
            };
        }
    }
}
