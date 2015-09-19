namespace Economy.scripts.EconConfig
{
    using System;

    public class BankAccountStruct
    {
        public ulong SteamId { get; set; }

        public decimal BankBalance { get; set; }

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
    }
}
