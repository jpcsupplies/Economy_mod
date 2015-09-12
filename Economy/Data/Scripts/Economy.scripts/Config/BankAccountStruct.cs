namespace Economy.scripts.Config
{
    using System;

    public class BankAccountStruct
    {
        public ulong SteamId { get; set; }

        public decimal BankBalance { get; set; }

        public string NickName { get; set; }

        public DateTime Date { get; set; }
    }
}
