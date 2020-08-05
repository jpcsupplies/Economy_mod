namespace Economy.scripts.Messages
{
    using EconConfig;
    using EconStructures;
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
   

    [ProtoContract]
    public class MessageMarketManagePlayer : MessageBase
    {
        #region Serialized fields

        [ProtoMember(201)]
        public PlayerMarketManage CommandType;

        [ProtoMember(202)]
        public string MarketName;

        [ProtoMember(203)]
        public decimal Size;

        [ProtoMember(204)]
        public long EntityId;

        /// <summary>
        /// item id we are setting
        /// </summary>
        [ProtoMember(205)]
        public string ItemTypeId;

        /// <summary>
        /// item subid we are setting
        /// </summary>
        [ProtoMember(206)]
        public string ItemSubTypeName;

        /// <summary>
        /// unit price to buy item at.
        /// </summary>
        [ProtoMember(207)]
        public decimal ItemBuyPrice;

        /// <summary>
        /// unit price to sell item at.
        /// </summary>
        [ProtoMember(208)]
        public decimal ItemSellPrice;

        /// <summary>
        /// Market quanity stock limit.
        /// </summary>
        [ProtoMember(209)]
        public decimal ItemStockLimit;

        /// <summary>
        /// Looking for a specific market name.
        /// </summary>
        [ProtoMember(210)]
        public string FindMarketName;

        [ProtoMember(211)]
        public decimal LicenceCost;

        [ProtoMember(212)]
        public long ConfirmCode;

        [ProtoMember(213)]
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

        public static void SendRelinkMessage(long entityId, string marketName, decimal size)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Relink, EntityId = entityId, MarketName = marketName, Size = size });
        }

        public static void SendConfirmMessage(long confirmCode, bool confirmFlag, PlayerMarketManage confirmType)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = confirmType, ConfirmCode = confirmCode, ConfirmFlag = confirmFlag });
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

        public static void SendLoadMessage(long entityId, string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Load, MarketName = marketName, EntityId = entityId });
        }

        public static void SendExportMessage(long entityId, string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Export, MarketName = marketName, EntityId = entityId });
        }

        public static void SendSaveMessage(long entityId, string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Save, MarketName = marketName, EntityId = entityId });
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

        public static void SendStockMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Stock });
        }

        public static void SendUnstockMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Unstock });
        }

        public static void SendRestrictMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Restrict });
        }

        public static void SendLimitMessage(string itemTypeId, string itemSubTypeName, decimal itemStockLimit)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Limit, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemStockLimit = itemStockLimit });
        }

        #endregion

        #region client processing

        public override void ProcessClient()
        {
            switch (CommandType)
            {
                case PlayerMarketManage.ConfirmRegister:
                    {
                        string msg = string.Format("Please confirm the registration of a new trade zone called '{0}', with a size of {1}m radius.\r\n" +
                                                   "The full cost to register it is {2} {3}.", MarketName, Size, LicenceCost, EconomyScript.Instance.ClientConfig.ServerConfig.CurrencyName);
                        MyAPIGateway.Utilities.ShowMissionScreen("Please confirm", " ", " ", msg, ConfirmRegisterResponse, "Accept");
                    }
                    break;
                case PlayerMarketManage.ConfirmRelink:
                    {
                        string msg = string.Format("Please confirm the relink of the existing trade zone called '{0}', with a size of {1}m radius.\r\n" +
                                                   "The cost to relink it is {2} {3}.", MarketName, Size, LicenceCost, EconomyScript.Instance.ClientConfig.ServerConfig.CurrencyName);
                        MyAPIGateway.Utilities.ShowMissionScreen("Please confirm", " ", " ", msg, ConfirmRelinkResponse, "Accept");
                    }
                    break;
            }
        }

        private void ConfirmRegisterResponse(ResultEnum result)
        {
            SendConfirmMessage(ConfirmCode, result == ResultEnum.OK, PlayerMarketManage.ConfirmRegister);
        }

        private void ConfirmRelinkResponse(ResultEnum result)
        {
            SendConfirmMessage(ConfirmCode, result == ResultEnum.OK, PlayerMarketManage.ConfirmRelink);
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

                                var radius = market.MarketZoneSphere?.Radius ?? 1;

                                // TODO: should add a basic stock count. total sum of items in the market.

                                if (destroyed)
                                {
                                    msg.AppendFormat("{0}: '{1}' {2:N}m unteathered.\r\n", counter, market.DisplayName, radius);
                                }
                                else
                                {
                                    if (beacon.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
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
                                EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
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

                        if (beaconBlock.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You must own the beacon to register it as trade zone.");
                            return;
                        }

                        // TODO: need configurable size limit.
                        if (Size < EconomyScript.Instance.ServerConfig.TradeZoneMinRadius || Size > EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A trade zone needs to have a radius between {1:N}m and {0:N}m.",
                                EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius,
                                EconomyScript.Instance.ServerConfig.TradeZoneMinRadius);
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
                        decimal licenceCost = EconomyScript.Instance.ServerConfig.CalculateZoneCost(Size, false);

                        // Check the account can afford the licence.
                        var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                        if (account.BankBalance < licenceCost)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "The Trade Zone Licence is {0:#,#.######} {1}. You cannot afford it.", licenceCost, EconomyScript.Instance.ServerConfig.CurrencyName);
                            return;
                        }

                        var msg = new MessageMarketManagePlayer { CommandType = PlayerMarketManage.ConfirmRegister, EntityId = EntityId, MarketName = MarketName, Size = Size, LicenceCost = licenceCost, ConfirmCode = MyRandom.Instance.NextLong(), SenderSteamId = SenderSteamId };
                        EconomyScript.Instance.PlayerMarketRegister.Add(msg.ConfirmCode, msg);
                        ConnectionHelper.SendMessageToPlayer(SenderSteamId, msg);
                    }
                    break;

                #endregion

                #region relink / Reestablish an unteathered market.

                case PlayerMarketManage.Relink:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "The specified block does not exist.");
                            return;
                        }

                        if (cubeBlock.CubeGrid.GridSizeEnum != MyCubeSize.Large)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "Only large beacons can be registered as a trade zone.");
                            return;
                        }

                        IMyBeacon beaconBlock = cubeBlock as IMyBeacon;

                        if (beaconBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "You need to target a beacon to register a trade zone.");
                            return;
                        }

                        if (beaconBlock.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "You must own the beacon to register it as trade zone.");
                            return;
                        }

                        // TODO: need configurable size limit.
                        if (Size < EconomyScript.Instance.ServerConfig.TradeZoneMinRadius || Size > EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "A trade zone needs to have a radius between {1:N}m and {0:N}m.",
                                EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius,
                                EconomyScript.Instance.ServerConfig.TradeZoneMinRadius);
                            return;
                        }

                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "You do not have a market by that name.");
                            return;
                        }

                        IMyEntity entity;
                        if (MyAPIGateway.Entities.TryGetEntityById(market.EntityId, out entity))
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "You cannot link that market as it is already teathered to a beacon.");
                            return;
                        }

                        // Calculate the full licence cost.
                        // TODO: use cost base + radius size for price.
                        decimal licenceCost = EconomyScript.Instance.ServerConfig.CalculateZoneCost(Size, true);

                        // Check the account can afford the licence.
                        var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                        if (account.BankBalance < licenceCost)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "The Trade Zone Licence is {0:#,#.######} {1}. You cannot afford it.", licenceCost, EconomyScript.Instance.ServerConfig.CurrencyName);
                            return;
                        }

                        var msg = new MessageMarketManagePlayer { CommandType = PlayerMarketManage.ConfirmRelink, EntityId = EntityId, MarketName = MarketName, Size = Size, LicenceCost = licenceCost, ConfirmCode = MyRandom.Instance.NextLong(), SenderSteamId = SenderSteamId };
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
                            EconomyScript.Instance.Data.CreditBalance += msg.LicenceCost;

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

                #region ConfirmRelink

                case PlayerMarketManage.ConfirmRelink:
                    {
                        if (!EconomyScript.Instance.PlayerMarketRegister.ContainsKey(ConfirmCode))
                            return;

                        if (ConfirmFlag)
                        {
                            var msg = EconomyScript.Instance.PlayerMarketRegister[ConfirmCode];

                            var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(msg.MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == msg.SenderSteamId);
                            {
                                if (market == null)
                                {
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "Relink aborted.");
                                    // TODO: error log. market has gone missing?
                                    return;
                                }
                            }

                            // deduct account balance.
                            var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                            account.BankBalance -= msg.LicenceCost;
                            EconomyScript.Instance.Data.CreditBalance += msg.LicenceCost;
                            market.EntityId = msg.EntityId;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "A trade zone has been linked to beacon '{0}'.", msg.MarketName);
                        }
                        else
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RELINK", "Relink cancelled.");
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
                        MessageUpdateClient.SendServerTradeZones();
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

                        if (beacon.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
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
                        MessageUpdateClient.SendServerTradeZones();
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

                        if (beacon.GetUserRelationToOwner(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Owner)
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
                        MessageUpdateClient.SendServerTradeZones();
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "Market '{0}' has been closed for trade.", market.DisplayName);
                    }
                    break;

                #endregion

                #region export
                case PlayerMarketManage.Export:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "The specified block does not exist.");
                            return;
                        }

                        IMyTextPanel textBlock = cubeBlock as IMyTextPanel;

                        if (textBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "You need to target a Text Panel to load market data.");
                            return;
                        }

                        var relation = textBlock.GetUserRelationToOwner(player.IdentityId);
                        if (relation != MyRelationsBetweenPlayerAndBlock.Owner && relation != MyRelationsBetweenPlayerAndBlock.NoOwnership)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "You must own the Text Panel to load market data.");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "*")
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "Invalid name supplied for the Trade Zone name.");
                            return;
                        }

                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "Market not found.");
                            return;
                        }

                        StringBuilder msg = new StringBuilder();

                        msg.AppendLine(@"# Limit/Quantity | Sell/Buy | Restriction | ""Name""");

                        var orderedList = new Dictionary<MarketItemStruct, string>();
                        var groupingList = new Dictionary<MarketItemStruct, MyDefinitionBase>();

                        foreach (var marketItem in market.MarketItems)
                        {
                            MarketItemStruct configItem = EconomyScript.Instance.ServerConfig.DefaultPrices.FirstOrDefault(m => m.TypeId == marketItem.TypeId && m.SubtypeName == marketItem.SubtypeName && !m.IsBlacklisted);
                            if (configItem == null)
                                continue; // doesn't exist or is blacklisted by server.

                            MyObjectBuilderType result;
                            if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
                            {
                                MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(marketItem.TypeId, marketItem.SubtypeName);

                                if (definition != null)
                                {
                                    groupingList.Add(marketItem, definition);
                                    orderedList.Add(marketItem, definition.GetDisplayName());
                                }
                            }
                        }

                        orderedList = orderedList.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        // TODO: unique name checks on items.

                        msg.AppendLine();
                        msg.AppendLine(@"## Ores");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Ore))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Ingots");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Ingot))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Components");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Component))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Ammo");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_AmmoMagazine))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Tools");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_PhysicalGunObject))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Supplies");

                        foreach (var kvp in orderedList)
                        {
                            if ((groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_ConsumableItem)) && (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Package)) && (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Datapad)))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }


                        msg.AppendLine();
                        msg.AppendLine(@"## Other");

                        foreach (var kvp in orderedList)
                        {
                            var type = groupingList[kvp.Key].Id.TypeId;

                            if (type == typeof(MyObjectBuilder_Ore)
                                || type == typeof(MyObjectBuilder_Ingot)
                                || type == typeof(MyObjectBuilder_Component)
                                || type == typeof(MyObjectBuilder_ConsumableItem)
                                || type == typeof(MyObjectBuilder_Package)
                                || type == typeof(MyObjectBuilder_Datapad)
                                || type == typeof(MyObjectBuilder_AmmoMagazine)
                                || type == typeof(MyObjectBuilder_PhysicalGunObject))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.Append(@"
# Any text behind the # will be ignored.
");

                        textBlock.WriteText(msg.ToString(), false);

                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ EXPORT", "Text panel updated.");
                    }
                    break;
                #endregion export


                #region load

                case PlayerMarketManage.Load:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "The specified block does not exist.");
                            return;
                        }

                        IMyTextPanel textBlock = cubeBlock as IMyTextPanel;

                        if (textBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "You need to target a Text Panel to load market data.");
                            return;
                        }

                        var relation = textBlock.GetUserRelationToOwner(player.IdentityId);
                        if (relation != MyRelationsBetweenPlayerAndBlock.Owner && relation != MyRelationsBetweenPlayerAndBlock.NoOwnership)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "You must own the Text Panel to load market data.");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "*")
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "Invalid name supplied for the Trade Zone name.");
                            return;
                        }

                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase) && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "You do not have a market by that name.");
                            return;
                        }

                        StringBuilder msg = new StringBuilder();

                        msg.AppendLine(@"# Limit/Quantity | Sell/Buy | Restriction | ""Name""");

                        var orderedList = new Dictionary<MarketItemStruct, string>();
                        var groupingList = new Dictionary<MarketItemStruct, MyDefinitionBase>();

                        foreach (var marketItem in market.MarketItems)
                        {
                            MarketItemStruct configItem = EconomyScript.Instance.ServerConfig.DefaultPrices.FirstOrDefault(m => m.TypeId == marketItem.TypeId && m.SubtypeName == marketItem.SubtypeName && !m.IsBlacklisted);
                            if (configItem == null)
                                continue; // doesn't exist or is blacklisted by server.

                            MyObjectBuilderType result;
                            if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
                            {
                                MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(marketItem.TypeId, marketItem.SubtypeName);

                                if (definition != null)
                                {
                                    groupingList.Add(marketItem, definition);
                                    orderedList.Add(marketItem, definition.GetDisplayName());
                                }
                            }
                        }

                        orderedList = orderedList.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        // TODO: unique name checks on items.

                        msg.AppendLine();
                        msg.AppendLine(@"## Ores");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Ore))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Ingots");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Ingot))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Components");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Component))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Ammo");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_AmmoMagazine))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Tools");

                        foreach (var kvp in orderedList)
                        {
                            if (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_PhysicalGunObject))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }


                        msg.AppendLine();
                        msg.AppendLine(@"## Supplies");

                        foreach (var kvp in orderedList)
                        {
                            if ((groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_ConsumableItem)) && (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Package)) && (groupingList[kvp.Key].Id.TypeId != typeof(MyObjectBuilder_Datapad)))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.AppendLine();
                        msg.AppendLine(@"## Other");

                        foreach (var kvp in orderedList)
                        {
                            var type = groupingList[kvp.Key].Id.TypeId;

                            if (type == typeof(MyObjectBuilder_Ore)
                                || type == typeof(MyObjectBuilder_Ingot)
                                || type == typeof(MyObjectBuilder_Component)
                                || type == typeof(MyObjectBuilder_AmmoMagazine)
                                || type == typeof(MyObjectBuilder_ConsumableItem)
                                || type == typeof(MyObjectBuilder_Package)
                                || type == typeof(MyObjectBuilder_Datapad)
                                || type == typeof(MyObjectBuilder_PhysicalGunObject))
                                continue;

                            msg.AppendFormat("{0}/{1} | {2}/{3} | {4} | \"{5}\" \r\n", kvp.Key.IsBlacklisted ? "-1" : (kvp.Key.StockLimit == decimal.MaxValue ? "MAX" : kvp.Key.StockLimit.ToString(CultureInfo.InvariantCulture)), kvp.Key.Quantity, kvp.Key.SellPrice, kvp.Key.BuyPrice, "A", kvp.Value);
                        }

                        msg.Append(@"
# Any text behind the # will be ignored.
#   They may be removed.
# Items will be displayed in alphabetical order.
# The values are:
# Limit/Quantity | Sell/Buy | Restriction | ""Name""
# Do not modify the name as this is used to identify 
#   the item.
# Do not modify the Quantity. It will have no effect.
# A limit of -1 means it is blacklisted.
# A limit of 0 or more, means your market will not not 
#   allow you stock any more items above that value (no
#   one can sell you more), but it will sell any items
#   it has in stock. You can overstock your own market
#   if you wish, but no one else can.
# Sell Price, is the price you sell at, which another
#   player will pay to buy from you.
# Buy price, is the price you buy at, which another
#   player will receive in credits to sell to you.
# The keyword RESET will reset the buy and sell price to
#   that of the default market as set by the server.
# The % used in either of the buy or sell price, will apply
#   that % based on the other value.
# Ie., a buy price of 3.73, and a sell price of 5% will apply
#   a 5% markup to the buy and become 3.9165
# a sell price of 3.9 and a buy price of 5%, will apply a
#   5% markdown to the sell and become 3.705
# Restriction flags are not yet implemented and are
#   placeholders for how to trade with faction members,
#   allies, neutral players and enemies.
");

                        textBlock.WriteText(msg.ToString(), false);

                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LOAD", "Text panel updated.");
                    }
                    break;

                #endregion

                #region save

                case PlayerMarketManage.Save:
                    {
                        // TODO:

                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SAVE", "Not yet.");
                    }
                    break;

                #endregion

                case PlayerMarketManage.FactionMode:
                    {
                        // TODO:

                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ FACTION", "Not yet.");
                    }
                    break;

                #region buy price

                case PlayerMarketManage.BuyPrice:
                    {
                        if (ItemBuyPrice < 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ BUYPRICE", "Can not set buy price to less than 0.");
                            return;
                        }

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

                #endregion

                #region sell price

                case PlayerMarketManage.SellPrice:
                    {
                        if (ItemSellPrice < 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ SELLPRICE", "Can not set buy price to less than 0.");
                            return;
                        }

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

                #endregion

                case PlayerMarketManage.Stock:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Unstock:
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

                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ RESTRICT", "Not yet.");
                    }
                    break;

                #region limit

                case PlayerMarketManage.Limit:
                    {
                        if (ItemStockLimit < 0)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "Can not set stock limit to less than 0.");
                            return;
                        }

                        var market = MarketManager.FindClosestPlayerMarket(player);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "You have no open markets.");
                            return;
                        }

                        var marketItem = market.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
                        if (marketItem == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "Sorry, the items you are trying to set doesn't have a market entry!");
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
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "Sorry, the item you specified doesn't exist!");
                            return;
                        }

                        marketItem.StockLimit = ItemStockLimit;
                        if (ItemStockLimit == decimal.MaxValue)
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "Item '{0}'; Set stock limit to {1}", definition.GetDisplayName(), "MAX");
                        else
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ LIMIT", "Item '{0}'; Set stock limit to {1}", definition.GetDisplayName(), ItemStockLimit);
                    }
                    break;

                #endregion

                #region blacklist

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

                    #endregion
            }
        }
    }
}
