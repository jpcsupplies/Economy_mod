namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using Economy.scripts;
    using EconStructures;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    /// <summary>
    /// this is to do the actual work of setting new prices and stock levels.
    /// </summary>
    [ProtoContract]
    public class MessageSet : MessageBase
    {
        #region properties

        /// <summary>
        /// The market to set prices in.
        /// </summary>
        [ProtoMember(1)]
        public ulong MarketId;

        /// <summary>
        /// The market name to set prices in.
        /// </summary>
        [ProtoMember(2)]
        public string MarketZone;

        /// <summary>
        /// item id we are setting
        /// </summary>
        [ProtoMember(3)]
        public string ItemTypeId;

        /// <summary>
        /// item subid we are setting
        /// </summary>
        [ProtoMember(4)]
        public string ItemSubTypeName;

        [ProtoMember(5)]
        public SetMarketItemType SetType;

        /// <summary>
        /// qty of item
        /// </summary>
        [ProtoMember(6)]
        public decimal ItemQuantity;

        /// <summary>
        /// unit price to buy item at.
        /// </summary>
        [ProtoMember(7)]
        public decimal ItemBuyPrice;

        /// <summary>
        /// unit price to sell item at.
        /// </summary>
        [ProtoMember(8)]
        public decimal ItemSellPrice;

        #endregion

        public static void SendMessage(ulong marketId, string marketZone, string itemTypeId, string itemSubTypeName, SetMarketItemType setType, decimal itemQuantity, decimal itemBuyPrice, decimal itemSellPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, MarketZone = marketZone, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = setType, ItemQuantity = itemQuantity, ItemBuyPrice = itemBuyPrice, ItemSellPrice = itemSellPrice });
        }

        public static void SendMessageBuy(ulong marketId, string marketZone, string itemTypeId, string itemSubTypeName, decimal itemBuyPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, MarketZone = marketZone, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.BuyPrice, ItemBuyPrice = itemBuyPrice });
        }

        public static void SendMessageSell(ulong marketId, string marketZone, string itemTypeId, string itemSubTypeName, decimal itemSellPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, MarketZone = marketZone, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.SellPrice, ItemSellPrice = itemSellPrice });
        }

        public static void SendMessageQuantity(ulong marketId, string marketZone, string itemTypeId, string itemSubTypeName, decimal itemQuantity)
        {
            ConnectionHelper.SendMessageToServer(new MessageSet { MarketId = marketId, MarketZone = marketZone, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, SetType = SetMarketItemType.Quantity, ItemQuantity = itemQuantity });
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
            List<MarketStruct> markets;
            if (string.IsNullOrEmpty(MarketZone))
            {
                var character = player.GetCharacter();

                if (character == null)
                {
                    // Player has no body. Could mean they are dead.
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "There is no market at your location to set.");
                    return;
                }

                var position = ((IMyEntity)character).WorldMatrix.Translation;
                markets = MarketManager.FindMarketsFromLocation(position).Where(m => m.MarketId == MarketId).ToList();
            }
            else
            {
                markets = EconomyScript.Instance.Data.Markets.Where(m => m.MarketId == MarketId && (MarketZone == "*" || m.DisplayName.Equals(MarketZone, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }

            if (markets.Count == 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SET", "Sorry, you are not near any markets currently or the market does not exist!");
                return;
            }

            var msg = new StringBuilder();

            msg.AppendFormat("Applying changes to : '{0}' {1}/{2}\r\n\r\n", definition.GetDisplayName(), ItemTypeId, ItemSubTypeName);

            foreach (var market in markets)
            {
                msg.AppendFormat("Market: '{0}'\r\n", market.DisplayName);

                var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                if (marketItem == null)
                {
                    msg.AppendLine("Sorry, the items you are trying to set doesn't have a market entry!");
                    // In reality, this shouldn't happen as all markets have their items synced up on start up of the mod.
                    continue;
                }

                MarketItemStruct configItem = null;
                if (player.IsAdmin() && MarketId == EconomyConsts.NpcMerchantId)
                    configItem = EconomyScript.Instance.ServerConfig.DefaultPrices.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);

                if (SetType.HasFlag(SetMarketItemType.Quantity))
                {
                    marketItem.Quantity = ItemQuantity;
                    msg.AppendFormat("Stock on hand to {0} units", ItemQuantity);
                }

                // Validation to prevent admins setting prices too low for items.
                if (SetType.HasFlag(SetMarketItemType.BuyPrice))
                {
                    if (ItemBuyPrice >= 0)
                    {
                        marketItem.BuyPrice = ItemBuyPrice;
                        msg.AppendFormat("Buy price to {0}", ItemBuyPrice);

                        if (configItem != null)
                        {
                            configItem.BuyPrice = ItemBuyPrice;
                            msg.AppendFormat("; config updated.");
                        }
                    }
                    else
                        msg.AppendFormat("Could not set buy price to less than 0.");
                }

                // Validation to prevent admins setting prices too low for items.
                if (SetType.HasFlag(SetMarketItemType.SellPrice))
                {
                    if (ItemSellPrice >= 0)
                    {
                        marketItem.SellPrice = ItemSellPrice;
                        msg.AppendFormat("Sell price to {0}", ItemSellPrice);

                        if (configItem != null)
                        {
                            configItem.SellPrice = ItemSellPrice;
                            msg.AppendFormat("; config updated.");
                        }
                    }
                    else
                        msg.AppendFormat("Could not set sell price to less than 0.");
                }

                if (SetType.HasFlag(SetMarketItemType.Blacklisted))
                {
                    marketItem.IsBlacklisted = !marketItem.IsBlacklisted;
                    msg.AppendFormat("Blacklist to {0}", marketItem.IsBlacklisted ? "On" : "Off");

                    if (configItem != null)
                    {
                        configItem.IsBlacklisted = marketItem.IsBlacklisted;
                        msg.AppendFormat("; config updated.");
                    }
                }
                msg.AppendLine();
                msg.AppendLine();
            }

            MessageClientDialogMessage.SendMessage(SenderSteamId, "SET", " ", msg.ToString());
        }
    }
}
