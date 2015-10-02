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

        //[ProtoMember(9)]
        //public string zone; //used to identify market we are selling to ??

        public static void SendMessage(string toUserName, decimal itemQuantity, string itemTypeId, string itemSubTypeName, decimal itemPrice, bool useBankBuyPrice, bool sellToMerchant, bool offerToMarket)
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { ToUserName = toUserName, ItemQuantity = itemQuantity, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemPrice = itemPrice, UseBankBuyPrice = useBankBuyPrice, SellToMerchant = sellToMerchant, OfferToMarket = offerToMarket });
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
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you specified doesn't exist!");
                return;
            }

            // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
            if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (ItemQuantity != Math.Truncate(ItemQuantity))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You must provide a whole number for the quantity to sell that item.");
                    return;
                }
                //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
            }

            if (ItemQuantity <= 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You must provide a valid quantity to sell.");
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
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, player does not exist or have an account!");
                return;
            }

            var marketItem = EconomyScript.Instance.Data.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
            if (marketItem == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the items you are trying to sell doesn't have a market entry!");
                // TODO: in reality, this item needs not just to have an entry created, but a value applied also. It's the value that is more important.
                return;
            }

            if (marketItem.IsBlacklisted)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you tried to sell is blacklisted on this server.");
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
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are dead. You cannot trade while dead.");
                return;
            }

            // TODO: is a null check adaqaute?, or do we need to check for IsDead?
            // I don't think the chat console is accessible during respawn, only immediately after death.
            // Is it valid to be able to trade when freshly dead?
            //var identities = new List<IMyIdentity>();
            //MyAPIGateway.Players.GetAllIdentites(identities, ident => ident.PlayerId == payingPlayer.PlayerID);
            //var identity = identities.FirstOrDefault();
            //MyAPIGateway.Utilities.ShowMessage("CHECK", "Is Dead: {0}", identity.IsDead);

            //if (identities.FirstOrDefault().IsDead)
            //{
            //    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You are dead. You cannot trade while dead.");
            //    return;
            //}

            var inventoryOwnwer = (IMyInventoryOwner)character;
            var inventory = (Sandbox.ModAPI.IMyInventory)inventoryOwnwer.GetInventory(0);
            MyFixedPoint amount = (MyFixedPoint)ItemQuantity;

            if (!inventory.ContainItems(amount, content))
            {
                // Insufficient items in inventory.
                // TODO: use of content.GetDisplayName() isn't localized here.
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You don't have {0} of '{1}' to sell.", ItemQuantity, content.GetDisplayName());
                return;
            }

            if (UseBankBuyPrice)
                // The player is selling, but the *Market* will *buy* it from the player at this price.
                ItemPrice = marketItem.BuyPrice * ItemQuantity;

            var accountToSell = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
            var transactionAmount = ItemPrice * ItemQuantity;

            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            if (SellToMerchant)
            {
                // here we look up item price and transfer items and money as appropriate
                inventory.RemoveItemsOfType(amount, content);

                account.BankBalance -= transactionAmount;
                account.Date = DateTime.Now;

                accountToSell.BankBalance += transactionAmount;
                accountToSell.Date = DateTime.Now;

                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Transaction complete for {0}", transactionAmount);
                return;
            }
            else if (OfferToMarket)
            {
                // Here we post offer to appropriate zone market
            }
            else
            {
                // TODO: check if paying player is online?

                // is it a player then?             
            }

            MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Not yet complete.");


            /*
            //  old code to be disemboweled later

            // It needs to first check the player has enough to cover his payment
            if (TransactionAmount <= accountToSpend.BankBalance || payingPlayer.IsAdmin())
            // do we have enough or are we admin so it doesnt matter
            //*      if true, 
            {
                // it needs to check the person being paid has an account record, 
                var account = EconomyScript.Instance.BankConfigData.FindAccount(ToUserName);

                //*               if true - it will always be true if real as it would have created it on join anyway

                //*               if false -  then they were never on this server anyway or seen would have added them already
                //*                         display an error message player not found
                if (account == null)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry, player does not exist or have an account!");
                    return;
                }
                //*               if true, { flag hasaccount bool true }

                if (account.SteamId == payingPlayer.SteamUserId)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry, you cannot pay yourself!");
                    return;
                }

                //*          if hasaccount bool true   

                // is there a modify property to save the need to remove then re-add? 
                // admins can give or take money, normal players can only give money so convert negative to positive
                // here we add the players bank record again with the updated balance minus what they spent
                accountToSpend.BankBalance -= TransactionAmount;
                accountToSpend.Date = DateTime.Now;

                // here we retrive the target player steam id and balance
                // here we write it back to our bank ledger file
                account.BankBalance += TransactionAmount;
                account.Date = DateTime.Now;

                // if this works this is a very sexy way to work with our file
                // testing: it does indeed work, if i was a teenager id probably need to change my underwear at this point

                // This notifies receiving player that they were paid and/or any message the sending player wrote
                // which needs to not send if the player isnt online - pity ive no idea how to write to the faction chat system
                // be a good place to send the player a faction message as it would work even if they were offline..
                MessageClientTextMessage.SendMessage(account.SteamId, "PAY",
                    string.Format("{0}, {1} just paid you {2} for {3}", account.NickName, SenderDisplayName, TransactionAmount, Reason));

                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY",
                    string.Format("You just paid {0}, {1} for {2}", account.NickName, TransactionAmount, Reason));

                EconomyScript.Instance.ServerLogger.Write("Pay: '{0}' sent {1} to '{2}'", accountToSpend.NickName, TransactionAmount, ToUserName);


                //*      if false/otherwise throw error you dont have enough money
            }
            else
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry you can't afford that much!");
            } */
        }

        private void ReturnInventoryItems()
        {
            // TODO: return the items to the seller inventory.
        }
    }
}
