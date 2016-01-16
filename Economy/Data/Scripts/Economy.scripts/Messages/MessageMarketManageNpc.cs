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
        [ProtoMember(1)]
        public MarketManage CommandType;

        [ProtoMember(2)]
        public string MarketName;

        [ProtoMember(3)]
        public decimal X;

        [ProtoMember(4)]
        public decimal Y;

        [ProtoMember(5)]
        public decimal Z;

        [ProtoMember(6)]
        public decimal Size;

        [ProtoMember(7)]
        public MarketZoneType Shape;

        [ProtoMember(8)]
        public string OldMarketName;

        public static void SendAddMessage(string marketName, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = MarketManage.Add, MarketName = marketName, X = x, Y = y, Z = z, Size = size, Shape = shape });
        }

        public static void SendDeleteMessage(string marketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = MarketManage.Delete, MarketName = marketName });
        }

        public static void SendRenameMessage(string oldMarketName, string newMarketName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = MarketManage.Rename, OldMarketName = oldMarketName, MarketName = newMarketName });
        }

        public static void SendListMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketManageNpc { CommandType = MarketManage.List });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Price List Request for from '{0}'", SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            if (player == null || !player.IsAdmin()) // hold on there, are we an admin first?
                return;

            switch (CommandType)
            {
                case MarketManage.Add:
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

                case MarketManage.Delete:
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

                case MarketManage.List:
                    {
                        var str = new StringBuilder();
                        foreach (var market in EconomyScript.Instance.Data.Markets)
                        {
                            if (market.MarketId != EconomyConsts.NpcMerchantId)
                                continue;

                            str.AppendFormat("Market: {0}\r\n", market.DisplayName);
                            str.AppendFormat("{0}", market.MarketZoneType);
                            if (market.MarketZoneType == MarketZoneType.FixedSphere && market.MarketZoneSphere.HasValue)
                                str.AppendFormat("  Position={0:N} | {1:N} | {2:N} Radius={3:N}m\r\n\r\n", market.MarketZoneSphere.Value.Center.X, market.MarketZoneSphere.Value.Center.Y, market.MarketZoneSphere.Value.Center.Z, market.MarketZoneSphere.Value.Radius);
                        }

                        MessageClientDialogMessage.SendMessage(SenderSteamId, "NPC Market List", " ", str.ToString());
                    }
                    break;

                case MarketManage.Rename:
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

                case MarketManage.Move:
                    // TODO:
                    break;
            }
        }
    }
}
