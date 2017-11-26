namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessageMarketManageNpc : MessageBase
    {
        [ProtoMember(201)]
        public NpcMarketManage CommandType;

        [ProtoMember(202)]
        public string MarketName;

        [ProtoMember(203)]
        public decimal X;

        [ProtoMember(204)]
        public decimal Y;

        [ProtoMember(205)]
        public decimal Z;

        [ProtoMember(206)]
        public decimal Size;

        [ProtoMember(207)]
        public MarketZoneType Shape;

        [ProtoMember(208)]
        public string OldMarketName;

        public static void SendAddMessage(string marketName, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = NpcMarketManage.Add, MarketName = marketName, X = x, Y = y, Z = z, Size = size, Shape = shape });
        }

        public static void SendDeleteMessage(string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = NpcMarketManage.Delete, MarketName = marketName });
        }

        public static void SendRenameMessage(string oldMarketName, string newMarketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = NpcMarketManage.Rename, OldMarketName = oldMarketName, MarketName = newMarketName });
        }

        public static void SendMoveMessage(string marketName, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = NpcMarketManage.Move, MarketName = marketName, X = x, Y = y, Z = z, Size = size, Shape = shape });
        }

        public static void SendListMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = NpcMarketManage.List });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Manage Npc Market Request for from '{0}'", SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            if (player == null || !player.IsAdmin()) // hold on there, are we an admin first?
                return;

            switch (CommandType)
            {
                case NpcMarketManage.Add:
                    {
                        if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "*")
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "NPC ADD", "Invalid name supplied for the market name.");
                            return;
                        }

                        var checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "NPC ADD", "A market of name '{0}' already exists.", checkMarket.DisplayName);
                            return;
                        }

                        // TODO: market inside market check?

                        EconDataManager.CreateNpcMarket(MarketName, X, Y, Z, Size, Shape);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "NPC ADD", "A new market called '{0}' has been created.", MarketName);
                    }
                    break;

                case NpcMarketManage.Delete:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (market == null)
                        {
                            var markets = EconomyScript.Instance.Data.Markets.Where(m => m.DisplayName.IndexOf(MarketName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();
                            if (markets.Length == 0)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "NPC DELETE", "The specified market name could not be found.");
                                return;
                            }
                            if (markets.Length > 1)
                            {
                                var str = new StringBuilder();
                                str.Append("The specified market name could not be found.\r\n    Which did you mean?\r\n");
                                foreach (var m in markets)
                                    str.AppendLine(m.DisplayName);
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "NPC DELETE", " ", str.ToString());
                                return;
                            }
                            market = markets[0];
                        }

                        EconomyScript.Instance.Data.Markets.Remove(market);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "NPC DELETE", "The market '{0}' has been removed and all inventory.", market.DisplayName);
                    }
                    break;

                case NpcMarketManage.List:
                    {
                        var str = new StringBuilder();
                        foreach (var market in EconomyScript.Instance.Data.Markets)
                        {
                            if (market.MarketId != EconomyConsts.NpcMerchantId)
                                continue;

                            str.AppendFormat("Market: {0}\r\n", market.DisplayName);
                            str.AppendFormat("{0}", market.MarketZoneType);
                            if (market.MarketZoneType == MarketZoneType.FixedSphere && market.MarketZoneSphere.HasValue)
                                str.AppendFormat("  Center Position=X:{0:N} | Y:{1:N} | Z:{2:N} Radius={3:N}m\r\n\r\n", market.MarketZoneSphere.Value.Center.X, market.MarketZoneSphere.Value.Center.Y, market.MarketZoneSphere.Value.Center.Z, market.MarketZoneSphere.Value.Radius);
                            else if (market.MarketZoneType == MarketZoneType.FixedBox && market.MarketZoneBox.HasValue)
                                str.AppendFormat("  Center Position=X:{0:N} | Y:{1:N} | Z:{2:N} Size={3:N}m\r\n\r\n", market.MarketZoneBox.Value.Center.X, market.MarketZoneBox.Value.Center.Y, market.MarketZoneBox.Value.Center.Z, market.MarketZoneBox.Value.Size.X);
                            else
                                str.AppendLine("\r\n");
                        }

                        MessageClientDialogMessage.SendMessage(SenderSteamId, "NPC Market List", " ", str.ToString());
                    }
                    break;

                case NpcMarketManage.Rename:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(OldMarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (market == null)
                        {
                            var markets = EconomyScript.Instance.Data.Markets.Where(m => m.DisplayName.IndexOf(OldMarketName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();
                            if (markets.Length == 0)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "NPC RENAME", "The specified market name could not be found.");
                                return;
                            }
                            if (markets.Length > 1)
                            {
                                var str = new StringBuilder();
                                str.Append("The specified market name could not be found.\r\n    Which did you mean?\r\n");
                                foreach (var m in markets)
                                    str.AppendLine(m.DisplayName);
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "NPC RENAME", " ", str.ToString());
                                return;
                            }
                            market = markets[0];
                        }


                        if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "*")
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "NPC RENAME", "Invalid name supplied for the market name.");
                            return;
                        }

                        var checkMarket = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (checkMarket != null)
                        {
                            MessageClientTextMessage.SendMessage(SenderSteamId, "NPC RENAME", "A market of name '{0}' already exists.", checkMarket.DisplayName);
                            return;
                        }

                        var oldName = market.DisplayName;
                        market.DisplayName = MarketName;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "NPC RENAME", "The market '{0}' has been renamed to '{1}.", oldName, market.DisplayName);
                    }
                    break;

                case NpcMarketManage.Move:
                    {
                        var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.DisplayName.Equals(MarketName, StringComparison.InvariantCultureIgnoreCase));
                        if (market == null)
                        {
                            var markets = EconomyScript.Instance.Data.Markets.Where(m => m.DisplayName.IndexOf(MarketName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();
                            if (markets.Length == 0)
                            {
                                MessageClientTextMessage.SendMessage(SenderSteamId, "NPC MOVE", "The specified market name could not be found.");
                                return;
                            }
                            if (markets.Length > 1)
                            {
                                var str = new StringBuilder();
                                str.Append("The specified market name could not be found.\r\n    Which did you mean?\r\n");
                                foreach (var m in markets)
                                    str.AppendLine(m.DisplayName);
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "NPC MOVE", " ", str.ToString());
                                return;
                            }
                            market = markets[0];
                        }

                        EconDataManager.SetMarketShape(market, X, Y, Z, Size, Shape);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "NPC MOVE", "The market '{0}' has been moved and resized.", market.DisplayName);
                    }
                    break;
            }
        }
    }
}
