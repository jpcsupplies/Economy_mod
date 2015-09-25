namespace Economy.scripts.Messages
{
    using System.Linq;

    /// <summary>
    /// checks for a valid NPC trader entry adds one if missing.
    /// </summary>
    public class NpcMerchantManager
    {
        /// <summary>
        /// Check we have our NPC banker ready.
        /// </summary>
        public static void VerifyAndCreate()
        {
            // we look up our bank record based on our bogus NPC Steam Id/
            var myNPCaccount = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                a => a.SteamId == EconomyConsts.NpcMerchantId);
            // Do it have an account already?
            if (myNPCaccount == null)
            {
                //nope, lets construct our bank record with a new balance
                myNPCaccount = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(EconomyConsts.NpcMerchantId, EconomyConsts.NpcMerchantName, 0);

                //ok lets apply it
                EconomyScript.Instance.BankConfigData.Accounts.Add(myNPCaccount);
                EconomyScript.Instance.ServerLogger.Write("Banker Account Created.");
            }
            else
            {
                EconomyScript.Instance.ServerLogger.Write("Banker Account Exists.");
            }
        }
    }
}
