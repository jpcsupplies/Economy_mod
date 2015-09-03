using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Economy
{
    public class BankAccountStruct
    {
        public ulong SteamId { get; set; }

        public decimal BankBalance { get; set; }

        public string NickName { get; set; }

        public DateTime Date { get; set; }
    }
}
