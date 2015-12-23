namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Economy.scripts.EconStructures;
    using Messages;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.ModAPI;
    using VRageMath;

    public static class MarketManager
    {
        #region Market helpers

        /// <summary>
        /// Check that all current Definitions are in the EconContentStruct.
        /// </summary>
        /// <param name="marketItems"></param>
        public static void SyncMarketItems(ref List<MarketItemStruct> marketItems)
        {
            // Combination of Components.sbc, PhysicalItems.sbc, and AmmoMagazines.sbc files.
            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach (var item in physicalItems)
            {
                if (item.Public)
                {
                    // TypeId and SubtypeName are both Case sensitive. Do not Ignore case.
                    if (!marketItems.Any(e => e.TypeId.Equals(item.Id.TypeId.ToString()) && e.SubtypeName.Equals(item.Id.SubtypeName)))
                    {
                        // Need to add new items as Blacklisted.
                        marketItems.Add(new MarketItemStruct { TypeId = item.Id.TypeId.ToString(), SubtypeName = item.Id.SubtypeName, BuyPrice = 1, SellPrice = 1, IsBlacklisted = true });
                        EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding new item: {0} {1}.", item.Id.TypeId.ToString(), item.Id.SubtypeName);
                    }
                }
            }

            // TODO: get Gas Property Items.  MyObjectBuilder_GasProperties
            //var gasItems = MyDefinitionManager.Static.GetGas???;


            // Bottles...
            // MyDefinitionId = MyOxygenContainerDefinition.StoredGasId;
            // maxVolume = MyOxygenContainerDefinition.Capacity;

            // Tanks... To be done later. it should work the same as bottles though.
            // MyDefinitionId = MyGasTankDefinition.StoredGasId;
        }

        public static void CreateSellOrder(ulong sellerId, string goodsTypeId, string goodsSubtypeName, decimal quantity, decimal price)
        {
            var order = new OrderBookStruct
            {
                Created = DateTime.Now,
                TraderId = sellerId,
                TypeId = goodsTypeId,
                SubtypeName = goodsSubtypeName,
                TradeState = TradeState.Sell,
                Quantity = quantity,
                Price = price,
                OptionalId = ""
            };
            EconomyScript.Instance.Data.OrderBook.Add(order);
        }

        public static void CreateTradeOffer(ulong sellerId, string goodsTypeId, string goodsSubtypeName, decimal quantity, decimal price, ulong targetPlayer)
        {
            var order = new OrderBookStruct
            {
                Created = DateTime.Now,
                TraderId = sellerId,
                TypeId = goodsTypeId,
                SubtypeName = goodsSubtypeName,
                TradeState = TradeState.SellDirectPlayer,
                Quantity = quantity,
                Price = price,
                OptionalId = targetPlayer.ToString()
            };

            EconomyScript.Instance.Data.OrderBook.Add(order);
        }

        public static void CreateStockHeld(ulong sellerId, string goodsTypeId, string goodsSubtypeName, decimal quantity, decimal price)
        {
            var order = new OrderBookStruct
            {
                Created = DateTime.Now,
                TraderId = sellerId,
                TypeId = goodsTypeId,
                SubtypeName = goodsSubtypeName,
                TradeState = TradeState.Holding,
                Quantity = quantity,
                Price = price,
                OptionalId = ""
            };

            EconomyScript.Instance.Data.OrderBook.Add(order);
        }

        public static void CheckTradeTimeouts()
        {
            var processingTime = DateTime.Now;

            if (EconomyScript.Instance.Config == null)
                return;

            var expiration = EconomyScript.Instance.Config.TradeTimeout;

            if (EconomyScript.Instance.Data == null || EconomyScript.Instance.Data.OrderBook.Count == 0)
                return;

            var cancellations = EconomyScript.Instance.Data.OrderBook.Where(order => processingTime - order.Created > expiration
            && (order.TradeState == TradeState.Sell || order.TradeState == TradeState.SellDirectPlayer)).ToArray();
            if (cancellations.Length == 0)
                return;

            EconomyScript.Instance.ServerLogger.WriteVerbose("CheckTradeTimeouts: {0} cancellations", cancellations.Length);

            foreach (var order in cancellations)
            {
                switch (order.TradeState)
                {
                    case TradeState.Sell:
                        // Change the TradeState first, to prevent other calls into this.
                        order.TradeState = TradeState.SellTimedout;
                        MessageClientTextMessage.SendMessage(order.TraderId, "SELL", "Your offer has timed out. Type '/sell collect' to collect your goods.");
                        break;

                    case TradeState.SellDirectPlayer:
                        // Change the TradeState first, to prevent other calls into this.
                        order.TradeState = TradeState.SellTimedout;
                        MessageClientTextMessage.SendMessage(order.TraderId, "SELL", "Your offer has timed out. Type '/sell collect' to collect your goods.");

                        ulong tradePartner;
                        if (ulong.TryParse(order.OptionalId, out tradePartner))
                        {
                            var sellingAccount = EconomyScript.Instance.Data.Accounts.First(a => a.SteamId == order.TraderId);
                            MessageClientTextMessage.SendMessage(tradePartner, "SELL", "The offer from {0} has now expired.", sellingAccount.NickName);
                        }
                        break;
                }
            }
        }

        public static List<MarketStruct> FindMarketsFromLocation(Vector3D position)
        {
            var list = new List<MarketStruct>();
            foreach (var market in EconomyScript.Instance.Data.Markets)
            {
                switch (market.MarketZoneType)
                {
                    case MarketZoneType.EntitySphere:
                        if (market.EntityId == 0 || !MyAPIGateway.Entities.EntityExists(market.EntityId))
                            continue;
                        if (!market.MarketZoneSphere.HasValue)
                            continue;
                        IMyEntity entity;
                        if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                            continue;
                        if (entity.Closed || entity.MarkedForClose)
                            continue;
                        var sphere = new BoundingSphereD(entity.WorldMatrix.Translation, market.MarketZoneSphere.Value.Radius);
                        if (sphere.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedSphere:
                        if (!market.MarketZoneSphere.HasValue)
                            continue;
                        if (market.MarketZoneSphere.Value.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedBox:
                        if (!market.MarketZoneBox.HasValue)
                            continue;
                        if (market.MarketZoneBox.Value.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;
                }
            }
            return list;
        }

        public static bool IsItemBlacklistedOnServer(string itemTypeId, string itemSubTypeName)
        {
            var marketItem = EconomyScript.Instance.Config.DefaultPrices.FirstOrDefault(e => e.TypeId == itemTypeId && e.SubtypeName == itemSubTypeName);
            if (marketItem == null)
                return true; // does't exist. Same as been blacklisted.

            return marketItem.IsBlacklisted;
        }

        //private static MyDefinitionId? GetDefinitionId(MarketItemStruct marketItem)
        //{
        //    MyObjectBuilderType result;
        //    if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
        //    {
        //        return new MyDefinitionId(result, marketItem.SubtypeName);
        //    }

        //    return null;
        //}

        #endregion
    }
}
