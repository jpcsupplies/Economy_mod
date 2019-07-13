namespace Economy.scripts.Management
{
    using EconConfig;
    using Economy.scripts;
    using Economy.scripts.EconStructures;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    public class LcdManager
    {
        private const string UpdateCrashMessage = "This exception may indicate an error in the game or mod code. If this exception continues to appear, then please contact the game developers.";

        //public static void UpdateLcds()
        //{
        //    if (!EconomyScript.Instance.ServerConfig.EnableLcds)
        //        return;

        //    var players = new List<IMyPlayer>();
        //    MyAPIGateway.Players.GetPlayers(players, p => p != null);
        //    var updatelist = new HashSet<IMyTextPanel>();

        //    foreach (var player in players)
        //    {
        //        // Establish a visual range of the LCD.
        //        // if there are no players closer than this, don't bother updating it.
        //        var sphere = new BoundingSphereD(player.GetPosition(), 75);
        //        var list = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
        //        foreach (var block in list)
        //        {
        //            // TODO: projected ship check?
        //            var textPanel = block as IMyTextPanel;
        //            if (textPanel != null
        //                && textPanel.IsFunctional
        //                && textPanel.IsWorking
        //                && EconomyConsts.LCDTags.Any(tag => textPanel.CustomName.IndexOf(tag, StringComparison.InvariantCultureIgnoreCase) >= 0))
        //            {
        //                updatelist.Add((IMyTextPanel)block);
        //            }
        //        }
        //    }

        //    foreach (var textPanel in updatelist)
        //        ProcessLcdBlock(textPanel);
        //}


        /// <summary>
        /// This forces any Economy enabled LCD to blank it's display.
        /// </summary>
        public static void BlankLcds()
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCubeGrid);

            foreach (var entity in entities)
            {
                var cubeGrid = (IMyCubeGrid)entity;
                if (cubeGrid.Physics == null)
                    continue;

                var blocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blocks, block => block?.FatBlock != null && 
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_TextPanel) && 
                    EconomyConsts.LCDTags.Any(tag => ((IMyTerminalBlock)block.FatBlock).CustomName.IndexOf(tag, StringComparison.InvariantCultureIgnoreCase) >= 0));

                foreach (var block in blocks)
                {
                    var writer = TextPanelWriter.Create((IMyTextPanel)block.FatBlock);
                    writer.AddPublicLine("Economy LCD is disabled");
                    writer.UpdatePublic();
                }
            }
        }

        private enum StartFrom { None, Line, Page };

        public static void ProcessLcdBlock(IMyTextPanel textPanel)
        {
            //counter++;

            var writer = TextPanelWriter.Create(textPanel);

            // Use the update interval on the LCD Panel to determine how often the display is updated.
            // It can only go as fast as the timer calling this code is.
            float interval;
            try
            {
                interval = Math.Max((float)EconomyScript.Instance.ServerConfig.MinimumLcdDisplayInterval, textPanel.GetValueFloat("ChangeIntervalSlider"));
            }
            catch (Exception ex)
            {
                // The game may generate an exception from the GetValueFloat(GetValue) call.
                EconomyScript.Instance.ServerLogger.WriteException(ex, UpdateCrashMessage);
                EconomyScript.Instance.ClientLogger.WriteException(ex, UpdateCrashMessage);
                // We can't safely ignore this one if it doesn't work, because this can affect the display timing.
                return;
            }
            if (writer.LastUpdate > DateTime.Now.AddSeconds(-interval))
                return;

            var checkArray = (textPanel.CustomName + ' ' + textPanel.GetPublicTitle()).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var showAll = false;
            bool showOre = false;
            bool showIngot = false;
            bool showComponent = false;
            bool showAmmo = false;
            bool showTools = false;
            bool showSupplies = false;
            bool showGasses = false;
            bool showStock = false;
            bool showPrices = true;
            bool showTest1 = false;
            bool showTest2 = false;
            StartFrom startFrom = StartFrom.None; //if # is specified eg #20  then run the start line logic
            int startLine = 0; //this is where our start line placeholder sits
            int pageNo = 1;

            // removed Linq, to reduce the looping through the array. This should only have to do one loop through all items in the array.
            foreach (var str in checkArray)
            {
                if (str.Equals("stock", StringComparison.InvariantCultureIgnoreCase))
                    showStock = true;
                if (str.Contains("#"))
                {
                    string[] lineNo = str.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineNo.Length != 0 && int.TryParse(lineNo[0], out startLine))
                    {
                        //this only runs if they put a number in
                        startFrom = StartFrom.Line;
                    }
                }
                if (str.StartsWith("P", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(str.Substring(1), out pageNo))
                        startFrom = StartFrom.Page;
                }
                if (str.Equals("*", StringComparison.InvariantCultureIgnoreCase))
                    showAll = true;
                if (!showAll)
                {
                    if (str.Equals("test1", StringComparison.InvariantCultureIgnoreCase))
                        showTest1 = true;
                    else if (str.Equals("test2", StringComparison.InvariantCultureIgnoreCase))
                        showTest2 = true;
                    else if (str.StartsWith("ore", StringComparison.InvariantCultureIgnoreCase))
                        showOre = true;
                    else if (str.StartsWith("ingot", StringComparison.InvariantCultureIgnoreCase))
                        showIngot = true;
                    else if (str.StartsWith("component", StringComparison.InvariantCultureIgnoreCase))
                        showComponent = true;
                    else if (str.StartsWith("ammo", StringComparison.InvariantCultureIgnoreCase))
                        showAmmo = true;
                    else if (str.StartsWith("tool", StringComparison.InvariantCultureIgnoreCase))
                        showTools = true;
                    else if (str.StartsWith("supplies", StringComparison.InvariantCultureIgnoreCase))
                        showSupplies = true;
                    else if (str.StartsWith("supply", StringComparison.InvariantCultureIgnoreCase))
                        showSupplies = true;
                    else if (str.StartsWith("gas", StringComparison.InvariantCultureIgnoreCase))
                        showGasses = true;
                }
            }

            bool showHelp = !showAll && !showOre && !showIngot && !showComponent && !showAmmo && !showSupplies && !showTools && !showGasses;



            showPrices = !showStock || writer.IsWide;

            if (showTest1)
            {
                Test1(writer);
                writer.UpdatePublic();
                return;
            }
            if (showTest2)
            {
                Test2(writer);
                writer.UpdatePublic();
                return;
            }

            if (showHelp)
            {
                writer.AddPublicLine("Please add a tag to the private or public title.");
                writer.AddPublicLine("ie., * ingot ore component ammo tools.");
                writer.UpdatePublic();
                return;
            }

            var buyColumn = TextPanelWriter.LcdLineWidth - 180;
            var sellColumn = TextPanelWriter.LcdLineWidth - 0;
            var stockColumn = TextPanelWriter.LcdLineWidth - 0;

            if (showPrices && showStock)
            {
                buyColumn = TextPanelWriter.LcdLineWidth - 280;
                sellColumn = TextPanelWriter.LcdLineWidth - 180;
                stockColumn = TextPanelWriter.LcdLineWidth - 0;
            }

            // This might be a costly operation to run.
            var markets = MarketManager.FindMarketsFromLocation(textPanel.WorldMatrix.Translation);
            if (markets.Count == 0)
            {
                writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "« {0} »", EconomyScript.Instance.ServerConfig.TradeNetworkName);
                writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "« No market in range »");
            }
            else
            {
                // TODO: not sure if we should display all markets, the cheapest market item, or the closet market.
                // LOGIC summary: it needs to show the cheapest in stock(in range) sell(to player) price, and the highest (in range) has funds buy(from player) price
                // but this logic depends on the buy/sell commands defaulting to the same buy/sell rules as above.
                // where buy /sell commands run out of funds or supply in a given market and need to pull from the next market
                //it will either have to stop at each price change and notify the player, and/or prompt to keep transacting at each new price, or blindly keep buying until the
                //order is filled, the market runs out of stock, or the money runs out. Blindly is probably not optimal unless we are using stockmarket logic (buy orders/offers)
                //so the prompt option is the safer
                var market = markets.FirstOrDefault();

                // Build a list of the items, so we can get the name so we can the sort the items by name.
                var list = new Dictionary<MarketItemStruct, string>();

                writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, market.DisplayName);

                if (startFrom == StartFrom.Page)
                {
                    // convert the page to lines required.
                    if (pageNo < 1)
                        pageNo = 1;
                    startLine = ((writer.DisplayLines - 2) * (pageNo - 1));
                    startFrom = StartFrom.Line;
                }

                string fromLine = " (From item #" + startLine + ".)";
                writer.AddPublicText("« Market List");
                if (startLine >= 1)
                    writer.AddPublicText(fromLine);
                else
                    startLine = 1; // needed for truncating end line.

                if (showPrices && showStock)
                {
                    writer.AddPublicRightText(buyColumn, "Buy");
                    writer.AddPublicRightText(sellColumn, "Sell");
                    writer.AddPublicRightLine(stockColumn, "Stock »");
                }
                else if (showStock)
                    writer.AddPublicRightLine(stockColumn, "Stock »");
                else if (showPrices)
                {
                    writer.AddPublicRightText(buyColumn, "Buy");
                    writer.AddPublicRightLine(sellColumn, "Sell »");
                }

                foreach (var marketItem in market.MarketItems)
                {
                    if (marketItem.IsBlacklisted || marketItem.SubtypeName == "SpaceCredit" || marketItem.SubtypeName == "Space Credit")
                        continue;

                    MyObjectBuilderType result;
                    if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
                    {
                        var id = new MyDefinitionId(result, marketItem.SubtypeName);
                        var content = Support.ProducedType(id);

                        //if (((Type)id.TypeId).IsSubclassOf(typeof(MyObjectBuilder_GasContainerObject))) // TODO: Not valid call yet.

                        // Cannot check the Type of the item, without having to use MyObjectBuilderSerializer.CreateNewObject().

                        if (showAll ||
                            (showOre && content is MyObjectBuilder_Ore) ||
                            (showIngot && content is MyObjectBuilder_Ingot) ||
                            (showComponent && content is MyObjectBuilder_Component) ||
                            (showAmmo && content is MyObjectBuilder_AmmoMagazine) ||
                            (showSupplies && content is MyObjectBuilder_ConsumableItem) || 
                            (showSupplies && content is MyObjectBuilder_Package) || 
                            (showSupplies && content is MyObjectBuilder_Datapad) || 
                            (showTools && content is MyObjectBuilder_PhysicalGunObject) || // guns, welders, hand drills, grinders.
                            (showGasses && content is MyObjectBuilder_GasContainerObject) || // aka gas bottle.
                            (showGasses && content is MyObjectBuilder_GasProperties))  // Type check here allows mods that inherit from the same type to also appear in the lists.
                        {
                            MyDefinitionBase definition;
                            if (MyDefinitionManager.Static.TryGetDefinition(id, out definition))
                                list.Add(marketItem, definition == null ? marketItem.TypeId + "/" + marketItem.SubtypeName : definition.GetDisplayName());
                        }
                    }
                }
                int line = 0;
                foreach (var kvp in list.OrderBy(k => k.Value))
                {
                    line++;
                    if (startFrom == StartFrom.Line && line < startLine) //if we have a start line specified skip all lines up to that
                        continue;
                    if (startFrom == StartFrom.Line && line - startLine >= writer.WholeDisplayLines - 2)  // counts 2 lines of headers.
                        break; // truncate the display and don't display the text on the bottom edge of the display.
                    writer.AddPublicLeftTrim(buyColumn - 120, kvp.Value);

                    decimal showBuy = kvp.Key.BuyPrice;
                    decimal showSell = kvp.Key.SellPrice;
                    if ((EconomyScript.Instance.ServerConfig.PriceScaling) && (market.MarketId == EconomyConsts.NpcMerchantId))
                    {
                        showBuy = EconDataManager.PriceAdjust(kvp.Key.BuyPrice, kvp.Key.Quantity, PricingBias.Buy);
                        showSell = EconDataManager.PriceAdjust(kvp.Key.SellPrice, kvp.Key.Quantity, PricingBias.Sell);
                    }

                    if (showPrices && showStock)
                    {
                        writer.AddPublicRightText(buyColumn, showBuy.ToString("\t0.000", EconomyScript.ServerCulture));
                        writer.AddPublicRightText(sellColumn, showSell.ToString("\t0.000", EconomyScript.ServerCulture));

                        // TODO: components and tools should be displayed as whole numbers. Will be hard to align with other values.
                        writer.AddPublicRightText(stockColumn, kvp.Key.Quantity.ToString("\t0.0000", EconomyScript.ServerCulture)); // TODO: recheck number of decimal places.
                    }
                    else if (showStock) //does this ever actually run? seems to already be in the above?
                    {
                        // TODO: components and tools should be displayed as whole numbers. Will be hard to align with other values.

                        writer.AddPublicRightText(stockColumn, kvp.Key.Quantity.ToString("\t0.0000", EconomyScript.ServerCulture)); // TODO: recheck number of decimal places.
                    }
                    else if (showPrices)
                    {
                        writer.AddPublicRightText(buyColumn, showBuy.ToString("\t0.000", EconomyScript.ServerCulture));
                        writer.AddPublicRightText(sellColumn, showSell.ToString("\t0.000", EconomyScript.ServerCulture));
                    }
                    writer.AddPublicLine();
                }
            }

            writer.UpdatePublic();
        }

        private static void Test1(TextPanelWriter writer)
        {
            //var lines = (int)((writer.DisplayLines / 2f) - 0.5f);
            //for (int i = 0; i < lines; i++)
            //    writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "|");
            //writer.AddPublicFill("«", '-', "»");
            //for (int i = 0; i < lines; i++)
            //    writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "|");
            //return;

            writer.AddPublicLine("« Economy Test Screen 1 »");
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "{0:N1}/{1:N4}/{2}/{3}", writer.FontSize, writer.WidthModifier, writer.DisplayLines, writer.WholeDisplayLines));
            writer.AddPublicLine("FontSize / WidthModifier / Lines / Whole Lines");

            Testline1(writer, "                |");
            Testline1(writer, "!!!!!!!!!!!!!!!!|");
            Testline1(writer, "77777777|");
            Testline1(writer, "zzzzzzzz|");
            Testline1(writer, "\xe008\xe008\xe008\xe008|");

            writer.AddPublicLine(TextPanelWriter.GetStringTrimmed(138f, "                   0"));
            writer.AddPublicRightLine(138f, "...123");

            //Testline2(writer, "\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008\xe008!!!");
            //Testline2(writer, "77777777777777777777777777777777777777!");
            //Testline2(writer, "                                                                        !");

            writer.AddPublicRightLine(TextPanelWriter.LcdLineWidth, "|");
            writer.AddPublicRightLine(TextPanelWriter.LcdLineWidth, "\xe008|");
            writer.AddPublicRightLine(TextPanelWriter.LcdLineWidth, "123456789|");
            writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "123456789");
        }

        private static void Test2(TextPanelWriter writer)
        {
            writer.AddPublicLine("« Economy Test Screen 2 »");
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "Culture Ietf Tag: {0}", EconomyScript.ServerCulture.IetfLanguageTag));
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "Culture Name: {0}", EconomyScript.ServerCulture.DisplayName));

            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "ShortDatePattern: {0}", EconomyScript.ServerCulture.DateTimeFormat.ShortDatePattern));
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "LongDatePattern: {0}", EconomyScript.ServerCulture.DateTimeFormat.LongDatePattern));
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "NumberPattern: {0}", (12345678.910d).ToString("N", EconomyScript.ServerCulture)));

            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "Local Date: {0:o}", DateTime.Now));
            writer.AddPublicLine(string.Format(EconomyScript.ServerCulture, "Elapsed Session Time: {0}", MyAPIGateway.Session.ElapsedGameTime()));
        }

        private static void Testline1(TextPanelWriter writer, string text)
        {
            var size = TextPanelWriter.MeasureString(text);
            writer.AddPublicText(text);
            writer.AddPublicLine("  " + size.ToString(EconomyScript.ServerCulture));
        }

        private static void Testline2(TextPanelWriter writer, string text)
        {
            var size = TextPanelWriter.MeasureString(text);
            writer.AddPublicLine(text);
            writer.AddPublicLine(size.ToString(EconomyScript.ServerCulture));
        }
    }
}
