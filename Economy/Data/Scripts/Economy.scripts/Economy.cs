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
 *  Refer to github issues or steam/git dev guide/wiki or the team notes
 *  for direction what needs to be worked on next
*/

namespace Economy
{
    [Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.AfterSimulation)]
    public class EconomyScript : MySessionComponentBase
    {
        bool initDone = false;
        int counter = 0;

        /// Ideally this data should be persistent until someone buys/sells/pays/joins but
        /// lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        public static BankConfig BankConfigData;
        
        public void init()
        {
            MyAPIGateway.Utilities.MessageEntered += gotMessage;
            initDone = true;
            BankConfigData = BankManagement.LoadContent();

            MyAPIGateway.Utilities.ShowMessage("Economy", "loaded!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/help' for more informations about available commands");
            MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");
        }

 
        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= gotMessage;
            base.UnloadData();

            if (BankConfigData != null)
            {
                BankManagement.SaveContent(BankConfigData);
                BankConfigData = null;
            }
        }

        public override void UpdateAfterSimulation()
        {
            //presumably we are sleeping 100 so we are on the screen after the player sees the screen or as an interval?
            //if I cannot find an on join event might have to maintain a player list this way
            if (!initDone && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                init();
            if (counter >= 100) { counter = 0; UpdateAfterSimulation100(); }
            counter++;
            base.UpdateAfterSimulation();
        }

        private static void gotMessage(string messageText, ref bool sendToOthers)
        { 
            // here is where we nail the echo back on commands "return" also exits us from processMessage
            if (processMessage(messageText)) { sendToOthers = false; }
        }

        private static bool processMessage(string messageText)
        {
            string reply; //used when i need to assemble bits for output to screen
            bool hasaccount; //flag used when transferring funds eg /pay bob 10 - indicates if bob even has an account record yet
            
            double bankbalance; //probably redundant may still use it for human readable reasons later
            double tran_amount; //how much are we trying to work with here?
            //string alias; //represents players current in game nickname
            //string timestamp; //will be used for seen command later maybe
            //int records; //number of record lines in bank file replaced by "BankConfigData.Accounts.Count.ToString()"
            //int count; //counter for iterating over records no longer used

            #region command list

            //this list is going to get messy since the help and commands themself tell user the same thing 
            
            string[] split = messageText.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                    if (split.Length >= 2)
                    { // did we at least type /pay someone something . . .
                        /*client.funds += 499.99; or -=  depending which side of transaction we are..
                        will need repeat of the "create default bal if doesnt exist" here.. or trigger the existing bal
                        command to save double handling - probably do that once we know it works first*/
                        //* Logic:                     
                        //* Get player steam ID
                        var accounttospend = BankConfigData.Accounts.FirstOrDefault(
                             a => a.SteamId == MyAPIGateway.Session.Player.SteamUserId);

                        //* Load the relevant bank balance data - check player even has an account yet; make one if not
                        if (accounttospend == null)
                        {
                            accounttospend = new BankAccountStruct() { BankBalance = 100, Date = DateTime.Now, NickName = MyAPIGateway.Session.Player.DisplayName, SteamId = MyAPIGateway.Session.Player.SteamUserId };
                            BankConfigData.Accounts.Add(accounttospend);
                        }
                        //* It needs to first check the player has enough to cover his payment
                        tran_amount = Convert.ToDouble(split[2]);
                        bankbalance = Convert.ToDouble(accounttospend.BankBalance);
                        if (tran_amount <= bankbalance)
                        //*      if true, 
                        {
                            //*          it needs to check the person being paid has an account record, 
                            var account = BankConfigData.Accounts.FirstOrDefault(
                                a => a.NickName.Equals(split[1], StringComparison.InvariantCultureIgnoreCase));
                            //*               if false, 
                            //*                  it needs to check if the other player is even online
                            //*                     if true
                            //*                         create one with default balance.
                            //*                         flag hasaccount bool true
                            //*                     if false
                            //*                         display an error message player not found
                            //*                         flag hasaccount bool false
                            if (account == null)
                            { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry player not found!"); hasaccount = false; }
                            //*               if true, { flag hasaccount bool true }
                            else
                            { hasaccount = true; }
                            //*          if hasaccount bool true   
                            if (hasaccount)
                            {
                                MyAPIGateway.Utilities.ShowMessage("PAY", "Yes we could pay them here debug");
                                //*               add payment amount to person being paid's balance
                                //*               deduct payment amount from person making payment
                                //*               force a save so we dont loose money on server crash (if possible)
                                //*               notify receiving player that they were paid and/or any message the sending player wrote
                            }
                            //*          else { throw error Unable to complete transaction }         
                            else { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry  can't find them in bank file!"); }

                            //*      if false/otherwise throw error you dont have enough money
                        }
                        else { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry you can't afford that much!"); }
                        //*      eg /pay bob 50 here is your payment
                        reply = "I HAVE " + bankbalance + ". Pay " + split[1] + " " + tran_amount;
                        MyAPIGateway.Utilities.ShowMessage("PAY", reply);
                        return true;
                    }  // i guess we didnt type enough params
                    else { MyAPIGateway.Utilities.ShowMessage("PAY", "Not enough parameters");  return true; }
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
                //how many balance records?
                MyAPIGateway.Utilities.ShowMessage("debug ", BankConfigData.Accounts.Count.ToString());

                if (split.Length <= 1)//did we just type bal? show our balance  
                {
                    // lets grab the current player data from our bankfile ready for next step
                    // we look up our Steam Id/
                    var account = BankConfigData.Accounts.FirstOrDefault(
                        a => a.SteamId == MyAPIGateway.Session.Player.SteamUserId);

                    // check if we actually found it, add default if not
                    if (account == null)
                    {
                        account = new BankAccountStruct() { BankBalance = 100, Date = DateTime.Now, NickName = MyAPIGateway.Session.Player.DisplayName, SteamId = MyAPIGateway.Session.Player.SteamUserId };
                        BankConfigData.Accounts.Add(account);
                    }


                    reply = "Your bank balance is " + account.BankBalance.ToString("0.######");
                    MyAPIGateway.Utilities.ShowMessage("BALANCE", reply);
                    return true;
                }
                else //we type more than 1 parm? must want to know someone elses balance
                {
                    if (MyAPIGateway.Session.Player.IsAdmin()) // hold on there, are we an admin first?
                    {
                        var account = BankConfigData.Accounts.FirstOrDefault(
                            a => a.NickName.Equals(split[1], StringComparison.InvariantCultureIgnoreCase));

                        if (account == null)
                            reply = "Player not found Balance: 0";
                        else
                            reply = "Player " + account.NickName + " Balance: " + account.BankBalance;

                        MyAPIGateway.Utilities.ShowMessage("BALANCE", reply);
                        MyAPIGateway.Utilities.ShowMessage("param:", split[1]);

                        return true;
                    }
                    else { return true; } // not an admin? lets pretend this never happened
                }
               
            }

            // test stub to list all accounts.
            if (messageText.StartsWith("/accounts", StringComparison.InvariantCultureIgnoreCase))
            {
                if (MyAPIGateway.Session.Player.IsAdmin())
                {
                    var description = new StringBuilder();
                    var prefix = string.Format("Count: {0}", BankConfigData.Accounts.Count);
                    var index = 1;
                    foreach (var account in BankConfigData.Accounts.OrderBy(s => s.NickName))
                    {
                        description.AppendFormat("#{0}: {1} : {2}\r\n", index++, account.NickName, account.BankBalance);
                    }

                    MyAPIGateway.Utilities.ShowMissionScreen("List Accounts", prefix, " ", description.ToString());
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
                        case "bal":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/bal Displays bank balance");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /bal");
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

