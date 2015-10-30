namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using Economy.scripts;
    using Economy.scripts.EconStructures;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    /// <summary>
    /// this is to do the actual work of checking and moving the goods.
    /// </summary>
    [ProtoContract]
    public class MessageSell : MessageBase
    {
        [ProtoMember(1)]
        public SellAction SellAction;

        /// <summary>
        /// person, NPC, offer or faction to sell to
        /// </summary>
        [ProtoMember(2)]
        public string ToUserName;

        /// <summary>
        /// qty of item
        /// </summary>
        [ProtoMember(3)]
        public decimal ItemQuantity;

        /// <summary>
        /// item id we are selling
        /// </summary>
        [ProtoMember(4)]
        public string ItemTypeId;

        /// <summary>
        /// item subid we are selling
        /// </summary>
        [ProtoMember(5)]
        public string ItemSubTypeName;

        /// <summary>
        /// unit price of item
        /// </summary>
        [ProtoMember(6)]
        public decimal ItemPrice;

        /// <summary>
        /// Use the Current Buy price to sell it at. The Player 
        /// will not have access to this information without fetching it first. This saves us the trouble.
        /// </summary>
        [ProtoMember(7)]
        public bool UseBankBuyPrice;

        /// <summary>
        /// We are selling to the Merchant.
        /// </summary>
        [ProtoMember(8)]
        public bool SellToMerchant;

        /// <summary>
        /// The Item is been put onto the market.
        /// </summary>
        [ProtoMember(9)]
        public bool OfferToMarket;

        //[ProtoMember(10)]
        //public string zone; //used to identify market we are selling to ??

        public static void SendAcceptMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { SellAction = SellAction.Accept });
        }

        public static void SendCancelMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { SellAction = SellAction.Cancel });
        }

        public static void SendCollectMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { SellAction = SellAction.Collect });
        }

        public static void SendDenyMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { SellAction = SellAction.Deny });
        }

        public static void SendSellMessage(string toUserName, decimal itemQuantity, string itemTypeId, string itemSubTypeName, decimal itemPrice, bool useBankBuyPrice, bool sellToMerchant, bool offerToMarket)
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { SellAction = SellAction.Create, ToUserName = toUserName, ItemQuantity = itemQuantity, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemPrice = itemPrice, UseBankBuyPrice = useBankBuyPrice, SellToMerchant = sellToMerchant, OfferToMarket = offerToMarket });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            switch (SellAction)
            {
                case SellAction.Create:
                    {
                        //* Logic:                     
                        //* Get player steam ID
                        var sellingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

                        MyPhysicalItemDefinition definition = null;
                        MyObjectBuilderType result;
                        if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
                        {
                            var id = new MyDefinitionId(result, ItemSubTypeName);
                            MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
                        }

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you specified doesn't exist!");
                            return;
                        }

                        // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
                        if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
                        {
                            if (ItemQuantity != Math.Truncate(ItemQuantity))
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You must provide a whole number for the quantity of that item.");
                                return;
                            }
                            //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
                        }

                        if (ItemQuantity <= 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Invalid quantity, or you dont have any to trade!");
                            return;
                        }

                        // Who are we selling to?
                        BankAccountStruct accountToBuy;
                        if (SellToMerchant)
                            accountToBuy = AccountManager.FindAccount(EconomyConsts.NpcMerchantId);
                        else
                            accountToBuy = AccountManager.FindAccount(ToUserName);

                        if (accountToBuy == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, player does not exist or have an account!");
                            return;
                        }

                        if (MarketManager.IsItemBlacklistedOnServer(ItemTypeId, ItemSubTypeName))
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you tried to sell is blacklisted on this server.");
                            return;
                        }

                        // Verify that the items are in the player inventory.
                        // TODO: later check trade block, cockpit inventory, cockpit ship inventory, inventory of targeted cube.

                        // Get the player's inventory, regardless of if they are in a ship, or a remote control cube.
                        var character = sellingPlayer.GetCharacter();
                        // TODO: do players in Cryochambers count as a valid trading partner? They should be alive, but the connected player may be offline.
                        // I think we'll have to do lower level checks to see if a physical player is Online.
                        if (character == null)
                        {
                            // Player has no body. Could mean they are dead.
                            // Either way, there is no inventory.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are dead. You cannot trade while dead.");
                            return;
                        }

                        // TODO: is a null check adaqaute?, or do we need to check for IsDead?
                        // I don't think the chat console is accessible during respawn, only immediately after death.
                        // Is it valid to be able to trade when freshly dead?
                        //var identity = payingPlayer.Identity();
                        //MyAPIGateway.Utilities.ShowMessage("CHECK", "Is Dead: {0}", identity.IsDead);

                        //if (identity.IsDead)
                        //{
                        //    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are dead. You cannot trade while dead.");
                        //    return;
                        //}

                        var position = ((IMyEntity)character).WorldMatrix.Translation;
                        var inventory = character.GetPlayerInventory();
                        MyFixedPoint amount = (MyFixedPoint)ItemQuantity;

                        var storedAmount = inventory.GetItemAmount(definition.Id);
                        if (amount > storedAmount)
                        {
                            // Insufficient items in inventory.
                            // TODO: use of definition.GetDisplayName() isn't localized here.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You don't have {0} of '{1}' to sell. You have {2} in your inventory.", ItemQuantity, definition.GetDisplayName(), storedAmount);
                            return;
                        }

                        MarketItemStruct marketItem = null;

                        if (SellToMerchant || UseBankBuyPrice)
                        {
                            var markets = MarketManager.FindMarketsFromLocation(position);
                            if (markets.Count == 0)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, your are not in range of any markets!");
                                return;
                            }

                            // TODO: find market with best Buy price that isn't blacklisted.

                            var market = markets.FirstOrDefault();
                            if (market == null)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the market you are accessing does not exist!");
                                return;
                            }

                            marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                            if (marketItem == null)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the items you are trying to sell doesn't have a market entry!");
                                // In reality, this shouldn't happen as all markets have their items synced up on start up of the mod.
                                return;
                            }

                            if (marketItem.IsBlacklisted)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you tried to sell is blacklisted in this market.");
                                return;
                            }

                            if (UseBankBuyPrice)
                                // The player is selling, but the *Market* will *buy* it from the player at this price.
                                ItemPrice = marketItem.BuyPrice;
                        }

                        var accountToSell = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                        var transactionAmount = ItemPrice * ItemQuantity;

                        // need fix negative amounts before checking if the player can afford it.
                        if (!sellingPlayer.IsAdmin())
                            transactionAmount = Math.Abs(transactionAmount);

                        if (SellToMerchant) // && (merchant has enough money  || !EconomyConsts.LimitedSupply)
                                            //this is also a quick fix ideally npc should buy what it can afford and the rest is posted as a sell offer
                        {
                            if (accountToBuy.BankBalance >= transactionAmount || !EconomyConsts.LimitedSupply)
                            {
                                // here we look up item price and transfer items and money as appropriate
                                inventory.RemoveItemsOfType(amount, definition.Id);
                                marketItem.Quantity += ItemQuantity; // increment Market content.

                                accountToBuy.BankBalance -= transactionAmount;
                                accountToBuy.Date = DateTime.Now;

                                accountToSell.BankBalance += transactionAmount;
                                accountToSell.Date = DateTime.Now;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just sold {0} worth of {2} ({1} units)", transactionAmount, ItemQuantity, definition.GetDisplayName());
                            }
                            else
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "NPC can't afford {0} worth of {2} ({1} units) NPC only has {3} funds!", transactionAmount, ItemQuantity, definition.GetDisplayName(), accountToBuy.BankBalance);
                            }
                            return;
                        }

                        if (OfferToMarket)
                        {
                            // TODO: Here we post offer to appropriate zone market

                            return;
                        }

                        // is it a player then?             
                        if (accountToBuy.SteamId == sellingPlayer.SteamUserId)
                        {
                            // commented out for testing with myself.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, you cannot sell to yourself!");
                            return;
                        }

                        // check if buying player is online and in range?
                        var buyingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(accountToBuy.SteamId);

                        if (EconomyConsts.LimitedRange && !Support.RangeCheck(buyingPlayer, sellingPlayer))
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you are not in range of that player!");
                            return;
                        }


                        // if other player online, send message.
                        if (buyingPlayer == null)
                        {
                            // TODO: other player offline.

                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You cannot sell to offline players at this time.");
                            return;

                            // TODO: we need a way to queue up messages.
                            // While you were gone....
                            // You missed an offer for 4000Kg of Gold for 20,000.
                        }
                        else
                        {
                            // The other player is online.

                            // write to Trade offer table.
                            MarketManager.CreateTradeOffer(SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity, transactionAmount, accountToBuy.SteamId);

                            // remove items from inventory.
                            inventory.RemoveItemsOfType(amount, definition.Id);

                            MessageClientTextMessage.SendMessage(accountToBuy.SteamId, "SELL",
                                "You have received an offer from {0} to buy {1} {2} at price {3} - type '/sell accept' to accept offer (or '/sell deny' to reject and return ore to seller)",
                                SenderDisplayName, ItemQuantity, definition.GetDisplayName(), transactionAmount);

                            // TODO: Improve the message here, to say who were are trading to, and that the item is gone from inventory.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Your offer has been sent.");

                            return;
                        }
                        // send message to seller to confirm action, "Your Trade offer has been submitted, and the goods removed from you inventory."

                        // Later actions..
                        // https://github.com/jpcsupplies/Economy_mod/issues/31
                        // https://github.com/jpcsupplies/Economy_mod/issues/46
                        // "/sell cancel"  to cancel trade offer. Did you mistype a number?  
                        // Returned goods need to be queued.
                        // if trade offer rejected, message back "Your Trade offer of xxx to yyy has been rejected."  if first item in queue, "Type '/return' to receive your goods back."
                        // if trade offer times out, message back "Your Trade offer of xxx to yyy has timed."  if first item in queue, "Type '/return' to receive your goods back."
                        // if trade offer accepted, finish funds trasfer. message back "Your Trade offer of xxx to yyy has been accepted. You have recieved zzzz"
                    }
                    break;

                case SellAction.Accept:
                    {
                        var order = EconomyScript.Instance.Data.OrderBook.FirstOrDefault(e => e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellDirectPlayer);
                        if (order == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There are no outstanding orders to be accepted.");
                            return;
                        }

                        var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

                        // get the accounts and check finance.
                        var accountToBuy = AccountManager.FindAccount(ulong.Parse(order.OptionalId));

                        var transactionAmount = order.Price * order.Quantity;

                        // need fix negative amounts before checking if the player can afford it.
                        if (!payingPlayer.IsAdmin())
                            transactionAmount = Math.Abs(transactionAmount);

                        if (accountToBuy.BankBalance < transactionAmount)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You cannot afford {0} at this time.", transactionAmount);
                            return;
                        }

                        var accountToSell = AccountManager.FindAccount(order.TraderId);

                        // rebalance accounts.
                        accountToBuy.BankBalance -= transactionAmount;
                        accountToBuy.Date = DateTime.Now;

                        accountToSell.BankBalance += transactionAmount;
                        accountToSell.Date = DateTime.Now;

                        order.TradeState = TradeState.SellAccepted;

                        var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");

                            // trade has been finalized, so we can exit safely.
                            return;
                        }

                        // TODO: Improve the messages.
                        MessageClientTextMessage.SendMessage(accountToSell.SteamId, "SELL", "You just sold {0} worth of {2} ({1} units)", transactionAmount, order.Quantity, definition.GetDisplayName());

                        var collectingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        var inventory = collectingPlayer.GetPlayerInventory();
                        bool hasAddedToInventory = true;

                        if (inventory != null)
                        {
                            MyFixedPoint amount = (MyFixedPoint)order.Quantity;
                            hasAddedToInventory = Support.InventoryAdd(inventory, amount, definition.Id);
                        }

                        if (hasAddedToInventory)
                        {
                            EconomyScript.Instance.Data.OrderBook.Remove(order); // item has been collected, so the order is finalized.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just purchased {0} worth of {2} ({1} units) which are now in your inventory.", transactionAmount, order.Quantity, definition.GetDisplayName());
                        }
                        else
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just purchased {0} worth of {2} ({1} units). Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName());

                        return;
                    }

                case SellAction.Collect:
                    {
                        var collectableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                            (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellTimedout)
                            || (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellRejected)
                            || (e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellAccepted)).ToArray();

                        if (collectableOrders.Length == 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There is nothing to collect currently.");
                        }

                        var collectingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        var character = collectingPlayer.GetCharacter();
                        var inventory = collectingPlayer.GetPlayerInventory();

                        if (inventory == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You cannot collect items at this time.");
                            return;
                        }

                        // TODO: this is just for debugging....
                        //MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are collecting items from {0} order/s.", collectableOrders.Length);

                        foreach (var order in collectableOrders)
                        {
                            MyPhysicalItemDefinition definition = null;
                            MyObjectBuilderType result;
                            if (MyObjectBuilderType.TryParse(order.TypeId, out result))
                            {
                                var id = new MyDefinitionId(result, order.SubtypeName);
                                MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
                            }

                            if (definition == null)
                            {
                                // Someone hacking, and passing bad data?
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");
                                // TODO: more detail on the item.
                                continue;
                            }

                            MyFixedPoint amount = (MyFixedPoint)order.Quantity;
                            if (!Support.InventoryAdd(inventory, amount, definition.Id))
                            {
                                Support.InventoryDrop((IMyEntity)character, amount, definition.Id);
                            }

                            EconomyScript.Instance.Data.OrderBook.Remove(order);

                            // TODO: display what was collected.
                            //MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just collected {0} worth of {2} ({1} units)", transactionAmount, ItemQuantity, definition.GetDisplayName());
                        }
                        return;
                    }

                case SellAction.Cancel:
                    {
                        var cancellableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellDirectPlayer)).OrderByDescending(e => e.Created).ToArray();

                        if (cancellableOrders.Length == 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There is nothing to cancel currently.");
                        }

                        // use of OrderByDescending above assures us that [0] is the most recent order added.
                        var order = cancellableOrders[0];
                        order.TradeState = TradeState.SellRejected;

                        var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");

                            // trade has been finalized, so we can exit safely.
                            return;
                        }

                        var transactionAmount = order.Price * order.Quantity;
                        var collectingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        var inventory = collectingPlayer.GetPlayerInventory();
                        bool hasAddedToInventory = true;

                        if (inventory != null)
                        {
                            MyFixedPoint amount = (MyFixedPoint)order.Quantity;
                            hasAddedToInventory = Support.InventoryAdd(inventory, amount, definition.Id);
                        }

                        if (hasAddedToInventory)
                        {
                            EconomyScript.Instance.Data.OrderBook.Remove(order); // item has been collected, so the order is finalized.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just cancelled the sale of {2} ({1} units) for {0} which are now in your inventory.", transactionAmount, order.Quantity, definition.GetDisplayName());
                        }
                        else
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just cancelled the sale of {2} ({1} units) for {0}. Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName());

                        cancellableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellDirectPlayer)).OrderByDescending(e => e.Created).ToArray();

                        if (cancellableOrders.Length > 0)
                        {
                            // TODO: Inform the player of the next order in the queue that can be cancelled.
                        }
                    }
                    break;

                case SellAction.Deny:
                    {
                        var rejectableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellDirectPlayer)).OrderByDescending(e => e.Created).ToArray();

                        if (rejectableOrders.Length == 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There is nothing to deny currently.");
                        }

                        // use of OrderByDescending above assures us that [0] is the most recent order added.
                        var order = rejectableOrders[0];
                        order.TradeState = TradeState.SellRejected;

                        var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");

                            // trade has been finalized, so we can exit safely.
                            return;
                        }

                        var transactionAmount = order.Price * order.Quantity;
                        var buyerId = ulong.Parse(order.OptionalId);
                        MessageClientTextMessage.SendMessage(buyerId, "SELL", "You just rejected the purchase of {2} ({1} units) for {0}.", transactionAmount, order.Quantity, definition.GetDisplayName());
                        MessageClientTextMessage.SendMessage(order.TraderId, "SELL", "{3} has just rejected your offer of {2} ({1} units) for {0}. Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName(), SenderDisplayName);

                        rejectableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellDirectPlayer)).OrderByDescending(e => e.Created).ToArray();

                        if (rejectableOrders.Length > 0)
                        {
                            // TODO: Inform the player of the next order in the queue that can be rejected.
                        }
                    }
                    break;
            }

            // this is a fall through from the above conditions not yet complete.
            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Not yet complete.");
        }
    }
}
