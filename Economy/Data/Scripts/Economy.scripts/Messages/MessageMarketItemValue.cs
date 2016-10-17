namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.ModAPI;

    [ProtoContract]
    public class MessageMarketItemValue : MessageBase
    {
        [ProtoMember(1)]
        public string TypeId;

        [ProtoMember(2)]
        public string SubtypeName;

        [ProtoMember(3)]
        public decimal Quantity;

        /// <summary>
        /// The localized Display Name from the Client to be sent back if the processing succeeds.
        /// </summary>
        [ProtoMember(4)]
        public string DisplayName;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Value Request for '{0}:{1}' from '{2}'", TypeId, SubtypeName, SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            var character = player.GetCharacter();
            if (character == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", "You are dead. You get market items values while dead.");
                return;
            }
            var position = ((IMyEntity)character).WorldMatrix.Translation;

            var markets = MarketManager.FindMarketsFromLocation(position);
            if (markets.Count == 0)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", "Sorry, your are not in range of any markets!");
                return;
            }

            // TODO: find market with best Buy price that isn't blacklisted.

            var market = markets.FirstOrDefault();
            if (market == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", "That market does not exist.");
                return;
            }

            // TypeId and SubtypeName are both Case sensitive. Do not Ignore case when comparing these.
            var item = market.MarketItems.FirstOrDefault(e => e.TypeId == TypeId && e.SubtypeName == SubtypeName);
            if (item == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", "Sorry, the items you are trying to value doesn't have a market entry!");
                return;
            }

            string reply;

            if (item.IsBlacklisted)
            {
                reply = "Sorry, the item you tried to get a value for is blacklisted on this server.";
            }
            else
            {
                // TODO: qty may need additional range checking here.
                // Adjust price to reflect stock
                decimal AdjPrice = ReactivePricing.PriceAdjust(item.SellPrice, item.Quantity);

                if (Quantity == 1)
                    // set reply to report back the current buy and sell price only since that is all we asked for
                    reply = String.Format("TRADE - You can buy each '{0}' for {1}, or sell it back for {2} each. Debug {3}", DisplayName, item.SellPrice, item.BuyPrice, AdjPrice); 
                else
                    // value BLAH 12 - we must want to know how much we make/pay for buying/selling 12
                    // set reply to current buy and sell price multiplied by the requested qty.
                    reply = String.Format("TRADE - You can buy {0} '{1}' for {2} or sell it back for {3} each.", Quantity, DisplayName, item.SellPrice * Quantity, item.BuyPrice * Quantity);
            }
            MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", reply);
        }

        public static void SendMessage(string typeId, string subtypeName, decimal quantity, string displayName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketItemValue { TypeId = typeId, SubtypeName = subtypeName, Quantity = quantity, DisplayName = displayName });
        }
    }
}
