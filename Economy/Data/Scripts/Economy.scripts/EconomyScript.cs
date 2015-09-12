/*
 *  Economy Mod V(TBA) 
 *  by PhoenixX (JPC Dev), Tangentspy, Screaming Angels
 *  For use with Space Engineers Game
 *  Refer to github issues or steam/git dev guide/wiki or the team notes
 *  for direction what needs to be worked on next
*/

namespace Economy.scripts
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using Economy.scripts.Config;
    using System.Globalization;
    using Economy.scripts.Messages;

    [Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.AfterSimulation)]
    public class EconomyScript : MySessionComponentBase
    {
        #region constants

        const string PayPattern = @"(?<command>/pay)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))\s+(?<value>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<reason>.+)\s*$";
        const string BalPattern = @"(?<command>/bal)(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?";
        const string SeenPattern = @"(?<command>/seen)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))";

        #endregion

        #region fields

        private bool _isInitialized;
        private bool _isClientRegistered;
        private bool _isServerRegistered;

        private readonly Action<byte[]> _messageHandler = new Action<byte[]>(HandleMessage);

        public static EconomyScript Instance;

        public TextLogger ServerLogger = new TextLogger();
        public TextLogger ClientLogger = new TextLogger();

        /// Ideally this data should be persistent until someone buys/sells/pays/joins but
        /// lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        public BankConfig BankConfigData;

        #endregion

        #region attaching events and wiring up

        public override void UpdateAfterSimulation()
        {
            Instance = this;

            // This needs to wait until the MyAPIGateway.Session.Player is created, as running on a Dedicated server can cause issues.
            // It would be nicer to just read a property that indicates this is a dedicated server, and simply return.
            if (!_isInitialized && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
            {
                if (MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE)) // pretend single player instance is also server.
                    InitServer();
                if (!MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE) && MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated)
                    InitServer();
                InitClient();
            }

            // Dedicated Server.
            if (!_isInitialized && MyAPIGateway.Utilities != null && MyAPIGateway.Multiplayer != null
                && MyAPIGateway.Session != null && MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
            {
                InitServer();
                return;
            }

            base.UpdateAfterSimulation();
        }

        private void InitClient()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateAfterSimulation().
            _isClientRegistered = true;

            ClientLogger.Init("EconClient.Log"); // comment this out if logging is not required for the Client.
            ClientLogger.Write("Starting Client");

            MyAPIGateway.Utilities.MessageEntered += GotMessage;

            if (MyAPIGateway.Multiplayer.MultiplayerActive && !_isServerRegistered) // if not the server, also need to register the messagehandler.
            {
                ClientLogger.Write("RegisterMessageHandler");
                MyAPIGateway.Multiplayer.RegisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);
            }

            MyAPIGateway.Utilities.ShowMessage("Economy", "loaded!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/help' for more informations about available commands");
            MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");

            // let the server know we are ready for connections
            ConnectionHelper.SendMessageToServer(new MessageConnectionRequest());
        }

        private void InitServer()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateAfterSimulation().
            _isServerRegistered = true;
            ServerLogger.Init("EconServer.Log", true); // comment this out if logging is not required for the Server.
            ServerLogger.Write("Starting Server");

            ServerLogger.Write("RegisterMessageHandler");
            MyAPIGateway.Multiplayer.RegisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);

            ServerLogger.Write("LoadBankContent");
            BankConfigData = BankManagement.LoadContent();
        }

        #endregion

        #region detaching events

        protected override void UnloadData()
        {
            if (_isClientRegistered)
            {
                if (MyAPIGateway.Utilities != null)
                {
                    MyAPIGateway.Utilities.MessageEntered -= GotMessage;
                }

                if (!_isServerRegistered) // if not the server, also need to unregister the messagehandler.
                {
                    ClientLogger.Write("UnregisterMessageHandler");
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);
                }

                ClientLogger.Write("Closed");
                ClientLogger.Terminate();
            }

            if (_isServerRegistered)
            {
                ServerLogger.Write("UnregisterMessageHandler");
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(ConnectionHelper.ConnectionId, _messageHandler);

                if (BankConfigData != null)
                {
                    BankManagement.SaveContent(BankConfigData);
                    BankConfigData = null;
                    ServerLogger.Write("SaveBankContent");
                }

                ServerLogger.Write("Closed");
                ServerLogger.Terminate();
            }

            base.UnloadData();
        }

        #endregion

        #region messge handling

        private void GotMessage(string messageText, ref bool sendToOthers)
        {
            // here is where we nail the echo back on commands "return" also exits us from processMessage
            if (ProcessMessage(messageText)) { sendToOthers = false; }
        }

        private static void HandleMessage(byte[] message)
        {
            EconomyScript.Instance.ServerLogger.Write("HandleMessage");
            EconomyScript.Instance.ClientLogger.Write("HandleMessage");
            ConnectionHelper.ProcessData(message);
        }

        #endregion

        private bool ProcessMessage(string messageText)
        {
            Match match; // used by the Regular Expression to test user input.

            #region command list

            // this list is going to get messy since the help and commands themself tell user the same thing 

            string[] split = messageText.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // nothing useful was entered.
            if (split.Length == 0)
                return false;

            // pay command
            // eg /pay bob 50 here is your payment
            // eg /pay "Screaming Angles" 10 fish and chips
            if (split[0].Equals("/pay", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, PayPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessagePayUser.SendMessage(match.Groups["user"].Value,
                        Convert.ToDecimal(match.Groups["value"].Value, CultureInfo.InvariantCulture),
                        match.Groups["reason"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("PAY", "Not enough parameters - /pay user amount reason");
                return true;
            }

            // buy command
            if (split[0].Equals("/buy", StringComparison.InvariantCultureIgnoreCase))
            {
                MyAPIGateway.Utilities.ShowMessage("BUY", "Not yet implemented in this release");
                return true;
            }

            // sell command
            if (split[0].Equals("/sell", StringComparison.InvariantCultureIgnoreCase))
            {
                MyAPIGateway.Utilities.ShowMessage("SELL", "Not yet implemented in this release");
                return true;
            }

            // seen command
            if (split[0].Equals("/seen", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, SeenPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessagePlayerSeen.SendMessage(match.Groups["user"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("SEEN", "Who are we looking for?");
                return true;
            }

            // bal command
            if (split[0].Equals("/bal", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, BalPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessageBankBalance.SendMessage(match.Groups["user"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("BAL", "Incorrect parameters");
                return true;
            }

            // accounts command.  For Admins only.
            if (split[0].Equals("/accounts", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                MessageListAccounts.SendMessage();
                return true;
                // don't respond to non-admins.
            }

            // reset command.  For Admins only.
            if (split[0].Equals("/reset", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                MessageResetAccount.SendMessage();
                return true;
                // don't respond to non-admins.
            }

            // help command
            if (split[0].Equals("/help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type help? show what else they can get help on
                    MyAPIGateway.Utilities.ShowMessage("help", "Commands: help, buy, sell, bal, pay, seen");
                    if (MyAPIGateway.Session.Player.IsAdmin())
                    {
                        MyAPIGateway.Utilities.ShowMessage("admin", "Commands: accounts, bal player, reset, pay player +/-any_amount");
                    }
                    MyAPIGateway.Utilities.ShowMessage("help", "Try '/help command' for more informations about specific command");
                    return true;
                }
                else
                {
                    switch (split[1].ToLowerInvariant())
                    {   
                        // did we type /help help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/help #", "Displays help on the specified command [#]."); 
                            return true;
                        // did we type /help buy etc
                        case "pay":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/pay X Y Z Pays player [x] amount [Y] [for reason Z]");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /pay bob 100 being awesome");
                            MyAPIGateway.Utilities.ShowMessage("Help", "for larger player names used quotes eg \"bob the builder\"");
                            if (MyAPIGateway.Session.Player.IsAdmin())
                            {
                                MyAPIGateway.Utilities.ShowMessage("Admin", "Admins can add or remove any amount from a player");
                            }
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
                            MyAPIGateway.Utilities.ShowMessage("Help", "/bal Displays your bank balance");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /bal");
                            if (MyAPIGateway.Session.Player.IsAdmin())
                            {
                                MyAPIGateway.Utilities.ShowMessage("Admin", "Admins can also view another player. eg. /bal bob");
                            }
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

            #endregion

            // it didnt start with help or anything else that matters so return false and get us out of here;
            return false;
        }
    }
}