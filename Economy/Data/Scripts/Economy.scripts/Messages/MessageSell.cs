namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using Economy.scripts;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Interfaces;
    using VRage;
    using VRage.ObjectBuilders;

    /// <summary>
    /// this is to do the actual work of moving the goods checks need to occur before this
    /// </summary>
    [ProtoContract]
    public class MessageSell : MessageBase
    {
        /// <summary>
        /// person, NPC, offer or faction to sell to
        /// </summary>
        [ProtoMember(1)]
        public string ToUserName;

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
        /// Use the Current Buy price to sell it at. The Player 
        /// will not have access to this information without fetching it first. This saves us the trouble.
        /// </summary>
        [ProtoMember(6)]
        public bool UseBankBuyPrice;

        /// <summary>
        /// We are selling to the Merchant.
        /// </summary>
        [ProtoMember(7)]
        public bool SellToMerchant;

        /// <summary>
        /// The Item is been put onto the market.
        /// </summary>
        [ProtoMember(8)]
        public bool OfferToMarket;

        [ProtoMember(9)]
        public bool Buying;

        //[ProtoMember(10)]
        //public string zone; //used to identify market we are selling to ??

        public static void SendMessage(string toUserName, decimal itemQuantity, string itemTypeId, string itemSubTypeName, decimal itemPrice, bool useBankBuyPrice, bool sellToMerchant, bool offerToMarket, bool buying)
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { ToUserName = toUserName, ItemQuantity = itemQuantity, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemPrice = itemPrice, UseBankBuyPrice = useBankBuyPrice, SellToMerchant = sellToMerchant, OfferToMarket = offerToMarket, Buying=buying });
        }

        public override void ProcessClient()
        {
            // never processed on client
            //will we need this to remove players inventory items?
        }

        public override void ProcessServer()
        {
            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

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
                MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "Sorry, the item you specified doesn't exist!");
                return;
            }

            // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
            if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (ItemQuantity != Math.Truncate(ItemQuantity))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "You must provide a whole number for the quantity of that item.");
                    return;
                }
                //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
            }

            if (ItemQuantity <= 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "You must provide a valid quantity.");
                return;
            }

            // Who are we selling to?
            BankAccountStruct account;
            if (SellToMerchant)
                account = EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == EconomyConsts.NpcMerchantId);
            else
                account = AccountManager.FindAccount(ToUserName);

            if (account == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "Sorry, player does not exist or have an account!");
                return;
            }

            var marketItem = EconomyScript.Instance.Data.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
            if (marketItem == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the items you are trying to trade doesn't have a market entry!");
                // TODO: in reality, this item needs not just to have an entry created, but a value applied also. It's the value that is more important.
                return;
            }

            if (marketItem.IsBlacklisted)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "Sorry, the item you tried to trade is blacklisted on this server.");
                return;
            }

            // Verify that the items are in the player inventory.
            // TODO: later check trade block, cockpit inventory, cockpit ship inventory, inventory of targeted cube.

            var definitionId = new MyDefinitionId(definition.Id.TypeId, definition.Id.SubtypeName);
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            // Get the player's inventory, regardless of if they are in a ship, or a remote control cube.
            var character = payingPlayer.GetCharacter();
            // TODO: do players in Cryochambers count as a valid trading partner? They should be alive, but the connected player may be offline.
            // I think we'll have to do lower level checks to see if a physical player is Online.
            if (character == null)
            {
                // Player has no body. Could mean they are dead.
                // Either way, there is no inventory.
                MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "You are dead. You cannot trade while dead.");
                return;
            }

            // TODO: is a null check adaqaute?, or do we need to check for IsDead?
            // I don't think the chat console is accessible during respawn, only immediately after death.
            // Is it valid to be able to trade when freshly dead?
            var identity = payingPlayer.Identity();
            MyAPIGateway.Utilities.ShowMessage("CHECK", "Is Dead: {0}", identity.IsDead);

            //if (identity.IsDead)
            //{
            //    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are dead. You cannot trade while dead.");
            //    return;
            //}

            var inventoryOwnwer = (IMyInventoryOwner)character;
            var inventory = (Sandbox.ModAPI.IMyInventory)inventoryOwnwer.GetInventory(0);
            MyFixedPoint amount = (MyFixedPoint)ItemQuantity;

                if (!inventory.ContainItems(amount, content) && !Buying)
                {
                    var storedAmount = inventory.GetItemAmount(content.GetObjectId());
                    // Insufficient items in inventory.
                    // TODO: use of content.GetDisplayName() isn't localized here.
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You don't have {0} of '{1}' to sell. You have {2} in your inventory.", ItemQuantity, content.GetDisplayName(), storedAmount);
                    return;
                }
            
            if (UseBankBuyPrice)
                // The player is buying, use sell price
                if (Buying) { ItemPrice = marketItem.SellPrice; }
                // The player is selling, but the *Market* will *buy* it from the player at this price.
                else { ItemPrice = marketItem.BuyPrice; }

            var accountToSell = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
            var transactionAmount = ItemPrice * ItemQuantity;

            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            if (SellToMerchant)
            {
                // here we look up item price and transfer items and money as appropriate
                if (Buying) {
                    inventory.AddItems(amount, content);
                    marketItem.Quantity -= ItemQuantity; // reduce Market content.
                    account.BankBalance += transactionAmount;
                    accountToSell.BankBalance -= transactionAmount;     
                    MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "{1} units purchased. Transaction complete for {0}", transactionAmount, ItemQuantity);
                }
                else
                {
                    inventory.RemoveItemsOfType(amount, content);
                    marketItem.Quantity += ItemQuantity; // increment Market content.
                    account.BankBalance -= transactionAmount;
                    accountToSell.BankBalance += transactionAmount;
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "{1} units sold. Transaction complete for {0}", transactionAmount, ItemQuantity);
                }
                account.Date = DateTime.Now;
                accountToSell.Date = DateTime.Now;

                return;
            }
            else if (OfferToMarket)
            {
                // TODO: Here we post offer to appropriate zone market

                return;
            }
            else
            {
                // is it a player then?             
                if (account.SteamId == payingPlayer.SteamUserId)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "Sorry, you cannot TRADE with yourself!");
                    return;
                }

                // check if paying player is online?
                var player = MyAPIGateway.Players.FindPlayerBySteamId(account.SteamId);
                if (player == null)
                {
                    // TODO: other player offline.

                }
                else
                {
                    // TODO: other player is online.

                }
            }

            MessageClientTextMessage.SendMessage(SenderSteamId, "TRADE", "Not yet complete.");



        }

        private void ReturnInventoryItems()
        {
            // TODO: return the items to the seller inventory.
        }
    }
}
