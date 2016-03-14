namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using EconStructures;
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using VRage.Game;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;
    using IMyCubeBlock = Sandbox.ModAPI.IMyCubeBlock;

    [ProtoContract]
    public class MessageMarketManagePlayer : MessageBase
    {
        [ProtoMember(1)]
        public PlayerMarketManage CommandType;

        [ProtoMember(2)]
        public long EntityId;

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

        /// <summary>
        /// unit price to buy item at.
        /// </summary>
        [ProtoMember(5)]
        public decimal ItemBuyPrice;

        /// <summary>
        /// unit price to sell item at.
        /// </summary>
        [ProtoMember(6)]
        public decimal ItemSellPrice;



        //[ProtoMember(2)]
        //public string MarketName;

        //[ProtoMember(3)]
        //public decimal X;

        //[ProtoMember(4)]
        //public decimal Y;

        //[ProtoMember(5)]
        //public decimal Z;

        //[ProtoMember(6)]
        //public decimal Size;

        //[ProtoMember(7)]
        //public MarketZoneType Shape;

        [ProtoMember(8)]
        public string OldMarketName;

        public static void SendRegisterMessage(long entityId)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Register, EntityId = entityId });
        }

        public static void SendUnregisterMessage(long entityId)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Unregister, EntityId = entityId });
        }

        public static void SendOpenMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Open });
        }

        public static void SendCloseMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Close });
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

        public override void ProcessClient()
        {
            // never processed on client
        }

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
                #region register

                case PlayerMarketManage.Register:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "The specified block does not exist.");
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

                        // This also prevents other players from using the same beacon for a market, in case they have already hacked the trade beacon.
                        var checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.EntityId == EntityId);
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A market has already been registered to beacon '{0}'.", beaconBlock.CustomName);
                            return;
                        }

                        var count = EconomyScript.Instance.Data.Markets.Count(m => m.MarketId == SenderSteamId);
                        if (count >= EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "You cannot register another trade zone. You already have {0} of the {1} allowed.", count, EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                            return;
                        }

                        // Check the account can afford the licence.
                        var account = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
                        if (account.BankBalance < EconomyScript.Instance.ServerConfig.TradeZoneLicenceCost)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "The Trade Zone Licence is {0:#,#.######} {1}. You cannot afford it.", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCost, EconomyScript.Instance.ServerConfig.CurrencyName);
                            return;
                        }

                        // deduct account balance.
                        account.BankBalance -= EconomyScript.Instance.ServerConfig.TradeZoneLicenceCost;
                        var marketAccount = EconomyScript.Instance.Data.Accounts.FirstOrDefault(a => a.SteamId == EconomyConsts.NpcMerchantId);
                        marketAccount.BankBalance += EconomyScript.Instance.ServerConfig.TradeZoneLicenceCost; // TODO: send fee to a core account instead.

                        EconDataManager.CreatePlayerMarket(player.SteamUserId, beaconBlock.EntityId, beaconBlock.Radius, beaconBlock.CustomName);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A new market called registered to beacon '{0}'.", beaconBlock.CustomName);
                    }
                    break;

                #endregion

                #region unregister

                case PlayerMarketManage.Unregister:
                    {
                        IMyCubeBlock cubeBlock = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeBlock;

                        if (cubeBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "The specified block does not exist.");
                            return;
                        }

                        IMyBeacon beaconBlock = cubeBlock as IMyBeacon;

                        if (beaconBlock == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "You need to target a beacon to register a trade zone.");
                            return;
                        }

                        if (beaconBlock.GetUserRelationToOwner(player.PlayerID) != MyRelationsBetweenPlayerAndBlock.Owner)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "You must own the beacon to register it as trade zone.");
                            return;
                        }

                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.EntityId == EntityId && m.MarketId == SenderSteamId);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "You do not have a market registered at this location.");
                            return;
                        }

                        EconomyScript.Instance.Data.Markets.Remove(market);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ UNREGISTER", "The market registered to beacon '{0}' has been removed.", beaconBlock.CustomName);
                    }
                    break;

                #endregion

                #region open

                case PlayerMarketManage.Open:
                    {
                        var market = MarketManager.FindClosestPlayerMarket(player, false);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ OPEN", "You have no closed markets.");
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
                        var market = MarketManager.FindClosestPlayerMarket(player, true);
                        if (market == null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ CLOSE", "You have no open markets.");
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
