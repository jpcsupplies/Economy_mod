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
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

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
                accountToSell = EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == EconomyConsts.NpcMerchantId);
            else
                accountToSell = AccountManager.FindAccount(FromUserName);

            if (accountToSell == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, player does not exist or have an account!");
                return;
            }

            var marketItem = EconomyScript.Instance.Data.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
            if (marketItem == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the items you are trying to buy doesn't have a market entry!");
                // TODO: in reality, this item needs not just to have an entry created, but a value applied also. It's the value that is more important.
                return;
            }

            if (marketItem.IsBlacklisted)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, the item you tried to buy is blacklisted on this server.");
                return;
            }

            // Verify that the items are in the player inventory.
            // TODO: later check trade block, cockpit inventory, cockpit ship inventory, inventory of targeted cube.

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

            if (UseBankSellPrice)
                // The player is buying, but the *Market* will *sell* it to the player at this price.
                ItemPrice = marketItem.SellPrice;

            var accountToBuy = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
            var transactionAmount = ItemPrice * ItemQuantity;

            // need fix negative amounts before checking if the player can afford it.
            if (!buyingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            // TODO: admin check on ability to afford it?
            if (accountToBuy.BankBalance < transactionAmount)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Sorry, you cannot afford {0}!", transactionAmount);
                return;
            }

            if (BuyFromMerchant)
            {
                // here we look up item price and transfer items and money as appropriate
                marketItem.Quantity -= ItemQuantity; // reduce Market content.

                var inventoryOwnwer = (IMyInventoryOwner)character;
                var inventory = (Sandbox.ModAPI.IMyInventory)inventoryOwnwer.GetInventory(0);
                MyFixedPoint amount = (MyFixedPoint)ItemQuantity;
                if (!InventoryAdd(inventory, amount, definition.Id))
                {
                    InventoryDrop(inventory, amount, definition.Id, (IMyEntity)character);
                }

                accountToSell.BankBalance += transactionAmount;
                accountToSell.Date = DateTime.Now;

                accountToBuy.BankBalance -= transactionAmount;
                accountToBuy.Date = DateTime.Now;

                MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "{1} units bought. Transaction complete for {0}", transactionAmount, ItemQuantity);
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

                // check if paying player is online?
                var player = MyAPIGateway.Players.FindPlayerBySteamId(accountToSell.SteamId);
                if (player == null)
                {
                    // TODO: other player offline.

                }
                else
                {
                    // TODO: other player is online.

                }
            }

            MessageClientTextMessage.SendMessage(SenderSteamId, "BUY", "Not yet complete.");
        }

        private bool InventoryAdd(Sandbox.ModAPI.IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, Content = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, (MyObjectBuilder_PhysicalObject)inventoryItem.Content, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }

        private void InventoryDrop(Sandbox.ModAPI.IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId, IMyEntity entity)
        {
            Vector3D position;

            if (entity is IMyCharacter)
                position = entity.WorldMatrix.Translation + entity.WorldMatrix.Forward * 1.5f + entity.WorldMatrix.Up * 1.5f; // Spawn item 1.5m in front of player.
            else
                position = entity.WorldMatrix.Translation + entity.WorldMatrix.Forward * 1.5f; // Spawn item 1.5m in front of player in cockpit.

            MyObjectBuilder_FloatingObject floatingBuilder = new MyObjectBuilder_FloatingObject();
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
            floatingBuilder.Item = new MyObjectBuilder_InventoryItem() { Amount = amount, Content = content };
            floatingBuilder.PersistentFlags = MyPersistentEntityFlags2.InScene; // Very important

            floatingBuilder.PositionAndOrientation = new MyPositionAndOrientation()
            {
                Position = position,
                Forward = entity.WorldMatrix.Forward.ToSerializableVector3(),
                Up = entity.WorldMatrix.Up.ToSerializableVector3(),
            };

            floatingBuilder.CreateAndSyncEntity();
        }
    }
}
