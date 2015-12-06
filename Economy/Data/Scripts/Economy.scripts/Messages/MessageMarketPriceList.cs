using System.Collections.Generic;
using Economy.scripts.EconStructures;

namespace Economy.scripts.Messages
{
    using System.Linq;
    using System.Text;
    using EconConfig;
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

            var reply = new StringBuilder();
            reply.AppendFormat("Market: {0}\r\n", market.DisplayName);

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

            foreach (var kvp in orderedList)
            {
                // TODO: formatting of numbers, and currency name.
                reply.AppendFormat("{0} Buy:{1}  Sell:{2}\r\n", kvp.Value, kvp.Key.BuyPrice, kvp.Key.SellPrice);
            }

            MessageClientDialogMessage.SendMessage(SenderSteamId, "PRICELIST", " ", reply.ToString());
        }
    }
}
