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
            string debug; //used if i need a random string for testing etc

            decimal bankbalance; //probably redundant may still use it for human readable reasons later
            decimal tran_amount=0; //how much are we trying to work with here?
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
                    if (split.Length >= 3)
                    { // did we at least type /pay someone something . . .
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
                        //wait was parameter 3 a number or garbage?
                        if (decimal.TryParse(split[2], out tran_amount)) {                    
                            //its a number
                            //* It needs to first check the player has enough to cover his payment
                            tran_amount = Convert.ToDecimal(split[2]);
                            bankbalance = Convert.ToDecimal(accounttospend.BankBalance);
                            if (tran_amount <= bankbalance || MyAPIGateway.Session.Player.IsAdmin()) // do we have enough or are we admin so it doesnt matter
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
                                    //is there a modify property to save the need to remove then re-add? 
                                    //here we remove this players bank record
                                    BankConfigData.Accounts.Remove(accounttospend);
                                    if (!MyAPIGateway.Session.Player.IsAdmin()) { tran_amount = Math.Abs(tran_amount); } //admins can give or take money, normal players can only give money so convert negative to positive
                                    //here we add the players bank record again with the updated balance minus what they spent
                                    accounttospend = new BankAccountStruct() { BankBalance = (bankbalance - tran_amount), Date = DateTime.Now, NickName = MyAPIGateway.Session.Player.DisplayName, SteamId = MyAPIGateway.Session.Player.SteamUserId }; 
                                    BankConfigData.Accounts.Add(accounttospend);

                                    //here we retrive the target player steam id and balance
                                    ulong theirID=account.SteamId;  decimal theirbank=(account.BankBalance+=tran_amount); string theirnick=account.NickName;
                                    //here we clean out the old data
                                    BankConfigData.Accounts.Remove(account);
                                    //here we build a new record with the correct data
                                    account = new BankAccountStruct() { BankBalance = theirbank, Date = DateTime.Now, NickName = theirnick, SteamId = theirID };
                                    //here we write it back to our bank ledger file
                                    BankConfigData.Accounts.Add(account);

                                    //if this works this is a very sexy way to work with our file
                                    //testing: it does indeed work, if i was a teenager id probably need to change my underwear at this point

                                    // now need to work out how to notify receiving player that they were paid and/or any message the sending player wrote
                                    // which needs to not send if the player isnt online - pity ive no idea how to write to the faction chat system
                                    // be a good place to send the player a faction message as it would work even if they were offline..
                                    reply = theirnick + ", " + MyAPIGateway.Session.Player.DisplayName + " just paid you " + tran_amount + " for ";                           
                                    //need to check the split[4] and upwards and display up to split.length here in reply
                                    MyAPIGateway.Utilities.ShowMessage("PAY", reply);
                                }
                                //*          else { throw error Unable to complete transaction }         
                                else { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry  can't find them in bank file!"); }

                                //*      if false/otherwise throw error you dont have enough money
                            } else { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry you can't afford that much!"); }
                            //*      eg /pay bob 50 here is your payment
                        
                        } else { MyAPIGateway.Utilities.ShowMessage("PAY", "Sorry invalid amount!"); } // i guess it wasn't a number
                        return true;
                    } else { MyAPIGateway.Utilities.ShowMessage("PAY", "Not enough parameters");  return true; }// i guess we didnt type enough params
                }

            } 

            //buy command
            if (messageText.StartsWith("/buy", StringComparison.InvariantCultureIgnoreCase))
            { MyAPIGateway.Utilities.ShowMessage("BUY", "Not yet implemented in this release"); return true; } 
            //sell command
            if (messageText.StartsWith("/sell", StringComparison.InvariantCultureIgnoreCase))
            { MyAPIGateway.Utilities.ShowMessage("SELL", "Not yet implemented in this release"); return true; }

            //seen command
            if (messageText.StartsWith("/seen", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)//did we just type seen? show error  
                {
                    MyAPIGateway.Utilities.ShowMessage("SEEN", "Who are we looking for? ");
                }
                else //look up that player and display time stamp
                {
                    var account = BankConfigData.Accounts.FirstOrDefault(
                            a => a.NickName.Equals(split[1], StringComparison.InvariantCultureIgnoreCase));

                    if (account == null)
                        reply = "Player not found";
                    else
                        reply = "Player " + account.NickName + " Last seen: " + account.Date;

                    MyAPIGateway.Utilities.ShowMessage("SEEN", reply);
                }
                //need to update our own timestamp here too!
                return true;
            } 

            //reset command - used by admins to reset their balance in the event they have overused
            //their /pay command on other players and now have a significantly wrong balance
            if (messageText.StartsWith("/reset", StringComparison.InvariantCultureIgnoreCase))
            {
                if (MyAPIGateway.Session.Player.IsAdmin()) // hold on there, are we an admin first?
                {   // we look up our bank record based on our Steam Id/
                    var myaccount = BankConfigData.Accounts.FirstOrDefault(
                        a => a.SteamId == MyAPIGateway.Session.Player.SteamUserId);
                    // wait do we even have an account yet? Cant remove whats not there!
                    if (myaccount != null) { BankConfigData.Accounts.Remove(myaccount); }
                    
                    //ok cause i am an admin and everything else checks out, lets construct our bank record with a new balance
                    myaccount = new BankAccountStruct() { BankBalance = 100, Date = DateTime.Now, NickName = MyAPIGateway.Session.Player.DisplayName, SteamId = MyAPIGateway.Session.Player.SteamUserId };
                    //ok lets apply it
                    BankConfigData.Accounts.Add(myaccount);
                    reply = " Done";
                    MyAPIGateway.Utilities.ShowMessage("Debug", reply);

                }
                return true;
            }

            //bal command
            if (messageText.StartsWith("/bal", StringComparison.InvariantCultureIgnoreCase))
            {
                //must update our own timestamp here too!
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
                        //MyAPIGateway.Utilities.ShowMessage("param:", split[1]);

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
                    //update our own timestamp here too!
                }
            }

            //help command
            if (messageText.StartsWith("/help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type help? show what else they can get help on
                    MyAPIGateway.Utilities.ShowMessage("help", "Commands: help, buy, sell, bal, pay, seen");
                    if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("admin", "Commands: accounts, bal player, reset, pay player +/-any_amount"); }
                    MyAPIGateway.Utilities.ShowMessage("help", "Try '/help command' for more informations about specific command debug 0");
                    return true;
                } else  {
                    switch (split[1].ToLowerInvariant())
                    {   // did we type /help help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/help #", "Displays help on the specified command [#]."); 
                            return true;
                        // did we type /help buy etc
                        case "pay":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/pay X Y Z Pays player [x] amount [Y] [for reason Z]");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /pay bob 100 being awesome");
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin", "Admins can add or remove any amount from a player"); }
                            return true;
                        case "seen":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/seen X Displays time and date that economy plugin last saw player X");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /seen bob");
                            return true;
                        case "accounts":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin", "/accounts displays all player balances"); return true; }
                            else { return false; }
                        case "reset":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin", "/reset resets your balance to 100"); return true; }
                            else { return false; }
                        case "bal":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/bal Displays bank balance");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /bal");
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin", "Admins can also view another player. eg. /bal bob"); }
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

