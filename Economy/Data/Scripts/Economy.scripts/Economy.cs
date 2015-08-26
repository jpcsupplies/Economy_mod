using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Definitions;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.Components;
using VRageMath;
using System.Text.RegularExpressions;

/*
 *  Economy Mod V(TBA) 
 *  by PhoenixX (JPC Dev), Tangentspy, Screaming Angels
 *  For use with Space Engineers Game
 *  Refer to github issues or steam dev guide or the below progress report
 *  for direction what needs to be worked on next
*/

namespace Economy
{
    [Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.AfterSimulation)]
    public class EconomyScript : MySessionComponentBase
    {
        bool initDone = false;
        int counter = 0;
        
        public void init()
        {
            MyAPIGateway.Utilities.MessageEntered += gotMessage;
            initDone = true;
            MyAPIGateway.Utilities.ShowMessage("Economy", "loaded!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/help' for more informations about available commands");
            MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");
        }
        public class load
        {
            //this is where we would normally read our file etc  simulated file read contents below
            public string bankdata = "1234567890,101,alias,timestamp\n9999999999,100,alias2,timestamp\n";
            // Ideally this data should be persistent until someone buys/sells/pays/joins but
            //lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        }

        /*
        public class Bank 
         //not sure if this should be static in a multiplayer context - looks to be double handling anyway - disabled 
         //in a perfect world this is how we maintain the bank ledger - but the values dont appear persistent
         //what a waste 
        {
            #region  bal table
            // This class is mutable. Its data can be modified from
            // outside the class.

                // Auto-Impl Properties for trivial get and set
                public double funds { get; set; }
                public string Name { get; set; }
                public string PlayerID { get; set; }
                public string timedate { get; set; }

                // Constructor
                public Bank(string UID, double balance, string name, string timestamp)
                {
                    funds = balance;
                    Name = name;
                    PlayerID = UID;
                    timedate = timestamp;
                }
                // Methods
                //public string GetContactInfo() { return "ContactInfo"; }
                //public string GetTransactionHistory() { return "History"; }
 
           #endregion 
        } */

  
        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= gotMessage;
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            //presumably we are sleeping 100 so we are on the screen after the player sees the screen or as an interval?
            //if I cannot find an on join event might have to maintain a player list this way
            if (!initDone && MyAPIGateway.Session != null) init(); if (counter >= 100) { counter = 0; UpdateAfterSimulation100(); }
            counter++; base.UpdateAfterSimulation();
        }

        private static void gotMessage(string messageText, ref bool sendToOthers)
        { 
            // here is where we nail the echo back on commands "return" also exits us from processMessage
            if (processMessage(messageText)) { sendToOthers = false; }
        }

        private static bool processMessage(string messageText)
        {
            string reply; //used when i need to assemble bits for output to screen
            string steamid; 
            double bankbalance; //probably redundant but i still use it for human readable reasons
            //string alias; //represents players current in game nickname
            //string timestamp; //will be used for seen command later maybe
            int records; //number of record lines in bank file
            int count; //counter for iterating over records

            #region command list

            //this list is going to get messy since the help and commands themself tell user the same thing 
            
            string[] split = messageText.Split(new Char[] { ' ' });
            //pay command
            if (messageText.StartsWith("/pay", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type pay? show help
                    MyAPIGateway.Utilities.ShowMessage("PAY", "refer help for using pay command");
                    return true;
                }
                else //we type more than 1 parm? 
                {
                    MyAPIGateway.Utilities.ShowMessage("PAY", "Had We made that part yet, we would be trying to pay someone here");
                    return true;
                }

            } 

            //buy command
            if (messageText.StartsWith("/buy", StringComparison.InvariantCultureIgnoreCase))
            {
                    MyAPIGateway.Utilities.ShowMessage("BUY", "Not yet implemented in this release");
                    return true;
            } 
            //sell command
            if (messageText.StartsWith("/sell", StringComparison.InvariantCultureIgnoreCase))
            {
                    MyAPIGateway.Utilities.ShowMessage("SELL", "Not yet implemented in this release");
                    return true;
            } 

            //bal command
            if (messageText.StartsWith("/bal", StringComparison.InvariantCultureIgnoreCase))
            {
                load bank = new load(); //lets grab the current data from our bankfile ready for next step
                //ideally the entire "lines array" should already exist under bank too, to cut down
                //double handling, but had trouble - need an expert to redo that section
                string[] lines = bank.bankdata.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                /*
                 * Now we have
                 * lines[0] = 234567890,101,alias,timestamp
                 * lines[1] = 9999999999,100,alias2,timestamp
                 * <blank>
                 */
                records = lines.Length;  //how many balance records?
                MyAPIGateway.Utilities.ShowMessage("debug ", records.ToString() );

                //will need to do this anywhere we need to check iterating on [0] and splitting every time
                // now we have ledger[0]=234567890 ledger[1]=101 ledger[2]=alias ledger[3]=timestamp
                string[] ledger = lines[0].Split(new Char[] { ',' });

                if (split.Length <= 1)//did we just type bal? show our balance  
                {
                                  
                    //for sake of testing let us assume we just looked up our steam ID
                    steamid = "9999999999"; //this should be replaced with a Steam UID lookup for current user

                    //steamid = ledger[0]; bankbalance = Convert.ToDouble(ledger[1]); alias = ledger[2]; timestamp = ledger[3];

                    //OCD could use a "while" here instead; but this seems to work fine
                    //really need to move this to its own sub since it will be called from several places
                    for (count = 0; count < records; count++) //search for our id
                    {
                        MyAPIGateway.Utilities.ShowMessage("debug ", count.ToString());
                        if (ledger[0] == steamid) { break; }
                        ledger = lines[count].Split(new Char[] { ',' });
                    }
                    if (ledger[0] != steamid) //check if we actually found it, add default if not
                    {
                        ledger[0] = steamid; ledger[1] = "100"; ledger[2] = "your current nickname"; ledger[3] = "timestamp";
                        Array.Resize(ref lines, records + 1);
                        lines[records + 1] = ledger[0]+","+ledger[1]+","+ledger[2]+","+ledger[3]+"\n";
                        // Trigger a save here to record our new record!!!
                    }
                    bankbalance = Convert.ToDouble(ledger[1]);

                    /* I just realized unless I can make Bank class persistent this bit is totally unnecessary
                    // Intialize a new object.
                    Bank client = new Bank(steamid, bankbalance, alias, timestamp);
                    MyAPIGateway.Utilities.ShowMessage("debug", client.funds.ToString("0.######"));
                    //Modify a property
                    client.funds += 499.99;
                    MyAPIGateway.Utilities.ShowMessage("debug", client.funds.ToString("0.######"));
                    */
                    
                    reply = "Your bank balance is " + bankbalance;
                    MyAPIGateway.Utilities.ShowMessage("BALANCE", reply);
                    return true;
                }
                else //we type more than 1 parm? must want to know someone elses balance
                {
                    count = 0; //not sure if this fixed out of array bound error
                    // need to do more analysis, may need a failsafe undef check
                    for (count = 0; count < records; count++) //search for nickname
                    {
                        MyAPIGateway.Utilities.ShowMessage("debug ", count.ToString());
                        if (ledger[2].ToLower() == split[1].ToLower()) { break; }
                        ledger = lines[count].Split(new Char[] { ',' });
                    }
                    if (ledger[2].ToLower() != split[1].ToLower()) //check if we actually found it
                    {
                         ledger[1] = "0"; ledger[2] = split[1]+ " not found"; 
                    }
                    bankbalance = Convert.ToDouble(ledger[1]);
                    reply = "Player " + ledger[2] + " Balance: " + bankbalance;
                    MyAPIGateway.Utilities.ShowMessage("BALANCE", reply);
                    MyAPIGateway.Utilities.ShowMessage("param:", split[1]);

                    return true;
                }
               
            } 

            //help command
            if (messageText.StartsWith("/help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type help? show what else they can get help on
                    MyAPIGateway.Utilities.ShowMessage("help", "Commands: help, buy, sell, pay");
                    MyAPIGateway.Utilities.ShowMessage("help", "Try '/help command' for more informations about specific command debug 0");
                    return true;
                } else  {
                    switch (split[1].ToLowerInvariant())
                    {   // did we type /help help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/help #", "Displays help on the specified command [#]. debug 1"); 
                            return true;
                        // did we type /help buy etc
                        case "pay":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/pay X Y Z Pays player [x] amount [Y] [for reason Z]");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /pay bob 100 being awesome");
                            return true;
                        case "buy":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/buy W X Y Z - Purchases a quantity [W] of item [X] [at price Y] [from player Z]");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /buy 20 Ice ");
                            return true;
                        case "sell":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/sell W X Y Z - Sells a quantity [W] of item [X] [at price Y] [to player Z]");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /sell 20 Ice ");
                            return true;
                    } 
                }
                
            } 
            //it didnt start with help or anything else that matters so return false and get us out of here;
            return false;
            #endregion     
        }

        public void UpdateAfterSimulation100() { }

    }
}

