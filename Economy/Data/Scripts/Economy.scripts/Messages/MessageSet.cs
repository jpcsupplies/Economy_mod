namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using Economy.scripts;
    using EconStructures;
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

        public static void SendMessage(ulong marketId, string itemTypeId, string itemSubTypeName, SetMarketItemType setType, decimal itemQuantity, decimal itemBuyPrice, decimal itemSellPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = setType, ItemQuantity = itemQuantity, ItemBuyPrice = itemBuyPrice, ItemSellPrice = itemSellPrice });
        }

        public static void SendMessageBuy(ulong marketId, string itemTypeId, string itemSubTypeName, decimal itemBuyPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.BuyPrice, ItemBuyPrice = itemBuyPrice });
        }

        public static void SendMessageSell(ulong marketId, string itemTypeId, string itemSubTypeName, decimal itemSellPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.SellPrice, ItemSellPrice = itemSellPrice });
        }

        public static void SendMessageQuantity(ulong marketId, string itemTypeId, string itemSubTypeName, decimal itemQuantity)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.Quantity, ItemQuantity = itemQuantity });
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
                EconomyScript.Instance.ServerLogger.WriteWarning("A Player without Admin \"{0}\" {1} attempted to set Default Market characteristics of item {2}/{3} to Quantity={4}.", SenderDisplayName, SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity);
                return;
            }

            // Only Player can change their own Market prices.
            if (SenderSteamId != MarketId && MarketId != EconomyConsts.NpcMerchantId)
            {
                EconomyScript.Instance.ServerLogger.WriteWarning("A Player \"{0}\" {1} attempted to set another Market characteristics of item {2}/{3} to Quantity={4}.", SenderDisplayName, SenderSteamId, ItemTypeId, ItemSubTypeName, ItemQuantity);
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

            MarketItemStruct configItem = null;
            if (player.IsAdmin() && MarketId == EconomyConsts.NpcMerchantId)
                configItem = EconomyScript.Instance.Config.DefaultPrices.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);

            var msg = new StringBuilder();
            msg.AppendFormat("You just set '{0}'", definition.GetDisplayName());

            if (SetType.HasFlag(SetMarketItemType.Quantity))
            {
                marketItem.Quantity = ItemQuantity;
                msg.AppendFormat(", stock on hand to {0} units", ItemQuantity);
            }

            // Validation to prevent admins setting prices too low for items.
            if (SetType.HasFlag(SetMarketItemType.BuyPrice))
            {
                if (ItemBuyPrice >= 0)
                {
                    marketItem.BuyPrice = ItemBuyPrice;
                    msg.AppendFormat(", buy price to {0}", ItemBuyPrice);

                    if (configItem != null)
                    {
                        configItem.BuyPrice = ItemBuyPrice;
                        msg.AppendFormat("; config updated.");
                    }
                }
                else
                    msg.AppendFormat(", could not set buy price to less than 0.");
            }

            // Validation to prevent admins setting prices too low for items.
            if (SetType.HasFlag(SetMarketItemType.SellPrice))
            {
                if (ItemSellPrice >= 0)
                {
                    marketItem.SellPrice = ItemSellPrice;
                    msg.AppendFormat(", sell price to {0}", ItemSellPrice);

                    if (configItem != null)
                    {
                        configItem.SellPrice = ItemSellPrice;
                        msg.AppendFormat("; config updated.");
                    }
                }
                else
                    msg.AppendFormat(", could not set sell price to less than 0.");
            }

            if (SetType.HasFlag(SetMarketItemType.Blacklisted))
            {
                marketItem.IsBlacklisted = !marketItem.IsBlacklisted;
                msg.AppendFormat(", blacklist to {0}", marketItem.IsBlacklisted ? "On" : "Off");

                if (configItem != null)
                {
                    configItem.IsBlacklisted = marketItem.IsBlacklisted;
                    msg.AppendFormat("; config updated.");
                }
            }

            MessageClientTextMessage.SendMessage(SenderSteamId, "SET", msg.ToString());
        }
    }
}
