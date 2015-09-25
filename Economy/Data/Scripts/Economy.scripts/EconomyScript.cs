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
    using System.Timers;
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
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;
    using System.Text.RegularExpressions;
    using Economy.scripts.EconConfig;
    using System.Globalization;
    using Economy.scripts.Messages;
    using VRage;

    [Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.AfterSimulation)]
    public class EconomyScript : MySessionComponentBase
    {
        #region constants

        const string PayPattern = @"(?<command>/pay)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))\s+(?<value>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<reason>.+)\s*$";
        const string BalPattern = @"(?<command>/bal)(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?";
        const string SeenPattern = @"(?<command>/seen)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))";
        const string ValuePattern = @"(?<command>/value)\s+(?:(?<Key>.+)\s+(?<Value>[+-]?((\d+(\.\d*)?)|(\.\d+)))|(?<Key>.+))";

        /// <summary>
        /// sell pattern check for "/sell" at[0], then number or string at [1], then string at [2], then number at [3], then string at [4]
        /// samples(optional): /sell all iron (price) (player/faction) || /sell 10 iron (price) (player/faction) || /sell accept || /sell deny || /sell cancel
        /// </summary>
        const string SellPattern = @"(?<command>/sell)\s+(?<qty>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?:(?:""(?<item>[^""]|.*?)"")|(?<item>[^\s]*))(?:\s+(?<price>[+-]?((\d+(\.\d*)?)|(\.\d+)))(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?)?";

        #endregion

        #region fields

        private bool _isInitialized;
        private bool _isClientRegistered;
        private bool _isServerRegistered;
        private bool _delayedConnectionRequest;

        private readonly Action<byte[]> _messageHandler = new Action<byte[]>(HandleMessage);

        public static EconomyScript Instance;

        public TextLogger ServerLogger = new TextLogger();
        public TextLogger ClientLogger = new TextLogger();
        public Timer DelayedConnectionRequestTimer;

        /// Ideally this data should be persistent until someone buys/sells/pays/joins but
        /// lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        public BankConfig BankConfigData;
        public MarketConfig MarketConfigData;

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

            if (_delayedConnectionRequest)
            {
                ClientLogger.Write("Delayed Connection Request");
                _delayedConnectionRequest = false;
                MessageConnectionRequest.SendMessage(EconomyConsts.ModCommunicationVersion);
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
                MyAPIGateway.Multiplayer.RegisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);
            }

            MyAPIGateway.Utilities.ShowMessage("Economy", "loaded!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/ehelp' for more informations about available commands");
            //MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");

            DelayedConnectionRequestTimer = new Timer(10000);
            DelayedConnectionRequestTimer.Elapsed += DelayedConnectionRequestTimer_Elapsed;
            DelayedConnectionRequestTimer.Start();

            // let the server know we are ready for connections
            MessageConnectionRequest.SendMessage(EconomyConsts.ModCommunicationVersion);
        }

        private void InitServer()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateAfterSimulation().
            _isServerRegistered = true;
            ServerLogger.Init("EconServer.Log", true); // comment this out if logging is not required for the Server.
            ServerLogger.Write("Starting Server");

            ServerLogger.Write("RegisterMessageHandler");
            MyAPIGateway.Multiplayer.RegisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);

            ServerLogger.Write("LoadBankContent");
            BankConfigData = BankManagement.LoadContent();
            MarketConfigData = MarketManagement.LoadContent();

            //Buy/Sell - check we have our NPC banker ready
            NpcMerchantManager.VerifyAndCreate();
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
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);
                }

                if (DelayedConnectionRequestTimer != null)
                {
                    DelayedConnectionRequestTimer.Stop();
                    DelayedConnectionRequestTimer.Close();
                }

                ClientLogger.Write("Closed");
                ClientLogger.Terminate();
            }

            if (_isServerRegistered)
            {
                ServerLogger.Write("UnregisterMessageHandler");
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);

                BankConfigData = null;
                MarketConfigData = null;

                ServerLogger.Write("Closed");
                ServerLogger.Terminate();
            }

            base.UnloadData();
        }

        public override void SaveData()
        {
            base.SaveData();

            if (_isServerRegistered)
            {
                if (BankConfigData != null)
                {
                    BankManagement.SaveContent(BankConfigData);
                    ServerLogger.Write("SaveBankContent");
                }

                if (MarketConfigData != null)
                {
                    MarketManagement.SaveContent(MarketConfigData);
                    ServerLogger.Write("SaveMarketContent");
                }
            }
        }

        #endregion

        #region message handling

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

        void DelayedConnectionRequestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _delayedConnectionRequest = true;
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
            {   //might need to add a check here preventing normal players paying NPC but not critical since they would be silly to try
                match = Regex.Match(messageText, PayPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessagePayUser.SendMessage(match.Groups["user"].Value,
                        Convert.ToDecimal(match.Groups["value"].Value, CultureInfo.InvariantCulture),
                        match.Groups["reason"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("PAY", "Missing parameter - /pay user amount reason");
                return true;
            }

            // buy command
            if (split[0].Equals("/buy", StringComparison.InvariantCultureIgnoreCase))
            {
                //initially we should work only with a players inventory size
                //worst case scenario overflow gets dropped at the players feet which if they
                //are standing on a collector should be adequte for Alpha milestone compliance.
                //Alternately we setup an "item bank" which works like the bank account, but allows 
                //materials to be stored too.. but that makes raids somewhat pointless.
                //buy command should be pretty basic since we are buying from appropriate market at requested price.
                //if in range of valid trade region
                //examples: /buy 100 uranium (buy 100 uranium starting at lowest offer price if you can afford it, give error if you cant)
                // /buy 100 uranium 0.5 (buy up to 100 uranium if the price is below 0.5, post a buy offer for any remaining amount if price goes over)
                // /buy 100 uranium 0.5 bob (ask bob to sell you 100 uranium at 0.5, or if bob already has a sell offer at or below 0.5, buy only from him)
                // /buy cancel ??? (possible command to close an open buy offer?)
                //
                //  if (no price specified) buy whatever the lowest offer is, accumulating qty, and adding up each price point qty until
                //  sale is concluded at desired qty, or player runs out of money, or there are none left to buy.
                //  if (they place an offer above market && have access to offers), post offer, and deduct funds
                //  if they choose to cancel an offer, remove offer and return remaining money

                MyAPIGateway.Utilities.ShowMessage("BUY", "Not yet implemented in this release");
                return true;
            }

            // sell command
            if (split[0].Equals("/sell", StringComparison.InvariantCultureIgnoreCase))
            {
                #region sell notes
                //initially we should work only with a players inventory contents
                //later on working with a special block or faction only storage crate setup may work
                //when they need to sell more than can be carried
                //another out of the box idea is trade when you are in a cockpit
                //then the mod works with the materials stored in that ship -API capabilites dependant however
                //
                //[0] = /sell  [1] = quantity [2] = keyword representing material to sell [3] = optional price (for posting a sell offer instead of selling to NPC) [4] optional player name for making offer to player
                //special keywords "all"  /sell all = sell their entire inventory || /sell all iron = sell all their iron ore only
                //examples  /sell 100 uranium  (sells 100 uranium to NPC at price table price - increases the qty in table, so long as npc can afford)
                //          /sell 100 uranium 5 (puts up a sell offer of 100 uranium at unit price of 5 each - would write to global market table(s))
                //          /sell 100 uranium 0.5 bob (offers to sell bob 100 uranium at 0.5 each - bob must /accept or /decline or it times out
                // Modifiers -  allow provision for multiple markets for faction to faction/ player to faction / player to player markets later
                //              may need restrictions by location or faction eventually as well.  Example - selling to NPC may only be allowed
                //              if player is located within 5km of 0,0,0 on map (which in ramblers would be the nearest blue mining trade base)
                //              Example 2 - selling to "BOB" faction is only allowed if you are allied (or neutral?) with them and within 5km of their registered
                //              trade base location.  So we may eventually need a "register" type command for faction 2 faction trade.
                // if player location is within range of valid trade territory (or 0,0,0 to begin with for this milestone) 
                // { 
                //      select case (does split contain data?) (actually my logic here is terrible, but its just comments ;)
                //          case [1] == "cancel" cancel specified order?
                //          case ![1] display brief help summary (means they only typed /sell)
                //          case ![2] check if [1] == "all" - display error if not (means they only typed /sell all, or some invalid option) 
                //              if (it is "ALL" - and [2] isnt "cancel") parse contents of inventory and calculate value, compare against NPC bank balance; transfer qty to bank and value to player if NPC can afford it
                //              else if [2] is cancel - remove all sell offers posted by this player, return items to inventory or spawn at feet if overflow.
                //          case ![3] lookup item specified at [2] if valid calculate value of goods at given qty (or all) and compare against NPC bank balance, transfer qty to item file and value to player if NPC can afford it;  up to what the npc can afford.
                //          case ![4] lookup item specified at [2] - if valid and in inventory, check if player is allowed to post offers here, if so add [1] items [2] to global market at price [3] deduct items from player
                //          case ![5] (optional check distance to player isnt too high) lookup item specified at [2] - if valid and in inventory && player [4] exists, send offer to player [4] to sell them qty [1] of item [2] at price [3] to player [4]
                //                                                                      Start timer, wait for reply from player [4], if accept take money transfer goods, if deny or time runs out stop timer cancel sale 
                //  } else { display error that no trade regions are in range }

                //  case 4: //ie /sell 1 uranium 50
                //  case 5: //ie /sell 1 uranium 50 bob to offer 1 uranium to bob for 50
                // note - if bob has already been sent an offer, reply with bob is already negotiating a deal
                //deal timer should be between 30 seconds and 2 minutes
                #endregion

                //now we need to check if range is ignored, or perform a range check on [4] to see if it is in selling range
                //if both checks fail tell player nothing is near enough to trade with

                //just have to figure out how to read coords and take stuff out of inventory and it should be pretty straightforward..
                //first off is there any point in proceeding
                //if (limited range setting is false or My location works out To be within 2500 of a valid trade area) {
                bool RangeCheck = true; //placeholder until private bool RangeCheck(string targetname) {} can be made
                if (!EconomyConsts.LimitedRange || RangeCheck)
                {
                    //in this release the only valid trade area is the default 0:0:0 area of map or other players?
                    //but should still go through the motions so backend is ready
                    //by default in initialisation we alerady created a bank entry for the NPC
                    //now we take our regex fields populated above and start checking
                    //what the player seems to want us to do - switch needs to be converted to the regex populated fields
                    //using split at the moment for debugging and structruing desired logic
                    decimal sellQuantity = 0;
                    string itemName = "";
                    decimal sellPrice = 1;
                    bool useBankBuyPrice = false;
                    string buyerName = "";
                    bool sellToMerchant = false;
                    bool offerToMarket = false;

                    match = Regex.Match(messageText, SellPattern, RegexOptions.IgnoreCase);
                    //ok we need to catch the target (player/faction) at [4] or set it to NPC if its null
                    //then populate the other fields.  
                    //string reply = "match " + match.Groups["qty"].Value + match.Groups["item"].Value + match.Groups["user"].Value + match.Groups["price"].Value;
                    if (match.Success)         
                    {
                        itemName = match.Groups["item"].Value;
                        buyerName = match.Groups["user"].Value;
                        sellQuantity = Convert.ToDecimal(match.Groups["qty"].Value, CultureInfo.InvariantCulture);
                        if (!decimal.TryParse(match.Groups["price"].Value, out sellPrice))
                            // We will use the they price they set at which they will buy from the player.
                            useBankBuyPrice = true;  // sellprice will be 0 because TryParse failed.

                        if (string.IsNullOrEmpty(buyerName))
                            sellToMerchant = true;

                        if (!useBankBuyPrice && sellToMerchant)
                        {
                            // A price was specified, we actually intend to offer the item for sale at a set price to the market, not the bank.
                            offerToMarket = true;
                            sellToMerchant = false;
                        }

                        MyObjectBuilder_Base content;
                        string[] options;
                        // Search for the item and find one match only, either by exact name or partial name.
                        if (!Support.FindPhysicalParts(itemName, out content, out options) && options.Length > 0)
                        {
                            if (options.Length > 10)
                                MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options) + " ?", null, "OK");
                            else
                                MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options) + " ?");
                            return true;
                        }

                        // TODO: do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.

                        if (sellQuantity <= 0)
                        {
                            MyAPIGateway.Utilities.ShowMessage("SELL", "You must provide a valid quantity to sell.");
                            return true;
                        }

                        // Verify that the items are in the player inventory.
                        // TODO: later check trade block, cockpit inventory, cockpit ship inventory, inventory of targeted cube.
                        var inventoryOwnwer = MyAPIGateway.Session.Player.Controller.ControlledEntity as IMyInventoryOwner;
                        var inventory = inventoryOwnwer.GetInventory(0) as Sandbox.ModAPI.IMyInventory;
                        MyFixedPoint amount = (MyFixedPoint)sellQuantity;

                        if (!inventory.ContainItems(amount, (MyObjectBuilder_PhysicalObject)content))
                        {
                            // Insufficient items in inventory.
                            MyAPIGateway.Utilities.ShowMessage("SELL", "You don't have {0} of '{1}' to sell.", sellQuantity, content.GetDisplayName()) ;
                            return true;
                        }

                        MyAPIGateway.Utilities.ShowMessage("SELL", "ok");

                        inventory.RemoveItemsOfType(amount, (MyObjectBuilder_PhysicalObject)content);

                        // TODO: add items into holding as part of the sell message, from container Id: inventory.Owner.EntityId.
                        MessageSell.SendMessage(buyerName, sellQuantity, content.TypeId.ToString(), content.SubtypeName, sellPrice, useBankBuyPrice, sellToMerchant, offerToMarket);

                        //    MyAPIGateway.Utilities.ShowMessage("SELL", reply);
                        return true;
                    }


                    switch (split.Length)
                    {
                        case 1: //ie /sell
                            MyAPIGateway.Utilities.ShowMessage("SELL", "/sell #1 #2 #3 #4");
                            MyAPIGateway.Utilities.ShowMessage("SELL", "#1 is quantity, #2 is item, #3 optional price to offer #4 optional person to sell to");
                            return true;
                        case 2: //ie /sell all or /sell cancel or /sell accept or /sell deny
                            if (split[1].Equals("cancel", StringComparison.InvariantCultureIgnoreCase)) { MyAPIGateway.Utilities.ShowMessage("SELL", "Cancel Not yet implemented in this release"); return true; }
                            if (split[1].Equals("all", StringComparison.InvariantCultureIgnoreCase)) { MyAPIGateway.Utilities.ShowMessage("SELL", "all not yet implemented"); return true; }
                            if (split[1].Equals("accept", StringComparison.InvariantCultureIgnoreCase)) { MyAPIGateway.Utilities.ShowMessage("SELL", "accept not yet implemented"); return true; }
                            if (split[1].Equals("deny", StringComparison.InvariantCultureIgnoreCase)) { MyAPIGateway.Utilities.ShowMessage("SELL", "deny not yet implemented"); return true; }
                            return false;
                        default: //must be more than 2
                            //ie /sell all uranium || /sell 1 uranium || /sell 1 uranium 50 || /sell 1 uranium 50 bob to offer 1 uranium to bob for 50
                            //need an item search sub for this bit to compliment the regex and for neatness
                            //if (split[3] == null) split[3]= "NPC";
                            if (split.Length == 3 && (decimal.TryParse(split[1], out sellQuantity) || split[1].Equals("all", StringComparison.InvariantCultureIgnoreCase)))//eg /sell 3 iron
                            {   //sellqty is now split[1] as decimal
                                //if split[1] = all then we would need to sell everything they player is carrying
                                itemName = split[2];
                                //sell price is set by price book in this scenario, and we assume we are selling to the NPC market
                                buyerName = EconomyConsts.NpcMerchantName;
                            }
                            else { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: qty wasnt a number or all?"); }

                            //eg /sell 3 iron 50
                            if (split.Length == 4 && decimal.TryParse(split[1], out sellQuantity) && decimal.TryParse(split[3], out sellPrice))
                            {   //sellqty is now split[1] as decimal
                                itemName = split[2];
                                //sellprice is now split[3], and we assume we are posting an offer to the stockmarket not selling blindly to npc
                                buyerName = "OFFER";
                            }
                            else { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: qty or price wasnt a number probably all?"); }

                            //eg /sell 3 iron 50 fred
                            if (split.Length == 5 && decimal.TryParse(split[1], out sellQuantity) && decimal.TryParse(split[3], out sellPrice))
                            {   //sellqty is now split[1] as decimal
                                itemName = split[2];
                                //sellprice is now split[3]
                                buyerName = split[4];
                                //this scenario we assume we sell to a player
                            }
                            else { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: qty or price wasnt a number probably all?"); }

                            //at this point we should have enough information to make a sale
                            string reply = "Debug: Selling " + sellQuantity + "x " + itemName + " to " + buyerName + " for " + (sellQuantity * sellPrice);
                            MyAPIGateway.Utilities.ShowMessage("SELL", reply);
                            //---------  now this bit of code is interesting we get id of item
                            MyObjectBuilder_Base content;
                            string[] options;
                            // Search for the item and find one match only, either by exact name or partial name.
                            if (!Support.FindPhysicalParts(itemName, out content, out options) && options.Length > 0)
                            {
                                MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options) + " ?");
                                return true;
                            }
                            reply = content.TypeId.ToString() + " - " + content.SubtypeName + " - " + MarketManagement.GetDisplayName(content.TypeId.ToString(), content.SubtypeName);
                            MyAPIGateway.Utilities.ShowMessage("SELL", reply);
                            //---------

                            if (buyerName != EconomyConsts.NpcMerchantName && buyerName != "OFFER")
                            { //must be selling to a player (or faction ?)
                                //check the item to sell is a valid product, do usual qty type checks etc
                                //check the player / faction exists etc etc
                                MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We are selling to a player send them the request prompt or if it is a faction check they are trading this item");
                            }
                            else
                            {
                                if (buyerName == EconomyConsts.NpcMerchantName) { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We must be selling to NPC - skip prompts sell immediately at price book price"); }
                                if (buyerName == "OFFER") { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We must be posting a sell offer to stockmarket - skip prompts post offer"); }
                            }
                            return true;
                    }


                }
                else { MyAPIGateway.Utilities.ShowMessage("SELL", "Nothing/Nobody nearby to trade with!"); return true; }
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

            // value command for looking up the table price of an item.
            // eg /value itemname optionalqty
            // !!!is it possible to make this work more like the bal or pay command so
            // that the same item search can be reused in buy and sell too!!! ?
            if (split[0].Equals("/value", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, ValuePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var itemName = match.Groups["Key"].Value;
                    var strAmount = match.Groups["Value"].Value;
                    MyObjectBuilder_Base content;
                    string[] options;

                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options) && options.Length > 0)
                    {
                        // TODO: use ShowMissionScreen if options.Length > 10 ?
                        MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options) + " ?");
                        return true;
                    }
                    if (content != null)
                    {
                        decimal amount;
                        if (!decimal.TryParse(strAmount, out amount))
                            amount = 1; // if it cannot parse it, assume it is 1. It may not have been specified.

                        if (amount < 0) // if a negative value is provided, make it 1.
                            amount = 1;

                        if (content.TypeId != typeof(MyObjectBuilder_Ore) && content.TypeId != typeof(MyObjectBuilder_Ingot))
                        {
                            // must be whole numbers.
                            amount = Math.Round(amount, 0);
                        }

                        // Primary checks for the component are carried out Client side to reduce processing time on the server. not that 2ms matters but if 
                        // there is thousands of these requests at once one day in "space engineers the MMO" or on some auto-trading bot it might become a problem
                        MessageMarketItemValue.SendMessage(content.TypeId.ToString(), content.SubtypeName, amount, MarketManagement.GetDisplayName(content.TypeId.ToString(), content.SubtypeName));
                        return true;
                    }

                    MyAPIGateway.Utilities.ShowMessage("VALUE", "Unknown Item. Could not find the specified name.");
                }
                else
                    MyAPIGateway.Utilities.ShowMessage("VALUE", "You need to specify something to value eg /value ice");
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
            if (split[0].Equals("/ehelp", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type help? show what else they can get help on
                    //might be better to make a more detailed help reply here using mission window later on
                    MyAPIGateway.Utilities.ShowMessage("help", "Commands: help, buy, sell, bal, pay, seen");
                    if (MyAPIGateway.Session.Player.IsAdmin())
                    {
                        MyAPIGateway.Utilities.ShowMessage("admin", "Commands: accounts, bal player, reset, pay player +/-any_amount");
                    }
                    MyAPIGateway.Utilities.ShowMessage("help", "Try '/ehelp command' for more informations about specific command");
                    return true;
                }
                else
                {
                    switch (split[1].ToLowerInvariant())
                    {
                        // did we type /ehelp help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/ehelp #", "Displays help on the specified command [#].");
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
                        case "value":
                            MyAPIGateway.Utilities.ShowMessage("Help", "/value X Y - Looks up item [X] of optional quantity [Y] and reports the buy and sell value.");
                            MyAPIGateway.Utilities.ShowMessage("Help", "Example: /value Ice 20    or   /value ice");
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