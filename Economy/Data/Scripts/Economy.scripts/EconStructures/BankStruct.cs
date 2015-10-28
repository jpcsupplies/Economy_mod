namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("Bank")]
    public class BankStruct
    {
        public List<BankAccountStruct> Accounts;
    }
}
