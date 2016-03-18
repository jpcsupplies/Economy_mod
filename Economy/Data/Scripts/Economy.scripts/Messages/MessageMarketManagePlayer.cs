namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using EconStructures;
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using IMyBeacon = Sandbox.ModAPI.Ingame.IMyBeacon;

    [ProtoContract]
    public class MessageMarketManagePlayer : MessageBase
    {
        #region Serialized fields

        [ProtoMember(1)]
        public PlayerMarketManage CommandType;

        [ProtoMember(2)]
        public string MarketName;

        [ProtoMember(3)]
        public decimal Size;

        [ProtoMember(4)]
        public long EntityId;

        /// <summary>
        /// item id we are setting
        /// </summary>
        [ProtoMember(5)]
        public string ItemTypeId;

        /// <summary>
        /// item subid we are setting
        /// </summary>
        [ProtoMember(6)]
        public string ItemSubTypeName;

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

        /// <summary>
        /// Looking for a specific market name.
        /// </summary>
        [ProtoMember(9)]
        public string FindMarketName;

        [ProtoMember(10)]
        public decimal LicenceCost;

        [ProtoMember(11)]
        public long ConfirmCode;

        [ProtoMember(12)]
        public bool ConfirmFlag;

        #endregion

        #region Send message Methods

        public static void SendListMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.List });
        }

        public static void SendRegisterMessage(long entityId, string marketName, decimal size)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Register, EntityId = entityId, MarketName = marketName, Size = size });
        }

        public static void SendConfirmMessage(long confirmCode, bool confirmFlag)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.ConfirmRegister, ConfirmCode = confirmCode, ConfirmFlag = confirmFlag });
        }

        public static void SendUnregisterMessage(string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Unregister, MarketName = marketName });
        }

        public static void SendOpenMessage(string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Open, MarketName = marketName });
        }

        public static void SendCloseMessage(string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Close, MarketName = marketName });
        }

        public static void SendFactionModeMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.FactionMode });
        }

        public static void SendBuyPriceMessage(string itemTypeId, string itemSubTypeName, decimal itemBuyPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.BuyPrice, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemBuyPrice = itemBuyPrice });
        }

        public static void SendSellPriceMessage(string itemTypeId, string itemSubTypeName, decimal itemSellPrice)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.SellPrice, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemSellPrice = itemSellPrice });
        }

        public static void SendBlacklistMessage(string itemTypeId, string itemSubTypeName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Blacklist, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName });
        }

        public static void SendLoadMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Load });
        }

        public static void SendUnloadMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Unload });
        }

        public static void SendRestrictMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Restrict });
        }

        public static void SendLimitMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Limit });
        }

        #endregion

        #region client processing

        public override void ProcessClient()
        {
            if (CommandType == PlayerMarketManage.ConfirmRegister)
            {
                string msg = string.Format("Please confirm the registration of a new trade zone called '{0}', with a size of {1}m radius.\r\n" +
                                           "The full cost to register it is {2} {3}.", MarketName, Size, LicenceCost, EconomyScript.Instance.ClientConfig.CurrencyName);
                MyAPIGateway.Utilities.ShowMissionScreen("Please confirm", " ", " ", msg, ConfirmDialog, "Accept");
            }
        }

        private void ConfirmDialog(ResultEnum result)
        {
            SendConfirmMessage(ConfirmCode, result == ResultEnum.OK);
        }

        #endregion

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Manage Player Market Request for from '{0}'", SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            if (!EconomyScript.Instance.ServerConfig.EnablePlayerTradezones)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "TZ", "Player Trade zones are disabled.");
                return;
            }

            switch (CommandType)
            {
                #region list

                case PlayerMarketManage.List:
                    {
                        var msg = new StringBuilder();
                        var markets = EconomyScript.Instance.Data.Markets.Where(m => m.MarketId == player.SteamUserId).ToArray();

                        if (markets.Length == 0)
                        {
                            msg.AppendLine("You do not have any registered markets currently.");
                            msg.AppendFormat("You can register up to {0} trade zones.\r\n", EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                        }
                        else
                        {
                            msg.AppendFormat("You have {0} markets currently registered trade zones from a maximum of {1}.\r\n", markets.Length, EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                            msg.AppendLine();

                            int counter = 1;
                            foreach (var market in markets.OrderBy(m => m.DisplayName))
                            {
                                bool destroyed = false;

                                IMyEntity entity;
                                if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                                    destroyed = true;

                                if (entity != null && (entity.Closed || entity.MarkedForClose))
                                    destroyed = true;

                                IMyBeacon beacon = entity as IMyBeacon;
                                if (beacon == null)
                                    destroyed = true;

                                var radius = market.MarketZoneSphere.HasValue ? market.MarketZoneSphere.Value.Radius : 1;

                                // TODO: should add a basic stock count. total sum of items in the market.

                                if (destroyed)
                                {
                                    msg.AppendFormat("{0}: '{1}' {2:N}m unteathered.\r\n", counter, market.DisplayName, radius);
                                }
                                else
                                {
                                    if (beacon.GetUserRelationToOwner(player.PlayerID) != MyRelationsBetweenPlayerAndBlock.Owner)
                                    {
                                        msg.AppendFormat("{0}: '{1}' {2:N}m occupied.\r\n", counter, market.DisplayName, radius);
                                    }
                                    else if (!beacon.IsWorking)
                                    {
                                        msg.AppendFormat("{0}: '{1}' {2:N}m turned off.\r\n", counter, market.DisplayName, radius);
                                    }
                                    else
                                    {
                                        msg.AppendFormat("{0}: '{1}' {2:N}m {3}.\r\n", counter, market.DisplayName, radius, market.Open ? "open" : "closed");
                                    }
                                }

                                counter++;
                            }

                            msg.AppendLine();
                            msg.AppendFormat("If you have an unteathered market, you can reestablish the market on a new beacon for {0:P} of the cost to establish a new one.\r\n",
                                EconomyScript.Instance.ServerConfig.TradeZoneReestablishRatio);
                            msg.AppendLine("If you have an occupied market, you can open it again at no cost after recapturing the beacon.");
                        }

                        msg.AppendLine();
                        msg.AppendFormat("The Trade Zone Licence is {0:#,#.######} {1} for 1m, to {2:#,#.######} {1} for {3:N}m.", 
                            EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin, 
                            EconomyScript.Instance.ServerConfig.CurrencyName,
                            EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax,
                            EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius);
                        MessageClientDialogMessage.SendMessage(SenderSteamId, "TZ LIST", " ", msg.ToString());
                        return;
                    }

                #endregion

                #region register

                case PlayerMarketManage.Register:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "The specified block does not exist.");
                            return;
                        }

                        if (cubeBlock.CubeGrid.GridSizeEnum != MyCubeSize.Large)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "Only large beacons can be registered as a trade zone.");
                            return;
                        }

                        IMyBeacon beaconBlock = cubeBlock as IMyBeacon;

                        if (beaconBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You need to target a beacon to register a trade zone.");
                            return;
                        }

                        if (beaconBlock.GetUserRelationToOwner(player.PlayerID) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You must own the beacon to register it as trade zone.");
                            return;
                        }

                        // TODO: need configurable size limit.
                        if (Size < EconomyScript.Instance.ServerConfig.TradeZoneMinRadius || Size > EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You cannot make a trade zone greated than {0} diameter.", EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "*")
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "Invalid name supplied for the Trade Zone name.");
                            return;
                        }

                        // This also prevents other players from using the same beacon for a market, in case they have already hacked the trade beacon.
                        var checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.EntityId == EntityId);
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A trade zone has already been registered to beacon '{0}'.", beaconBlock.CustomName);
                            return;
                        }

                        checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A trade zone of name '{0}' already exists.", checkMarket.DisplayName);
                            return;
                        }

                        var count = EconomyScript.Instance.Data.Markets.Count(m => m.MarketId == SenderSteamId);
                        if (count >= EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You cannot register another trade zone. You already have {0} of the {1} allowed.", count, EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                            return;
                        }

                        // Calculate the full licence cost.
                        // TODO: use cost base + radius size for price.
                        decimal licenceCost = EconomyScript.Instance.ServerConfig.CalculateZoneCost(Size);

                        // Check the account can afford the licence.
                        var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                        if (account.BankBalance < licenceCost)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "The Trade Zone Licence is {0:#,#.######} {1}. You cannot afford it.", licenceCost, EconomyScript.Instance.ServerConfig.CurrencyName);
                            return;
                        }

                        var msg = new MessageMarketManagePlayer { CommandType = PlayerMarketManage.ConfirmRegister, EntityId = EntityId, MarketName = MarketName, Size = Size, LicenceCost = licenceCost, ConfirmCode = MyRandom.Instance.NextLong() };
                        EconomyScript.Instance.PlayerMarketRegister.Add(msg.ConfirmCode, msg);
                        ConnectionHelper.SendMessageToPlayer(SenderSteamId, msg);
                    }
                    break;

                #endregion

                #region ConfirmRegister

                case PlayerMarketManage.ConfirmRegister:
                    {
                        if (!EconomyScript.Instance.PlayerMarketRegister.ContainsKey(ConfirmCode))
                            return;

                        if (ConfirmFlag)
                        {
                            var msg = EconomyScript.Instance.PlayerMarketRegister[ConfirmCode];
                            // deduct account balance.
                            var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                            account.BankBalance -= msg.LicenceCost;
                            var marketAccount = EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == EconomyConsts.NpcMerchantId);
                            marketAccount.BankBalance += msg.LicenceCost; // TODO: send fee to a core account instead.

                            EconDataManager.CreatePlayerMarket(player.SteamUserId, msg.EntityId, (double)msg.Size, msg.MarketName);
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A new trade zone called registered to beacon '{0}'.", msg.MarketName);
                        }
                        else
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "Registration cancelled.");
                        }

                        EconomyScript.Instance.PlayerMarketRegister.Remove(ConfirmCode);
                        break;
                    }
                #endregion

                #region unregister

                case PlayerMarketManage.Unregister:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "You do not have a market by that name.");
                            return;
                        }

                        EconomyScript.Instance.Data.Markets.Remove(market);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "The market '{0}' has been removed.", MarketName);
                    }
                    break;

                #endregion

                #region open

                case PlayerMarketManage.Open:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "You do not have a market by that name.");
                            return;
                        }

                        IMyEntity entity;
                        if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "You cannot open that market as it is unteathered.");
                            return;
                        }

                        IMyBeacon beacon = entity as IMyBeacon;
                        if (entity.Closed || entity.MarkedForClose || beacon == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "You cannot open that market as it is unteathered.");
                            return;
                        }

                        if (beacon.GetUserRelationToOwner(player.PlayerID) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "You cannot open that market you don't own the block anymore.");
                            return;
                        }

                        if (market.Open)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "That market is already open.");
                            return;
                        }

                        market.Open = true;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "Market '{0}' has been opened for trade.", market.DisplayName);
                    }
                    break;

                #endregion

                #region close

                case PlayerMarketManage.Close:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "You do not have a market by that name.");
                            return;
                        }

                        IMyEntity entity;
                        if (!MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "You cannot close that market as it is unteathered.");
                            return;
                        }

                        IMyBeacon beacon = entity as IMyBeacon;
                        if (entity.Closed || entity.MarkedForClose || beacon == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "You cannot close that market as it is unteathered.");
                            return;
                        }

                        if (beacon.GetUserRelationToOwner(player.PlayerID) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "You cannot close that market you don't own the block anymore.");
                            return;
                        }

                        if (!market.Open)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "That market is already closed.");
                            return;
                        }

                        market.Open = false;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "Market '{0}' has been closed for trade.", market.DisplayName);
                    }
                    break;

                #endregion

                case PlayerMarketManage.FactionMode:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.BuyPrice:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "You have no open markets.");
                            return;
                        }

                        var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                        if (marketItem == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "Sorry, the items you are trying to set doesn't have a market entry!");
                            return;
                        }

                        MyDefinitionBase definition = null;
                        MyObjectBuilderType result;
                        if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
                        {
                            var id = new MyDefinitionId(result, ItemSubTypeName);
                            MyDefinitionManager.Static.TryGetDefinition(id, out definition);
                        }

                        if (definition == null)
                        {
                            // Passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "Sorry, the item you specified doesn't exist!");
                            return;
                        }

                        marketItem.BuyPrice = ItemBuyPrice;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "Item '{0}'; Set buy price to {1}", definition.GetDisplayName(), ItemBuyPrice);
                    }
                    break;

                case PlayerMarketManage.SellPrice:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SELLPRICE", "You have no open markets.");
                            return;
                        }


                        var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                        if (marketItem == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SELLPRICE", "Sorry, the items you are trying to set doesn't have a market entry!");
                            return;
                        }

                        MyDefinitionBase definition = null;
                        MyObjectBuilderType result;
                        if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
                        {
                            var id = new MyDefinitionId(result, ItemSubTypeName);
                            MyDefinitionManager.Static.TryGetDefinition(id, out definition);
                        }

                        if (definition == null)
                        {
                            // Passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SELLPRICE", "Sorry, the item you specified doesn't exist!");
                            return;
                        }

                        marketItem.SellPrice = ItemSellPrice;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SELLPRICE", "Item '{0}'; Set sell price to {1}", definition.GetDisplayName(), ItemSellPrice);
                    }
                    break;

                case PlayerMarketManage.Load:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Unload:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Restrict:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RESTRICT", "You have no open markets.");
                            return;
                        }

                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Limit:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "You have no open markets.");
                            return;
                        }

                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Blacklist:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BLACKLIST", "You have no open markets.");
                            return;
                        }

                        var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                        if (marketItem == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BLACKLIST", "Sorry, the items you are trying to set doesn't have a market entry!");
                            return;
                        }

                        MarketItemStruct configItem = EconomyScript.Instance.ServerConfig.DefaultPrices.First(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                        if (configItem.IsBlacklisted)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BLACKLIST", "That item is blacklisted by the server! You cannot change it.");
                            return;
                        }

                        MyDefinitionBase definition = null;
                        MyObjectBuilderType result;
                        if (MyObjectBuilderType.TryParse(ItemTypeId, out result))
                        {
                            var id = new MyDefinitionId(result, ItemSubTypeName);
                            MyDefinitionManager.Static.TryGetDefinition(id, out definition);
                        }

                        if (definition == null)
                        {
                            // Passing bad data?
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BLACKLIST", "Sorry, the item you specified doesn't exist!");
                            return;
                        }

                        marketItem.IsBlacklisted = !marketItem.IsBlacklisted;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "Item '{0}'; Set Blacklist to {1}", definition.GetDisplayName(), marketItem.IsBlacklisted ? "On" : "Off");
                    }
                    break;
            }
        }
    }
}
