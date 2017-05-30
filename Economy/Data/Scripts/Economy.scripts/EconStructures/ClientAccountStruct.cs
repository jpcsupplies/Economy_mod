namespace Economy.scripts.EconStructures
{
    using System;
    using System.Xml.Serialization;

    [XmlType("ClientAccount")]
    public class ClientAccountStruct
    {
        public ulong SteamId { get; set; }

        /// <summary>
        /// The player's current bank balance.
        /// </summary>
        public decimal BankBalance { get; set; }

        /// <summary>
        /// The player's current name in game, or the last recorded name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// The last time the player was seen.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The date the player was first seen and the account created.
        /// </summary>
        public DateTime OpenedDate { get; set; }

        /// <summary>
        /// Indicates what language the player uses for text in game.
        /// Mapped agsint: VRage.MyLanguagesEnum
        /// Typically retrieved via: MyAPIGateway.Session.Config.Language
        /// </summary>
        public int Language { get; set; }  

        /// <summary>
        /// Identifier which indicates the current mission the player is on.
        /// 0 will represent no current mission.
        /// </summary>
        public int MissionId { get; set; }

        /// <summary>
        /// Holds the client hud settings that the play has saved.
        /// </summary>
        public ClientHudSettingsStruct ClientHudSettings { get; set; }
    }
}
