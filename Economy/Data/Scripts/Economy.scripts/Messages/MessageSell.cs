namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EconConfig;
    using Economy.scripts;
    using Economy.scripts.EconStructures;
    using ProtoBuf;
    using Sandbox.Common;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using IMyCargoContainer = Sandbox.ModAPI.Ingame.IMyCargoContainer;

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
                            EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during '/sell'; '{0}' '{1}'.", ItemTypeId, ItemSubTypeName);
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

                        // Build list of all cargo blocks that player is attached to as pilot or passenger.
                        List<IMySlimBlock> cargoBlocks = new List<IMySlimBlock>();
                        var controllingCube = sellingPlayer.Controller.ControlledEntity as IMyCubeBlock;
                        if (controllingCube != null)
                        {
                            var grids = controllingCube.CubeGrid.GetAttachedGrids(AttachedGrids.Static);

                            var blocks = new List<IMySlimBlock>();
                            foreach (var grid in grids)
                            {
                                grid.GetBlocks(blocks, b => b != null
                                    && b.FatBlock != null
                                    && !b.FatBlock.BlockDefinition.TypeId.IsNull
                                    && b.FatBlock is IMyCargoContainer);

                                foreach (var block in blocks)
                                {
                                    var relation = block.FatBlock.GetUserRelationToOwner(sellingPlayer.PlayerID);
                                    if (relation != MyRelationsBetweenPlayerAndBlock.Enemies
                                        && relation != MyRelationsBetweenPlayerAndBlock.Neutral)
                                        cargoBlocks.Add(block);
                                }
                            }
                        }

                        var position = ((IMyEntity)character).WorldMatrix.Translation;
                        var playerInventory = character.GetPlayerInventory();
                        MyFixedPoint amount = (MyFixedPoint)ItemQuantity;
                        var storedAmount = playerInventory.GetItemAmount(definition.Id);

                        foreach (var block in cargoBlocks)
                        {
                            var cubeBlock = (Sandbox.Game.Entities.MyEntity)block.FatBlock;
                            var cubeInventory = cubeBlock.GetInventory();
                            storedAmount += cubeInventory.GetItemAmount(definition.Id);
                        }

                        if (amount > storedAmount)
                        {
                            // Insufficient items in inventory.
                            // TODO: use of definition.GetDisplayName() isn't localized here.

                            if (cargoBlocks.Count == 0)
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You don't have {0} of '{1}' to sell. You have {2} in your inventory.", ItemQuantity, definition.GetDisplayName(), storedAmount);
                            else
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You don't have {0} of '{1}' to sell. You have {2} in your player and cargo inventory.", ItemQuantity, definition.GetDisplayName(), storedAmount);
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

                        // need fix negative amounts before checking if the player can afford it.
                        if (!sellingPlayer.IsAdmin())
                            ItemPrice = Math.Abs(ItemPrice);

                        var transactionAmount = ItemPrice * ItemQuantity;

                        if (!sellingPlayer.IsAdmin())
                            transactionAmount = Math.Abs(transactionAmount);

                        if (SellToMerchant) // && (merchant has enough money  || !EconomyScript.Instance.Config.LimitedSupply)
                                            //this is also a quick fix ideally npc should buy what it can afford and the rest is posted as a sell offer
                        {
                            if (accountToBuy.BankBalance >= transactionAmount || !EconomyScript.Instance.Config.LimitedSupply)
                            {
                                // here we look up item price and transfer items and money as appropriate
                                RemoveInventory(playerInventory, cargoBlocks, amount, definition.Id);
                                marketItem.Quantity += ItemQuantity; // increment Market content.

                                accountToBuy.BankBalance -= transactionAmount;
                                accountToBuy.Date = DateTime.Now;

                                accountToSell.BankBalance += transactionAmount;
                                accountToSell.Date = DateTime.Now;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just sold {0} {3} worth of {2} ({1} units)", transactionAmount, ItemQuantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);
                            }
                            else
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "NPC can't afford {0} {4} worth of {2} ({1} units) NPC only has {3} funds!", transactionAmount, ItemQuantity, definition.GetDisplayName(), accountToBuy.BankBalance, EconomyScript.Instance.Config.CurrencyName);
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

                        if (EconomyScript.Instance.Config.LimitedRange && !Support.RangeCheck(buyingPlayer, sellingPlayer))
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
                            MarketManager.CreateTradeOffer(SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity, ItemPrice, accountToBuy.SteamId);

                            // remove items from inventory.
                            RemoveInventory(playerInventory, cargoBlocks, amount, definition.Id);

                            // Only send message to targeted player if this is the only offer pending for them.
                            // Otherwise it will be sent when the have with previous orders in their order Queue.
                            if (EconomyScript.Instance.Data.OrderBook.Count(e => (e.OptionalId == accountToBuy.SteamId.ToString() && e.TradeState == TradeState.SellDirectPlayer)) == 1)
                            {
                                MessageClientTextMessage.SendMessage(accountToBuy.SteamId, "SELL",
                                    "You have received an offer from {0} to buy {1} {2} at price {3} {4} each - type '/sell accept' to accept offer (or '/sell deny' to reject and return item to seller)",
                                    SenderDisplayName, ItemQuantity, definition.GetDisplayName(), ItemPrice, EconomyScript.Instance.Config.CurrencyName);
                            }

                            // TODO: Improve the message here, to say who were are trading to, and that the item is gone from inventory.
                            // send message to seller to confirm action, "Your Trade offer has been submitted, and the goods removed from you inventory."
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Your offer of {0} {1} for {2} {4} each has been sent to {3}.", ItemQuantity, definition.GetDisplayName(), ItemPrice, accountToBuy.NickName, EconomyScript.Instance.Config.CurrencyName);

                            return;
                        }
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
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You cannot afford {0} {1} at this time.", transactionAmount, EconomyScript.Instance.Config.CurrencyName);
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
                            EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during '/sell accept'; '{0}' '{1}'.", order.TypeId, order.SubtypeName);
                            return;
                        }

                        // TODO: Improve the messages.
                        // message back "Your Trade offer of xxx to yyy has been accepted. You have recieved zzzz"
                        MessageClientTextMessage.SendMessage(accountToSell.SteamId, "SELL", "You just sold {0} {3} worth of {2} ({1} units)", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);

                        var collectingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        var playerInventory = collectingPlayer.GetPlayerInventory();
                        bool hasAddedToInventory = true;

                        if (playerInventory != null)
                        {
                            MyFixedPoint amount = (MyFixedPoint)order.Quantity;
                            hasAddedToInventory = Support.InventoryAdd(playerInventory, amount, definition.Id);
                        }

                        if (hasAddedToInventory)
                        {
                            EconomyScript.Instance.Data.OrderBook.Remove(order); // item has been collected, so the order is finalized.
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just purchased {0} {3} worth of {2} ({1} units) which are now in your player inventory.", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);
                        }
                        else
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just purchased {0} {3} worth of {2} ({1} units). Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);

                        // Send message to player if additional offers are pending their attention.
                        DisplayNextOrderToAccept(SenderSteamId);

                        return;
                    }

                case SellAction.Collect:
                    {
                        var collectableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                            (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellTimedout)
                            || (e.TraderId == SenderSteamId && e.TradeState == TradeState.Holding)
                            || (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellRejected)
                            || (e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellAccepted)).ToArray();

                        if (collectableOrders.Length == 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There is nothing to collect currently.");
                            return;
                        }

                        var collectingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

                        // TODO: this is just for debugging until the message below are completed....
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
                                EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during '/sell collect'; '{0}' '{1}'.", order.TypeId, order.SubtypeName);
                                continue;
                            }

                            var remainingToCollect = MessageSell.AddToInventories(collectingPlayer, order.Quantity, definition.Id);
                            var collected = order.Quantity - remainingToCollect;

                            if (remainingToCollect == 0)
                            {
                                EconomyScript.Instance.Data.OrderBook.Remove(order);
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just collected {0} worth of {2} ({1} units)", order.Price * collected, collected, definition.GetDisplayName());
                            }
                            else
                            {
                                order.Quantity = remainingToCollect;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just collected {0} worth of {2} ({1} units). There are {3} remaining.", order.Price * collected, collected, definition.GetDisplayName(), remainingToCollect);
                            }
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

                        // Sellers should be presented with the newest order first, as they will be the most recently created.
                        // use of OrderByDescending above assures us that [0] is the most recent order added.
                        var order = cancellableOrders[0];
                        order.TradeState = TradeState.SellRejected;

                        var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");

                            // trade has been finalized, so we can exit safely.
                            EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during '/sell cancel'; '{0}' '{1}'.", order.TypeId, order.SubtypeName);
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
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just cancelled the sale of {2} ({1} units) for a total of {0} {3} which are now in your inventory.", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);
                        }
                        else
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You just cancelled the sale of {2} ({1} units) for a total of {0} {3}. Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);

                        cancellableOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.TraderId == SenderSteamId && e.TradeState == TradeState.SellDirectPlayer)).OrderByDescending(e => e.Created).ToArray();

                        if (cancellableOrders.Length > 0)
                        {
                            // TODO: Inform the player of the next order in the queue that can be cancelled.
                        }
                        return;
                    }
                    break;

                case SellAction.Deny:
                    {
                        var buyOrdersForMe = EconomyScript.Instance.Data.OrderBook.Where(e =>
                              (e.OptionalId == SenderSteamId.ToString() && e.TradeState == TradeState.SellDirectPlayer)).OrderBy(e => e.Created).ToArray();

                        if (buyOrdersForMe.Length == 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "There is nothing to deny currently.");
                        }

                        // Buyers should be presented with the oldest order first, as they will timout first.
                        // use of OrderBy above assures us that [0] is the most oldest order added.
                        var order = buyOrdersForMe[0];
                        order.TradeState = TradeState.SellRejected;

                        var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

                        if (definition == null)
                        {
                            // Someone hacking, and passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item in your order doesn't exist!");

                            // trade has been finalized, so we can exit safely.
                            EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during '/sell deny'; '{0}' '{1}'.", order.TypeId, order.SubtypeName);
                            return;
                        }

                        var transactionAmount = order.Price * order.Quantity;
                        var buyerId = ulong.Parse(order.OptionalId);
                        MessageClientTextMessage.SendMessage(buyerId, "SELL", "You just rejected the purchase of {2} ({1} units) for a total of {0} {3}.", transactionAmount, order.Quantity, definition.GetDisplayName(), EconomyScript.Instance.Config.CurrencyName);

                        // TODO: return items to inventory automatically to Trader inventory if there is space.
                        MessageClientTextMessage.SendMessage(order.TraderId, "SELL", "{3} has just rejected your offer of {2} ({1} units) for a total of {0} {4}. Enter '/sell collect' when you are ready to receive them.", transactionAmount, order.Quantity, definition.GetDisplayName(), SenderDisplayName, EconomyScript.Instance.Config.CurrencyName);

                        // Send message to player if additional offers are pending their attention.
                        DisplayNextOrderToAccept(SenderSteamId);
                        return;
                    }
                    break;
            }

            // this is a fall through from the above conditions not yet complete.
            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Not yet complete.");
        }

        /// <summary>
        /// Send a message to targeted player if have additional offers pending for them.
        /// </summary>
        /// <param name="steamdId"></param>
        private void DisplayNextOrderToAccept(ulong steamdId)
        {
            // Buyers should be presented with the oldest order first, as they will timout first.
            // use of OrderBy assures us that [0] is the most oldest order added.
            var remaingingUnacceptedOrders = EconomyScript.Instance.Data.OrderBook.Where(e =>
                (e.OptionalId == steamdId.ToString() && e.TradeState == TradeState.SellDirectPlayer)).OrderBy(e => e.Created).ToList();

            if (remaingingUnacceptedOrders.Count <= 0)
                return;

            var order = remaingingUnacceptedOrders[0];
            var accountToSell = AccountManager.FindAccount(order.TraderId);
            var transactionAmount = order.Price * order.Quantity;
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(steamdId);

            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            var definition = MyDefinitionManager.Static.GetDefinition(order.TypeId, order.SubtypeName);

            if (definition == null)
            {
                // Someone hacking, and passing bad data?
                MessageClientTextMessage.SendMessage(steamdId, "SELL", "Sorry, the item in your order doesn't exist!");
                EconomyScript.Instance.ServerLogger.Write("Definition could not be found for item during 'DisplayNextOrderToAccept'; '{0}' '{1}'.", order.TypeId, order.SubtypeName);
                return;
            }

            MessageClientTextMessage.SendMessage(steamdId, "SELL",
                "You have received an offer from {0} to buy {1} {2} at total price {3} {4} - type '/sell accept' to accept offer (or '/sell deny' to reject and return item to seller)",
                accountToSell.NickName, order.Quantity, definition.GetDisplayName(), transactionAmount, EconomyScript.Instance.Config.CurrencyName);
        }

        private void RemoveInventory(IMyInventory playerInventory, List<IMySlimBlock> cargoBlocks, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var available = playerInventory.GetItemAmount(definitionId);
            if (amount <= available)
            {
                playerInventory.RemoveItemsOfType(amount, definitionId);
                amount = 0;
            }
            else
            {
                playerInventory.RemoveItemsOfType(available, definitionId);
                amount -= available;
            }

            foreach (var block in cargoBlocks)
            {
                if (amount > 0)
                {
                    var cubeBlock = (Sandbox.Game.Entities.MyEntity)block.FatBlock;
                    var cubeInventory = cubeBlock.GetInventory();
                    available = cubeInventory.GetItemAmount(definitionId);
                    if (amount <= available)
                    {
                        cubeInventory.RemoveItemsOfType(amount, definitionId);
                        amount = 0;
                    }
                    else
                    {
                        cubeInventory.RemoveItemsOfType(available, definitionId);
                        amount -= available;
                    }
                }
            }
        }

        internal static decimal AddToInventories(IMyPlayer collectingPlayer, decimal quantity, MyDefinitionId definitionId)
        {
            MyFixedPoint amount = (MyFixedPoint)quantity;

            List<IMySlimBlock> cargoBlocks = new List<IMySlimBlock>();
            var controllingCube = collectingPlayer.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube != null)
            {
                var grids = controllingCube.CubeGrid.GetAttachedGrids(AttachedGrids.Static);

                var blocks = new List<IMySlimBlock>();
                foreach (var grid in grids)
                {
                    grid.GetBlocks(blocks, b => b != null
                        && b.FatBlock != null
                        && !b.FatBlock.BlockDefinition.TypeId.IsNull
                        && b.FatBlock is IMyCargoContainer);

                    foreach (var block in blocks)
                    {
                        var relation = block.FatBlock.GetUserRelationToOwner(collectingPlayer.PlayerID);
                        if (relation != MyRelationsBetweenPlayerAndBlock.Enemies
                            && relation != MyRelationsBetweenPlayerAndBlock.Neutral)
                            cargoBlocks.Add(block);
                    }
                }
            }

            foreach (var block in cargoBlocks)
            {
                if (amount > 0)
                {
                    var cubeBlock = (Sandbox.Game.Entities.MyEntity)block.FatBlock;
                    var cubeInventory = cubeBlock.GetInventory();
                    var space = cubeInventory.ComputeAmountThatFits(definitionId);
                    if (amount <= space)
                    {
                        if (Support.InventoryAdd(cubeInventory, amount, definitionId))
                            amount = 0;
                    }
                    else
                    {
                        if (Support.InventoryAdd(cubeInventory, space, definitionId))
                            amount -= space;
                    }
                }
            }

            if (amount > 0)
            {
                var playerInventory = collectingPlayer.GetPlayerInventory();
                if (playerInventory != null)
                {
                    var space = ((Sandbox.Game.MyInventory)playerInventory).ComputeAmountThatFits(definitionId);

                    if (amount <= space)
                    {
                        if (Support.InventoryAdd(playerInventory, amount, definitionId))
                            amount = 0;
                    }
                    else
                    {
                        if (Support.InventoryAdd(playerInventory, space, definitionId))
                            amount -= space;
                    }
                }
            }

            return (decimal)amount;
        }
    }
}
