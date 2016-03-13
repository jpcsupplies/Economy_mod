namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using VRage.Game;
    using IMyCubeBlock = Sandbox.ModAPI.IMyCubeBlock;

    [ProtoContract]
    public class MessageMarketManagePlayer : MessageBase
    {
        [ProtoMember(1)]
        public PlayerMarketManage CommandType;

        [ProtoMember(2)]
        public long EntityId;

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

        public static void SendBuyPriceMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.BuyPrice });
        }

        public static void SendSellPriceMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.SellPrice });
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

        public static void SendBlacklistMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManagePlayer { CommandType = PlayerMarketManage.Blacklist });
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

                        var checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.EntityId == EntityId);
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A market has already been registered to beacon '{0}'.", beaconBlock.CustomName);
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
                        marketAccount.BankBalance += EconomyScript.Instance.ServerConfig.TradeZoneLicenceCost;

                        EconDataManager.CreatePlayerMarket(player.SteamUserId, beaconBlock.EntityId, beaconBlock.Radius, beaconBlock.CustomName);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "TZ REGISTER", "A new market called registered to beacon '{0}'.", beaconBlock.CustomName);
                    }
                    break;

                case PlayerMarketManage.Unregister:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Open:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Close:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.FactionMode:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.BuyPrice:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.SellPrice:
                    {
                        // TODO:
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
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Limit:
                    {
                        // TODO:
                    }
                    break;

                case PlayerMarketManage.Blacklist:
                    {
                        // TODO:
                    }
                    break;
            }
        }
    }
}
