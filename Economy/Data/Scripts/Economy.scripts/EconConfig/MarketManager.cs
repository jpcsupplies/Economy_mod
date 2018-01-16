namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Economy.scripts.EconStructures;
    using Messages;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.ModAPI;
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

            // get Gas Property Items.  MyObjectBuilder_GasProperties
            var gasItems = MyDefinitionManager.Static.GetGasDefinitions();

            foreach (var item in gasItems)
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

            // TODO: make sure buy and sell work with correct value of Gas.
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

        public static void CreateStockHeld(ulong marketId, ulong sellerId, string goodsTypeId, string goodsSubtypeName, decimal quantity, decimal price)
        {
            var order = new OrderBookStruct
            {
                Created = DateTime.Now,
                MarketId = marketId,
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

            if (EconomyScript.Instance.ServerConfig == null)
                return;

            var expiration = EconomyScript.Instance.ServerConfig.TradeTimeout;

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
                            var sellingAccount = EconomyScript.Instance.Data.Clients.FirstOrDefault(a => a.SteamId == order.TraderId);
                            // If the account is null, then the account may have been cleaned up because it hasn't been used.
                            if (sellingAccount != null)
                                MessageClientTextMessage.SendMessage(tradePartner, "SELL", "The offer from {0} has now expired.", sellingAccount.NickName);
                            else
                                MessageClientTextMessage.SendMessage(tradePartner, "SELL", "An offer has now expired.");
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Finds all available markets that are within range that can be traded with.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static List<MarketStruct> FindMarketsFromLocation(Vector3D position)
        {
            var list = new List<MarketStruct>();
            foreach (var market in EconomyScript.Instance.Data.Markets)
            {
                if (!market.Open)
                    continue;

                switch (market.MarketZoneType)
                {
                    case MarketZoneType.EntitySphere:
                        if (!EconomyScript.Instance.ServerConfig.EnablePlayerTradezones)
                            continue;
                        if (market.EntityId == 0 || !MyAPIGateway.Entities.EntityExists(market.EntityId))
                            continue;
                        IMyEntity entity;
                        if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                        {
                            // Close the market, because the cube no longer exists.
                            market.Open = false;
                            continue;
                        }
                        if (entity.Closed || entity.MarkedForClose)
                        {
                            // Close the market, because the cube no longer exists.
                            market.Open = false;
                            continue;
                        }
                        IMyBeacon beacon = entity as IMyBeacon;
                        if (beacon == null)
                            continue;
                        if (!beacon.IsWorking)
                            continue;

                        // TODO: I'm not sure if these two commands will impact perfomance.

                        // player will be null if the player is not online.
                        // I'm not sure if there is a way to may a steamId to a playerId without them been online.
                        var player = MyAPIGateway.Players.FindPlayerBySteamId(market.MarketId);
                        if (player != null && beacon.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            // Close the market, because it's no longer owner by the player.
                            market.Open = false;
                            continue;
                        }

                        var sphere = new BoundingSphereD(entity.WorldMatrix.Translation, market.MarketZoneSphere?.Radius ?? 1);
                        if (sphere.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedSphere:
                        if (!EconomyScript.Instance.ServerConfig.EnableNpcTradezones)
                            continue;
                        if (market.MarketZoneSphere == null)
                            continue;
                        if (((BoundingSphereD)market.MarketZoneSphere).Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedBox:
                        if (!EconomyScript.Instance.ServerConfig.EnableNpcTradezones)
                            continue;
                        if (!market.MarketZoneBox.HasValue)
                            continue;
                        if (market.MarketZoneBox.Value.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;
                }
            }
            return list;
        }

        /// <summary>
        /// Finds all available markets that are within range that can be traded with.
        /// This is onyl called from the Client side, so any configuration must be passed in.
        /// </summary>
        public static List<MarketStruct> ClientFindMarketsFromLocation(List<MarketStruct> markets, Vector3D position,
                        bool enablePlayerTradezones, bool enableNpcTradezones)
        {
            var list = new List<MarketStruct>();
            foreach (var market in markets)
            {
                if (!market.Open)
                    continue;

                switch (market.MarketZoneType)
                {
                    case MarketZoneType.EntitySphere:
                        if (!enablePlayerTradezones)
                            continue;
                        if (market.EntityId == 0 || !MyAPIGateway.Entities.EntityExists(market.EntityId))
                            continue;
                        IMyEntity entity;
                        if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                            // Not in range of player, or no longer exists.
                            continue;
                        if (entity.Closed || entity.MarkedForClose)
                            // Not in range of player, or no longer exists.
                            continue;
                        IMyBeacon beacon = entity as IMyBeacon;
                        if (beacon == null)
                            continue;
                        if (!beacon.IsWorking)
                            continue;

                        var sphere = new BoundingSphereD(entity.WorldMatrix.Translation, market.MarketZoneSphere?.Radius ?? 1);
                        if (sphere.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedSphere:
                        if (!enableNpcTradezones)
                            continue;
                        if (market.MarketZoneSphere == null)
                            continue;
                        if (((BoundingSphereD)market.MarketZoneSphere).Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;

                    case MarketZoneType.FixedBox:
                        if (!enableNpcTradezones)
                            continue;
                        if (!market.MarketZoneBox.HasValue)
                            continue;
                        if (market.MarketZoneBox.Value.Contains(position) == ContainmentType.Contains)
                            list.Add(market);
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// Find a market of the specified name, trying exact match first, then case insensative, then partial string.
        /// </summary>
        /// <param name="marketname"></param>
        /// <returns></returns>
        public static List<MarketStruct> FindMarketsFromName(string marketname)
        {
            var list = new List<MarketStruct>();

            if (string.IsNullOrEmpty(marketname))
                return list;

            foreach (var market in EconomyScript.Instance.Data.Markets)
            {
                if (market.DisplayName == marketname)
                    list.Add(market);
            }

            if (list.Count == 0)
            {
                foreach (var market in EconomyScript.Instance.Data.Markets)
                {
                    if (market.DisplayName.Equals(marketname, StringComparison.InvariantCultureIgnoreCase))
                        list.Add(market);
                }
            }

            if (list.Count == 0)
            {
                foreach (var market in EconomyScript.Instance.Data.Markets)
                {
                    if (market.DisplayName.IndexOf(marketname, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        list.Add(market);
                }
            }

            return list;
        }

        /// <summary>
        /// Finds the closest player owned market of the specified open state.
        /// </summary>
        /// <returns></returns>
        public static MarketStruct FindClosestPlayerMarket(IMyPlayer player, bool? isOpen = null)
        {
            var character = player.Character;
            if (character == null)
            {
                // Cannot determine the player's location.
                return null;
            }

            var position = character.GetPosition();
            var markets = EconomyScript.Instance.Data.Markets.Where(m => m.MarketId == player.SteamUserId && (!isOpen.HasValue || m.Open == isOpen.Value)).ToArray();

            if (markets.Length == 0)
            {
                // no open markets found for the player;
                return null;
            }

            var list = new Dictionary<IMyTerminalBlock, double>();

            foreach (var market in markets)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                    continue;
                if (entity.Closed || entity.MarkedForClose)
                    continue;
                IMyBeacon beacon = entity as IMyBeacon;
                if (beacon == null)
                    continue;

                if (beacon.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
                {
                    // Close the market, because it's no longer owner by the player.
                    market.Open = false;
                    continue;
                }

                // TODO: should only return markets in the set range.

                var distance = (position - beacon.WorldMatrix.Translation).Length();
                list.Add(beacon, distance);
            }

            if (list.Count == 0)
                return null;

            var closetEntity = list.OrderBy(f => f.Value).First().Key;
            var closetMarket = EconomyScript.Instance.Data.Markets.First(m => m.MarketId == player.SteamUserId && (!isOpen.HasValue || m.Open == isOpen.Value) && m.EntityId == closetEntity.EntityId);
            return closetMarket;
        }

        public static bool IsItemBlacklistedOnServer(string itemTypeId, string itemSubTypeName)
        {
            var marketItem = EconomyScript.Instance.ServerConfig.DefaultPrices.FirstOrDefault(e => e.TypeId == itemTypeId && e.SubtypeName == itemSubTypeName);
            if (marketItem == null)
                return true; // does't exist. Same as been blacklisted.

            return marketItem.IsBlacklisted;
        }

        #endregion
    }
}
