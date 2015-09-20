namespace Economy.scripts.Messages
{   //checks for a valid NPC trader entry adds one if missing - hope i got this right its based on MessageResetAccount
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System.Linq;

    [ProtoContract]
    public class CheckNPC : MessageBase
    {
        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {   //This might need to be tweaked to behave correctly in a DS context
            //afaik this section is not needed yet
            // we look up our bank record based on our bogus NPC Steam Id/
            /* var myNPCaccount = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                a => a.SteamId == 1234);
            // Do it have an account already?
            if (myNPCaccount == null)
            {
                //nope, lets construct our bank record with a new balance
                myNPCaccount = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(1234, "NPC", 0);

                //ok lets apply it
                EconomyScript.Instance.BankConfigData.Accounts.Add(myNPCaccount);
                //MessageClientTextMessage.SendMessage(SenderSteamId, "Banker", "Created");
                MyAPIGateway.Utilities.ShowMessage("Banker", "Created");
                }   */        
        } 

        public static void SendMessage()
        {
            //ConnectionHelper.SendMessageToServer(new CheckNPC());
            //duplicating code in process since it didn't seem to trigger there, this may be wrong
            // we look up our bank record based on our bogus NPC Steam Id/
            var myNPCaccount = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                a => a.SteamId == 1234);
            // Do it have an account already?
            if (myNPCaccount == null)
            {
                //nope, lets construct our bank record with a new balance
                myNPCaccount = EconomyScript.Instance.BankConfigData.CreateNewDefaultAccount(1234, "NPC", 0);

                //ok lets apply it
                EconomyScript.Instance.BankConfigData.Accounts.Add(myNPCaccount);
                //MessageClientTextMessage.SendMessage(SenderSteamId, "Banker", "Created");
                MyAPIGateway.Utilities.ShowMessage("Banker", "Created");
            } else { MyAPIGateway.Utilities.ShowMessage("Banker", "Ok"); }
        }
    }
}
