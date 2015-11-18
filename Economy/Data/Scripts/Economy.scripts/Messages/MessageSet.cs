namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using Economy.scripts;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.ObjectBuilders;

    /// <summary>
    /// this is to do the actual work of setting new prices and stock levels.
    /// </summary>
    [ProtoContract]
    public class MessageSet : MessageBase
    {
        /// <summary>
        /// The market to set prices in.
        /// </summary>
        [ProtoMember(1)]
        public ulong MarketId;

        /// <summary>
        /// item id we are setting
        /// </summary>
        [ProtoMember(2)]
        public string ItemTypeId;

        /// <summary>
        /// item subid we are setting
        /// </summary>
        [ProtoMember(3)]
        public string ItemSubTypeName;

        [ProtoMember(4)]
        public SetMarketItemType SetType;

        /// <summary>
        /// qty of item
        /// </summary>
        [ProtoMember(5)]
        public decimal ItemQuantity;

        /// <summary>
        /// unit price to buy item at.
        /// </summary>
        [ProtoMember(6)]
        public decimal ItemBuyPrice;

        /// <summary>
        /// unit price to sell item at.
        /// </summary>
        [ProtoMember(7)]
        public decimal ItemSellPrice;

        /// <summary>
        /// Blacklist item.
        /// </summary>
        [ProtoMember(8)]
        public bool BlackListed;

        public static void SendMessage(ulong marketId, string itemTypeId, string itemSubTypeName, SetMarketItemType setType, decimal itemQuantity, decimal itemBuyPrice, decimal itemSellPrice, bool blackListed)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = setType, ItemQuantity = itemQuantity, ItemBuyPrice = itemBuyPrice, ItemSellPrice = itemSellPrice, BlackListed = blackListed });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            // Only Admin can change Npc Market prices.
            if (!player.IsAdmin() && MarketId == EconomyConsts.NpcMerchantId)
            {
                EconomyScript.Instance.ServerLogger.Write("A Player without Admin \"{0}\" {1} attempted to set Default Market characteristics of item {2}/{3} to Quantity={4}.", SenderDisplayName, SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity);
                return;
            }

            // Only Player can change their own Market prices.
            if (SenderSteamId != MarketId && MarketId != EconomyConsts.NpcMerchantId)
            {
                EconomyScript.Instance.ServerLogger.Write("A Player \"{0}\" {1} attempted to set another Market characteristics of item {2}/{3} to Quantity={4}.", SenderDisplayName, SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity);
                return;
            }

            // TODO: do we check range to market?

            MyPhysicalItemDefinition definition = null;
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
            {
                var id = new MyDefinitionId(result, ItemSubTypeName);
                MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
            }

            if (definition == null)
            {
                // Passing bad data?
                MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "Sorry, the item you specified doesn't exist!");
                return;
            }

            if (SetType.HasFlag(SetMarketItemType.Quantity))
            {
                // Do a floating point check on the item item. Tools and components cannot have decimals. They must be whole numbers.
                if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
                {
                    if (ItemQuantity != Math.Truncate(ItemQuantity))
                    {
                        MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "You must provide a whole number for the quantity of that item.");
                        return;
                    }
                    //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
                }

                if (ItemQuantity <= 0)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "Invalid quantity spectified");
                    return;
                }
            }

            // Find the specified market.
            var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.MarketId == MarketId);
            if (market == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "Sorry, the market you are accessing does not exist!");
                return;
            }

            var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
            if (marketItem == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "Sorry, the items you are trying to set doesn't have a market entry!");
                // In reality, this shouldn't happen as all markets have their items synced up on start up of the mod.
                return;
            }

            var msg = new StringBuilder();
            msg.AppendFormat("You just set {0}", definition.GetDisplayName());

            if (SetType.HasFlag(SetMarketItemType.Quantity))
            {
                marketItem.Quantity = ItemQuantity;
                msg.AppendFormat(", stock on hand to {0} units", ItemQuantity);
            }

            if (SetType.HasFlag(SetMarketItemType.Prices))
            {   //using positive numbers as both a flag, and to prevent admins setting items to pay you when you buy them or vice versa.. 2 for 1 exploit protection and mode detect! 
                if (ItemBuyPrice >= 0) { marketItem.BuyPrice = ItemBuyPrice; msg.AppendFormat(", buy price to {0}", ItemBuyPrice); }
                if (ItemSellPrice >= 0) { marketItem.SellPrice = ItemSellPrice; msg.AppendFormat(", sell price to {1}", ItemSellPrice); }
            }

            if (SetType.HasFlag(SetMarketItemType.Blacklisted))
            {
                //marketItem.IsBlacklisted = BlackListed;
                marketItem.IsBlacklisted = !marketItem.IsBlacklisted;
                msg.AppendFormat(", blacklist to {0}, ", marketItem.IsBlacklisted ? "On" : "Off");
            }

            MessageClientTextMessage.SendMessage(SenderSteamId, "SET", msg.ToString());
        }
    }
}
