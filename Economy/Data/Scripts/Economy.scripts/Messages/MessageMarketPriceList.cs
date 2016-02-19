namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EconConfig;
    using Economy.scripts.EconStructures;
    using Management;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    [ProtoContract]
    public class MessageMarketPriceList : MessageBase
    {
        [ProtoMember(1)]
        public bool ShowOre;

        [ProtoMember(2)]
        public bool ShowIngot;

        [ProtoMember(3)]
        public bool ShowComponent;

        [ProtoMember(4)]
        public bool ShowAmmo;

        [ProtoMember(5)]
        public bool ShowTools;

        [ProtoMember(6)]
        public bool ShowGasses;

        public static void SendMessage(bool showOre, bool showIngot, bool showComponent, bool showAmmo, bool showTools, bool showGasses)
        {
            ConnectionHelper.SendMessageToServer(new MessageMarketPriceList { ShowAmmo = showAmmo, ShowComponent = showComponent, ShowIngot = showIngot, ShowOre = showOre, ShowTools = showTools, ShowGasses = showGasses});
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
                    bool showAll = !ShowOre && !ShowIngot && !ShowComponent && !ShowAmmo && !ShowTools && !ShowGasses;

                    var orderedList = new Dictionary<MarketItemStruct, string>();
                    foreach (var marketItem in market.MarketItems)
                    {
                        if (marketItem.IsBlacklisted)
                            continue;

                        MyObjectBuilderType result;
                        if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
                        {
                            var id = new MyDefinitionId(result, marketItem.SubtypeName);
                            var content = Support.ProducedType(id);

                            // Cannot check the Type of the item, without having to use MyObjectBuilderSerializer.CreateNewObject().

                            if (showAll ||
                                (ShowOre && content is MyObjectBuilder_Ore) ||
                                (ShowIngot && content is MyObjectBuilder_Ingot) ||
                                (ShowComponent && content is MyObjectBuilder_Component) ||
                                (ShowAmmo && content is MyObjectBuilder_AmmoMagazine) ||
                                (ShowTools && content is MyObjectBuilder_PhysicalGunObject) || // guns, welders, hand drills, grinders.
                                (ShowTools && content is MyObjectBuilder_GasContainerObject) || // aka gas bottle.
                                (ShowGasses && content is MyObjectBuilder_GasProperties))
                                // Type check here allows mods that inherit from the same type to also appear in the lists.
                            {
                                var definition = MyDefinitionManager.Static.GetDefinition(marketItem.TypeId, marketItem.SubtypeName);
                                var name = definition == null ? marketItem.SubtypeName : definition.GetDisplayName();
                                orderedList.Add(marketItem, name);
                            }
                        }
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
