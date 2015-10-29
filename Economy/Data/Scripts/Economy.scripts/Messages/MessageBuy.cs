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
    public class MessageBuy : MessageBase
    {
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
            // Get player steam ID
            var buyingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

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
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you specified doesn't exist!");
                return;
            }

            // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
            if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (ItemQuantity != Math.Truncate(ItemQuantity))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You must provide a whole number for the quantity to buy that item.");
                    return;
                }
                //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
            }

            if (ItemQuantity <= 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You must provide a valid quantity to buy.");
                return;
            }

            // Who are we buying to?
            BankAccountStruct accountToSell;
            if (BuyFromMerchant)
                accountToSell = AccountManager.FindAccount(EconomyConsts.NpcMerchantId);
            else
                accountToSell = AccountManager.FindAccount(FromUserName);

            if (accountToSell == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, player does not exist or have an account!");
                return;
            }

            if (MarketManager.IsItemBlacklistedOnServer(ItemTypeId, ItemSubTypeName))
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you tried to buy is blacklisted on this server.");
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
                    return;
                }

                // TODO: find market with best Sell price that isn't blacklisted.

                var market = markets.FirstOrDefault();
                if (market == null)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the market you are accessing does not exist!");
                    return;
                }

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
            if (accountToBuy.BankBalance < transactionAmount)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you cannot afford {0}!", transactionAmount);
                return;
            }

            if (BuyFromMerchant) // and supply is not exhausted, or unlimited mode is not on.   
                                 //This is a quick fix, ideally it should do a partial buy of what is left and post a buy offer for remainder
            {
                // here we look up item price and transfer items and money as appropriate
                if (marketItem.Quantity >= ItemQuantity || !EconomyConsts.LimitedSupply)
                {
                    marketItem.Quantity -= ItemQuantity; // reduce Market content.

                    var inventory = character.GetPlayerInventory();
                    MyFixedPoint amount = (MyFixedPoint)ItemQuantity;
                    if (!Support.InventoryAdd(inventory, amount, definition.Id))
                    {
                        Support.InventoryDrop((IMyEntity)character, amount, definition.Id);
                    }

                    //EconomyConsts.LimitedSupply

                    accountToSell.BankBalance += transactionAmount;
                    accountToSell.Date = DateTime.Now;

                    accountToBuy.BankBalance -= transactionAmount;
                    accountToBuy.Date = DateTime.Now;
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "You just purchased {1} '{2}' for {0}", transactionAmount, ItemQuantity, definition.GetDisplayName());
                }
                else { MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "There isn't '{0}' of {1} available to purchase! Only {2} available to buy!", ItemQuantity, definition.GetDisplayName(), marketItem.Quantity); }

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

                if (EconomyConsts.LimitedRange && !Support.RangeCheck(buyingPlayer, payingPlayer))
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
