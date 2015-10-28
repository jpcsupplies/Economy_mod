namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using ProtoBuf;

    [ProtoContract]
    public class MessageMarketItemValue : MessageBase
    {
        [ProtoMember(1)]
        public ulong MarketId;

        [ProtoMember(2)]
        public string TypeId;

        [ProtoMember(3)]
        public string SubtypeName;

        [ProtoMember(4)]
        public decimal Quantity;

        /// <summary>
        /// The localized Display Name from the Client to be sent back if the processing succeeds.
        /// </summary>
        [ProtoMember(5)]
        public string DisplayName;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.Write("Value Request for '{0}:{1}' from '{2}'", TypeId, SubtypeName, SenderSteamId);

            var market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.MarketId == MarketId);
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

                if (Quantity == 1)
                    // set reply to report back the current buy and sell price only since that is all we asked for
                    reply = String.Format("TRADE - You can buy each '{0}' for {1}, or sell it back for {2} each.", DisplayName, item.SellPrice, item.BuyPrice);
                else
                // value BLAH 12 - we must want to know how much we make/pay for buying/selling 12
                // set reply to current buy and sell price multiplied by the requested qty.
                    reply = String.Format("TRADE - You can buy {0} '{1}' for {2} or sell it back for {3} each.", Quantity, DisplayName, item.SellPrice*Quantity, item.BuyPrice*Quantity);
            }
            MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", reply);
        }

        public static void SendMessage(ulong marketId, string typeId, string subtypeName, decimal quantity, string displayName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketItemValue { MarketId = marketId, TypeId = typeId, SubtypeName = subtypeName, Quantity = quantity, DisplayName = displayName });
        }
    }
}
