namespace Economy.scripts.EconStructures
{
    using System.Xml.Serialization;

    [XmlType("HudSettings")]
    public class ClientHudSettingsStruct
    {
        /// <summary>
        /// Shows the entire hud.
        /// </summary>
        public bool ShowHud { get; set; }

        /// <summary>
        /// Shows the balance in the hud.
        /// </summary>
        public bool ShowBalance { get; set; }

        /// <summary>
        /// Shows the Region in the hud.
        /// </summary>
        public bool ShowRegion { get; set; }

        /// <summary>
        /// Shows the position (GPS) in X,Y,Z in the hud.
        /// </summary>
        public bool ShowPosition { get; set; }

        /// <summary>
        /// Shows the number of active contracts in the hud.
        /// </summary>
        public bool ShowContractCount { get; set; }

        /// <summary>
        /// Shows the available cargo space in the hud.
        /// </summary>
        public bool ShowCargoSpace { get; set; }

        /// <summary>
        /// Shows the faction in the hud.
        /// </summary>
        public bool ShowFaction { get; set; }

        public ClientHudSettingsStruct()
        {
            // set defaults.
            ShowHud = false;
            ShowBalance = true;
            ShowRegion = false;
            ShowPosition = true;
            ShowContractCount = false;
            ShowCargoSpace = false;
            ShowFaction = true;
        }
    }
}
