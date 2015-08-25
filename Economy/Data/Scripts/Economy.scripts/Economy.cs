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
 * These can presumably be adapted to catch joins and steam uids
 * MyAPIGateway.Session.OnSessionReady += Session_OnSessionReady;
 * SteamId = selectedPlayer.SteamUserId
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
            //string progress;
            MyAPIGateway.Utilities.MessageEntered += gotMessage;
            initDone = true;
            MyAPIGateway.Utilities.ShowMessage("Economy", "loaded!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/help' for more informations about available commands");

            MyAPIGateway.Utilities.ShowMissionScreen("Economy Mod", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");
            #region Progress Report
            /*Be nice if i could get this to display in my popup
            Progress report: \n
             * Abstract First Stage Started\n
             * Have script responding to /bal /help and /pay
             * cant get file operations to work after trying different stuff all day
             * like really how hard can it be to load a text file into an array or write it back..
             * have some notes on pulling up a player UID, need to figure it out more later
                Development target progress\n
                Achieved\n
                    1: Create a placeholder script that loads off workshop, assign contributors\n
                Pending\n
                    2: Configure script to read a config file containing things like starting balance, server bank balance, currency name, server trading company name, delivery method, delivery speed etc - although they will be placeholders at this stage they should read into memory\n
                    3: Configure script to log players into a file with a starting balance\n -[nothing works so far, its either f'kin not allowed, or doesnt work]
                    4: Add a command to permit players to examine their balance\n -[Command added but not functional]
                Future\n
                    5: Add a command for admins to set players balances\n
                    6: Add a command to allow players to transfer their balance to other players\n
                    7: Add a database of all ores and resources (and blocks?) and assign an arbitrary default value for each ore\n
                    8: Add a command to read the price of a particular resource from this data base\n
                    9: Configure script to create a file with the "server pool" balance of each resource type.\n
                    10: Configure some sort of file to record items particular players have for sale, what quantity, and what price - universal auction market?\n
                       */
            #endregion
        }

        /* Well this is not right have to try another method
         public MyWriter(string logFile)
         {
             m_writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(logFile, typeof(MyWriter));
         }
        */
        // this didnt work either. i seem to be missing something here
        // public void WriteLine(string writethis)
        // {
        //     MyAPIGateway.Utilities.ShowMessage("Trained Monkey:", writethis);
        /* m_writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("bank.txt", typeof(writethis));
        if (text.Length > 0)
            m_writer.WriteLine(writethis);
        text.Clear();
       // m_writer.WriteLine(m_cache.Append(writethis));
        m_writer.Flush();
        m_writer.Close();
        //return; */

        // }


        //togame = string.Format(Bank,Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        //MyAPIGateway.Utilities.ShowMessage("path", togame);
        //ok this gives me the full game save folder MyAPIGateway.Session.CurrentPath
        //this gives me just the save folder name alone Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath);
        //this gives me bank with save folder name appended string.Format("bank{0}.txt",Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));


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
            if (processMessage(messageText)) { sendToOthers = false; }
        }
/*        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            if (processMessage(messageText)) { sendToOthers = false; }
        }
*/
        private static bool processMessage(string messageText)
        {
            #region command list
            decimal bal = 100;
            string reply;
            //this list is going to get messy since the help and commands themself tell user the same thing if they dont type parms right.
            //!BUGS! doesnt supress what player types in chat 
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
                else //we type more than 1 parm? must want to do stuff
                //will need to drag out the contents of each element of the split eg /pay | bob | 10  and check it is valid
                //then check the player has enough balance, check the name is in the bank database, then update it 
                {
                    MyAPIGateway.Utilities.ShowMessage("PAY", "Had We made that part yet, we would be trying to pay someone here");
                    return true;
                }
                //return false;
            }



            //bal command
            if (messageText.StartsWith("/bal", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type bal? show balance
                    reply = "Had We made that part yet you would see your bank balance now " + bal.ToString();
                    MyAPIGateway.Utilities.ShowMessage("BALANCE", reply);
                    return true;
                }
                else //we type more than 1 parm? must want to know someone elses balance
                {
                    MyAPIGateway.Utilities.ShowMessage("BALANCE", "Had We made that part yet, we check you are admin then tell you another players balance");
                    // WriteLine("Had my trained monkey been better schooled this would be getting saved to a file");
                    MyAPIGateway.Utilities.ShowMessage("Trained Monkey says", split[1].ToLowerInvariant());
                    return true;
                }
                //return false;
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
                }
                else
                {
                    switch (split[1].ToLowerInvariant())
                    {
                        // did we type /help help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/help #", "Displays help on the specified command [#]. debug 1"); return true;
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

                            if (split.Length > 2)
                            {
                                //if we asked for help 3 levels deep eg /help buy list dont need atm
                                switch (split[2].ToLowerInvariant())
                                {
                                    case "list":
                                        MyAPIGateway.Utilities.ShowMessage("/help buy list", "test");
                                        MyAPIGateway.Utilities.ShowMessage("/help buy list", "Example:");
                                        return true;
                                }
                                //return false;
                            }
                            else { MyAPIGateway.Utilities.ShowMessage("/help test #", "Commands: list debug 2"); return true; }
                    }
                }
                //return false;
            }
            #endregion

            //return false;
        }

        public void UpdateAfterSimulation100() { }

    }
}

