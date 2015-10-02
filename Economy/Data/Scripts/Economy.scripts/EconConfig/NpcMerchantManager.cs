namespace Economy.scripts.EconConfig
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
        public static void VerifyAndCreate(EconDataStruct data)
        {
            // we look up our bank record based on our bogus NPC Steam Id/
            var myNpcAccount = data.Accounts.FirstOrDefault(
                a => a.SteamId == EconomyConsts.NpcMerchantId);
            // Do it have an account already?
            if (myNpcAccount == null)
            {
                //nope, lets construct our bank record with a new balance
                myNpcAccount = AccountManager.CreateNewDefaultAccount(EconomyConsts.NpcMerchantId, EconomyConsts.NpcMerchantName, 0);

                //ok lets apply it
                data.Accounts.Add(myNpcAccount);
                EconomyScript.Instance.ServerLogger.Write("Banker Account Created.");
            }
            else
            {
                EconomyScript.Instance.ServerLogger.Write("Banker Account Exists.");
            }
        }
    }
}
