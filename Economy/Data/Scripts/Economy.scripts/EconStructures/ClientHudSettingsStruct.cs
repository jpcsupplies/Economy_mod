namespace Economy.scripts.EconStructures
{
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProtoBuf;

    [XmlType("HudSettings")]
    [ProtoContract]
    public class ClientHudSettingsStruct
    {
        /// <summary>
        /// Shows the entire hud.
        /// </summary>
        [ProtoMember(1)]
        [DefaultValue(false)]
        public bool ShowHud { get; set; } = false;

        /// <summary>
        /// Shows the balance in the hud.
        /// </summary>
        [ProtoMember(2)]
        [DefaultValue(true)]
        public bool ShowBalance { get; set; } = true;

        /// <summary>
        /// Shows the Trade Zone Region name in the hud.
        /// </summary>
        [ProtoMember(3)]
        [DefaultValue(false)]
        public bool ShowRegion { get; set; } = false;

        /// <summary>
        /// Shows the position (GPS) in X,Y,Z in the hud.
        /// </summary>
        [ProtoMember(4)]
        [DefaultValue(true)]
        public bool ShowPosition { get; set; } = true;

        /// <summary>
        /// Shows the number of active contracts in the hud.
        /// </summary>
        [ProtoMember(5)]
        [DefaultValue(false)]
        public bool ShowContractCount { get; set; } = false;

        /// <summary>
        /// Shows the available cargo space in the hud.
        /// </summary>
        [ProtoMember(6)]
        [DefaultValue(false)]
        public bool ShowCargoSpace { get; set; } = false;

        /// <summary>
        /// Shows the faction in the hud.
        /// </summary>
        [ProtoMember(7)]
        [DefaultValue(true)]
        public bool ShowFaction { get; set; } = true;
    }
}
