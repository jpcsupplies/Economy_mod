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
            EconomyScript.Instance.ServerLogger.Write("Value Request for '{0}:{1}' from '{2}'", TypeId, SubtypeName, SenderSteamId);

            // TypeId and SubtypeName are both Case sensitive. Do not Ignore case when comparing these.
            var item = EconomyScript.Instance.Data.MarketItems.FirstOrDefault(e => e.TypeId == TypeId && e.SubtypeName == SubtypeName);
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
                    reply = String.Format("you can sell each '{0}' for {1}, or buy it for {2} each.", DisplayName, item.SellPrice, item.BuyPrice);
                else
                // value BLAH 12 - we must want to know how much we make/pay for buying/selling 12
                // set reply to current buy and sell price multiplied by the requested qty.
                    reply = String.Format("you can sell {0} '{1}' for {2} or buy it for {3} each.", Quantity, DisplayName, item.SellPrice*Quantity, item.BuyPrice*Quantity);
            }
            MessageClientTextMessage.SendMessage(SenderSteamId, "VALUE", reply);
        }

        public static void SendMessage(string typeId, string subtypeName, decimal quantity, string displayName)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketItemValue { TypeId = typeId, SubtypeName = subtypeName, Quantity = quantity, DisplayName = displayName });
        }
    }
}
