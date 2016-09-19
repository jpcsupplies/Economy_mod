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
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    /// <summary>
    /// this is to do the actual work of checking and moving the goods when a player is buying from something/someone
    /// </summary>
    [ProtoContract]
    public class MessageBuy : MessageBase
    {
        #region properties

        /// <summary>
        /// person, NPC, offer or faction to submit an offer to buy from
        /// </summary>
        [ProtoMember(1)]
        public string FromUserName;

        /// <summary>
        /// qty of item
        /// </summary>
        [ProtoMember(2)]
        public decimal ItemQuantity;

        /// <summary>
        /// item name / id we are selling
        /// </summary>
        [ProtoMember(3)]
        public string ItemTypeId;

        [ProtoMember(4)]
        public string ItemSubTypeName;

        /// <summary>
        /// unit price of item
        /// </summary>
        [ProtoMember(5)]
        public decimal ItemPrice;

        /// <summary>
        /// Use the Current Sell price to buy it at. The Player 
        /// will not have access to this information without fetching it first. This saves us the trouble.
        /// </summary>
        [ProtoMember(6)]
        public bool UseBankSellPrice;

        /// <summary>
        /// We are selling to the Merchant.
        /// </summary>
        [ProtoMember(7)]
        public bool BuyFromMerchant;

        /// <summary>
        /// The Item is been put onto the market.
        /// </summary>
        [ProtoMember(8)]
        public bool FindOnMarket;

        #endregion

        public static void SendMessage(string toUserName, decimal itemQuantity, string itemTypeId, string itemSubTypeName, decimal itemPrice, bool useBankBuyPrice, bool sellToMerchant, bool offerToMarket)
        {
            ConnectionHelper.SendMessageToServer(new MessageBuy { FromUserName = toUserName, ItemQuantity = itemQuantity, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemPrice = itemPrice, UseBankSellPrice = useBankBuyPrice, BuyFromMerchant = sellToMerchant, FindOnMarket = offerToMarket });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy started by Steam Id '{0}'.", SenderSteamId);

            if (!EconomyScript.Instance.ServerConfig.EnableNpcTradezones && !EconomyScript.Instance.ServerConfig.EnablePlayerTradezones)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "All Trade zones are disabled.");
                return;
            }

            // Get player steam ID
            var buyingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            MyDefinitionBase definition = null;
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
            {
                var id = new MyDefinitionId(result, ItemSubTypeName);
                MyDefinitionManager.Static.TryGetDefinition(id, out definition);
            }

            if (definition == null)
            {
                // Someone hacking, and passing bad data?
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you specified doesn't exist!");
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- item doesn't exist.", SenderSteamId);
                return;
            }

            if (definition.Id.TypeId == typeof (MyObjectBuilder_GasProperties))
            {
                // TODO: buy gasses!
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Cannot buy gasses currently.");
                return;
            }

            // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
            if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (ItemQuantity != Math.Truncate(ItemQuantity))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You must provide a whole number for the quantity to buy that item.");
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- invalid qantity.", SenderSteamId);
                    return;
                }
                //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
            }

            if (ItemQuantity <= 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You must provide a valid quantity to buy.");
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- invalid qantity.", SenderSteamId);
                return;
            }

            // Who are we buying from?
            BankAccountStruct accountToSell;
            if (BuyFromMerchant)
                accountToSell = AccountManager.FindAccount(EconomyConsts.NpcMerchantId);
            else
                accountToSell = AccountManager.FindAccount(FromUserName);

            if (accountToSell == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, player does not exist or have an account!");
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- no account.", SenderSteamId);
                return;
            }

            if (MarketManager.IsItemBlacklistedOnServer(ItemTypeId, ItemSubTypeName))
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you tried to buy is blacklisted on this server.");
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- item blacklisted.", SenderSteamId);
                return;
            }

            // Get the player's inventory, regardless of if they are in a ship, or a remote control cube.
            var character = buyingPlayer.GetCharacter();
            // TODO: do players in Cryochambers count as a valid trading partner? They should be alive, but the connected player may be offline.
            // I think we'll have to do lower level checks to see if a physical player is Online.
            if (character == null)
            {
                // Player has no body. Could mean they are dead.
                // Either way, there is no inventory.
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You are dead. You cannot trade while dead.");
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- player is dead.", SenderSteamId);
                return;
            }

            // TODO: is a null check adaqaute?, or do we need to check for IsDead?
            // I don't think the chat console is accessible during respawn, only immediately after death.
            // Is it valid to be able to trade when freshly dead?
            //var identity = buyingPlayer.Identity();
            //MyAPIGateway.Utilities.ShowMessage("CHECK", "Is Dead: {0}", identity.IsDead);

            //if (identity.IsDead)
            //{
            //    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You are dead. You cannot trade while dead.");
            //    return;
            //}

            var position = ((IMyEntity)character).WorldMatrix.Translation;

            MarketItemStruct marketItem = null;

            if (BuyFromMerchant || UseBankSellPrice)
            {
                var markets = MarketManager.FindMarketsFromLocation(position);
                if (markets.Count == 0)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, your are not in range of any markets!");
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- no market in range.", SenderSteamId);
                    return;
                }

                // TODO: find market with best Sell price that isn't blacklisted.

                var market = markets.FirstOrDefault();
                if (market == null)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the market you are accessing does not exist!");
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- no market found.", SenderSteamId);
                    return;
                }

                accountToSell = AccountManager.FindAccount(market.MarketId);

                marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                if (marketItem == null)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the items you are trying to buy doesn't have a market entry!");
                    // In reality, this shouldn't happen as all markets have their items synced up on start up of the mod.
                    return;
                }

                if (marketItem.IsBlacklisted)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you tried to buy is blacklisted in this market.");
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- item is blacklisted in market.", SenderSteamId);
                    return;
                }

                // Verify that the items are in the player inventory.
                // TODO: later check trade block, cockpit inventory, cockpit ship inventory, inventory of targeted cube.

                if (UseBankSellPrice)
                    // The player is buying, but the *Market* will *sell* it to the player at this price.
                    ItemPrice = marketItem.SellPrice;
            }

            var accountToBuy = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
            var transactionAmount = ItemPrice * ItemQuantity;

            // need fix negative amounts before checking if the player can afford it.
            if (!buyingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            // TODO: admin check on ability to afford it? 
            //[maybe later, our pay and reset commands let us steal money from npc anyway best to keep admin abuse features to minimum]
            //[we could put an admin check on blacklist however, allow admins to spawn even blacklisted gear]
            if (accountToBuy.BankBalance < transactionAmount && accountToBuy.SteamId != accountToSell.SteamId)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you cannot afford {0} {1}!", transactionAmount, EconomyScript.Instance.ServerConfig.CurrencyName);
                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- not enough money.", SenderSteamId);
                return;
            }

            if (BuyFromMerchant) // and supply is not exhausted, or unlimited mode is not on.   
                                 //This is a quick fix, ideally it should do a partial buy of what is left and post a buy offer for remainder
            {
                // here we look up item price and transfer items and money as appropriate
                if (marketItem.Quantity >= ItemQuantity
                    || (!EconomyScript.Instance.ServerConfig.LimitedSupply && accountToBuy.SteamId != accountToSell.SteamId))
                {
                    marketItem.Quantity -= ItemQuantity; // reduce Market content.
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy finalizing by Steam Id '{0}' -- adding to inventory.", SenderSteamId);
                    var remainingToCollect = MessageSell.AddToInventories(buyingPlayer, ItemQuantity, definition.Id);

                    //EconomyScript.Instance.Config.LimitedSupply

                    if (accountToBuy.SteamId != accountToSell.SteamId)
                    {
                        accountToSell.BankBalance += transactionAmount;
                        accountToSell.Date = DateTime.Now;

                        accountToBuy.BankBalance -= transactionAmount;
                        accountToBuy.Date = DateTime.Now;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You just purchased {1} '{2}' for {0} {3}", transactionAmount, ItemQuantity, definition.GetDisplayName(), EconomyScript.Instance.ServerConfig.CurrencyName);

                        MessageUpdateClient.SendAccountMessage(accountToSell);
                        MessageUpdateClient.SendAccountMessage(accountToBuy);
                    }
                    else
                    {
                        accountToBuy.Date = DateTime.Now;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You just arranged transfer of {0} '{1}' into your inventory.", ItemQuantity, definition.GetDisplayName());
                    }

                    if (remainingToCollect > 0)
                    {
                        MarketManager.CreateStockHeld(buyingPlayer.SteamUserId, ItemTypeId, ItemSubTypeName, remainingToCollect, ItemPrice);
                        // TODO: there should be a common command to collect items. Not use /sell.
                        MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "There are {0} remaining to collect. Use '/collect'", remainingToCollect);
                    }
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy complete by Steam Id '{0}' -- items bought.", SenderSteamId);
                }
                else
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "There isn't '{0}' of {1} available to purchase! Only {2} available to buy!", ItemQuantity, definition.GetDisplayName(), marketItem.Quantity);
                    EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Buy aborted by Steam Id '{0}' -- not enough stock.", SenderSteamId);
                }
                // sliding price logic goes here?
                if (EconomyConsts.PriceScaling && marketItem.SellPrice>(marketItem.BuyPrice *2)) 
                    //Check if the option to change price based on stock is on, and that the sell to player price is at least 100% larger than the buy from player price before we start
                    //sliding the price
  
                //the reduction scale probably needs tweaking? Should the price drop by a fraction proprtional to
                //the sale size ?  then every sell brings "NPC buy from player" price down.
                {
                    decimal scale = 0;
                    if (ItemPrice < (decimal)0.00002) { ItemPrice = (decimal)0.00002; } //safety limit to prevent decimal point overflow errors
                    if ((ItemPrice <= 1) && (ItemPrice > (decimal)0.00001) && (marketItem.Quantity >= 500000)) { scale = (decimal)0.6; } //40%
                    if ((ItemPrice > 1) && (ItemPrice <= 50) && (marketItem.Quantity >= 200000)) { scale = (decimal)0.85; } //15%
                    if ((ItemPrice > 50) && (ItemPrice <= 150) && (marketItem.Quantity >= 150000)) { scale = (decimal)0.9; } //10%
                    if ((ItemPrice > 150) && (ItemPrice <= 300) && (marketItem.Quantity >= 100000)) { scale = (decimal)0.95; } //5%
                    if ((ItemPrice > 300) && (ItemPrice <= 1000) && (marketItem.Quantity >= 50000)) { scale = (decimal)0.96; } //4%
                    if ((ItemPrice > 1000) && (ItemPrice <= 5000) && (marketItem.Quantity >= 10000)) { scale = (decimal)0.97; } //3%
                    if ((ItemPrice > 5000) && (ItemPrice <= 15000) && (marketItem.Quantity >= 5000)) { scale = (decimal)0.98; }// 2%
                    if ((ItemPrice > 15000) && (marketItem.Quantity >= 500)) { scale = (decimal)0.99; } //1%
                    marketItem.BuyPrice = (marketItem.BuyPrice * scale);

                }
                return;
            }
            else if (FindOnMarket)
            {
                // TODO: Here we find the best offer on the zone market

                return;
            }
            else
            {
                // is it a player then?             
                if (accountToSell.SteamId == buyingPlayer.SteamUserId)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you cannot buy from yourself!");
                    return;
                }

                // check if selling player is online and in range?
                var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(accountToSell.SteamId);

                if (EconomyScript.Instance.ServerConfig.LimitedRange && !Support.RangeCheck(buyingPlayer, payingPlayer))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you are not in range of that player!");
                    return;
                }

                if (payingPlayer == null)
                {
                    // TODO: other player offline.

                }
                else
                {
                    // TODO: other player is online.

                }
            }

            // this is a fall through from the above conditions not yet complete.
            MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Not yet complete.");
        }
    }
}
