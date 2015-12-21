namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EconConfig;
    using Economy.scripts.EconStructures;
    using Management;
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.ModAPI;

    [ProtoContract]
    public class MessageMarketPriceList : MessageBase
    {
        public static void SendMessage()
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketPriceList());
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.Write("Price List Request for from '{0}'", SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            var character = player.GetCharacter();
            if (character == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PRICELIST", "You are dead. You get market items values while dead.");
                return;
            }
            var position = ((IMyEntity)character).WorldMatrix.Translation;

            var markets = MarketManager.FindMarketsFromLocation(position);
            if (markets.Count == 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PRICELIST", "Sorry, your are not in range of any markets!");
                return;
            }

            // TODO: combine multiple markets to list best Buy and Sell prices that isn't blacklisted.

            var market = markets.FirstOrDefault();
            if (market == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PRICELIST", "That market does not exist.");
                return;
            }

            string reply = null;

            MyAPIGateway.Parallel.StartBackground(delegate ()
            // Background processing occurs within this block.
            {

                try
                {
                    var orderedList = new Dictionary<MarketItemStruct, string>();
                    foreach (var marketItem in market.MarketItems)
                    {
                        if (marketItem.IsBlacklisted)
                            continue;

                        var definition = MyDefinitionManager.Static.GetDefinition(marketItem.TypeId, marketItem.SubtypeName);
                        var name = definition == null ? marketItem.SubtypeName : definition.GetDisplayName();
                        orderedList.Add(marketItem, name);
                    }

                    orderedList = orderedList.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    var str = new SeTextBuilder();
                    str.AppendLine("Market: {0}\r\n", market.DisplayName);
                    str.AddLeftTrim(550, "Item");
                    str.AddRightText(650, "Buy at");
                    str.AddRightText(850, "Sell at");
                    str.AppendLine();

                    foreach (var kvp in orderedList)
                    {
                        // TODO: formatting of numbers, and currency name.
                        str.AddLeftTrim(550, kvp.Value);
                        str.AddRightText(650, kvp.Key.BuyPrice.ToString("0.00", EconomyScript.ServerCulture));
                        str.AddRightText(850, kvp.Key.SellPrice.ToString("0.00", EconomyScript.ServerCulture));
                        str.AppendLine();
                    }
                    reply = str.ToString();
                }
                catch (Exception ex)
                {
                    EconomyScript.Instance.ServerLogger.WriteException(ex);
                    MessageClientTextMessage.SendMessage(SenderSteamId, "PRICELIST", "Failed and died. Please contact the administrator.");
                }
            }, delegate ()
            // when the background processing is finished, this block will run foreground.
            {
                if (reply != null)
                {
                    try
                    {
                        MessageClientDialogMessage.SendMessage(SenderSteamId, "PRICELIST", " ", reply);
                    }
                    catch (Exception ex)
                    {
                        EconomyScript.Instance.ServerLogger.WriteException(ex);
                        MessageClientTextMessage.SendMessage(SenderSteamId, "PRICELIST", "Failed and died. Please contact the administrator.");
                    }
                }
            });
        }
    }
}
