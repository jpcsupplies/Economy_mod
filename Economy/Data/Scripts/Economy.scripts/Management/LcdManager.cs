namespace Economy.scripts.Management
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Economy.scripts;
    using EconConfig;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class LcdManager
    {
        #region fields

        //private static int counter = 0;

        #endregion

        public static void UpdateLcds()
        {
            //counter = 0;

            #region sample 1
            //// scan through all ships, and all cubes.

            //var start1 = DateTime.Now;
            //var entities = new HashSet<IMyEntity>();
            //MyAPIGateway.Entities.GetEntities(entities, e => e is Sandbox.ModAPI.IMyCubeGrid);
            //// TODO: projected ship check?
            //foreach (var entity in entities)
            //{
            //    var cubeGrid = (Sandbox.ModAPI.IMyCubeGrid)entity;
            //    var blocks = new List<Sandbox.ModAPI.IMySlimBlock>();
            //    cubeGrid.GetBlocks(blocks, block => block != null && block.FatBlock != null &&
            //        block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_TextPanel) &&
            //        ((Sandbox.ModAPI.IMyTerminalBlock)block.FatBlock).CustomName.IndexOf("[Economy]", StringComparison.InvariantCultureIgnoreCase) >= 0);
            //    foreach (var block in blocks)
            //        ProcessLcdBlock((IMyTextPanel)block.FatBlock);
            //}
            //var time1 = DateTime.Now - start1;

            #endregion

            //MyAPIGateway.Utilities.ShowMessage("Count A", "{0} {1}", counter, time1);
            //counter = 0;

            #region Sample 2

            //var start2 = DateTime.Now;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p != null);
            var updatelist = new HashSet<IMyTextPanel>();
            foreach (var player in players)
            {
                // Establish a visual range of the LCD.
                // if there are no players closer than this, don't bother updating it.
                var sphere = new BoundingSphereD(player.GetPosition(), 75);
                var list = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                foreach (var block in list)
                {
                    var textPanel = block as IMyTextPanel;
                    if (textPanel != null 
                        && textPanel.IsFunctional
                        && textPanel.IsWorking 
                        && textPanel.CustomName.IndexOf("[Economy]", StringComparison.InvariantCultureIgnoreCase) >= 0)
                        updatelist.Add((IMyTextPanel)block);
                }
            }

            foreach (var textPanel in updatelist)
            {
                var interval = Math.Max(1f, textPanel.GetValueFloat("ChangeIntervalSlider"));
                ProcessLcdBlock(textPanel);
            }

            //var time2 = DateTime.Now - start2;

            #endregion

            //MyAPIGateway.Utilities.ShowMessage("Count B", "{0} {1}", counter, time2);
            //counter = 0;
        }

        private static void ProcessLcdBlock(IMyTextPanel textPanel)
        {
            //counter++;

            var check1 = textPanel.GetPublicTitle().Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var check2 = textPanel.GetPrivateTitle().Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var showAll = check1.Any(e => e.Equals("*", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("*", StringComparison.InvariantCultureIgnoreCase));

            bool showOre = false;
            bool showIngot = false;
            bool showComponent = false;
            bool showAmmo = false;
            bool showTest = check1.Any(e => e.Equals("test", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("test", StringComparison.InvariantCultureIgnoreCase));

            if (!showAll)
            {
                showOre = check1.Any(e => e.Equals("ore", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("ore", StringComparison.InvariantCultureIgnoreCase));
                showIngot = check1.Any(e => e.Equals("ingot", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("ingot", StringComparison.InvariantCultureIgnoreCase));
                showComponent = check1.Any(e => e.Equals("component", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("component", StringComparison.InvariantCultureIgnoreCase));
                showAmmo = check1.Any(e => e.Equals("ammo", StringComparison.InvariantCultureIgnoreCase)) || check2.Any(e => e.Equals("ammo", StringComparison.InvariantCultureIgnoreCase));
            }

            bool showHelp = !showAll && !showOre && !showIngot && !showComponent & !showAmmo;

            var writer = TextPanelWriter.Create(textPanel);

            if (showTest)
            {
                Test(writer);
                writer.UpdatePublic();
                return;
            }

            if (showHelp)
            {
                writer.AddPublicLine("Please add a tag to the private or public title.");
                writer.AddPublicLine("ie., * ingot ore component ammo.");
                writer.UpdatePublic();
                return;
            }

            var buyColumn = TextPanelWriter.LcdLineWidth - 180;
            var sellColumn = TextPanelWriter.LcdLineWidth - 0;

            writer.AddPublicText("« Market List");
            writer.AddPublicRightText(buyColumn, "Buy");
            writer.AddPublicRightLine(sellColumn, "Sell »");

            // Build a list of the items, so we can get the name so we can the sort the items by name.
            var list = new Dictionary<MarketStruct, string>();
            foreach (var marketItem in EconomyScript.Instance.Data.MarketItems)
            {
                if (marketItem.IsBlacklisted)
                    continue;

                if (showAll ||
                    (showOre && marketItem.TypeId == "MyObjectBuilder_Ore") ||
                    (showIngot && marketItem.TypeId == "MyObjectBuilder_Ingot") ||
                    (showComponent && marketItem.TypeId == "MyObjectBuilder_Component") ||
                    (showAmmo && marketItem.TypeId == "MyObjectBuilder_AmmoMagazine"))
                {
                    MyPhysicalItemDefinition definition = null;
                    MyObjectBuilderType result;
                    if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
                    {
                        var id = new MyDefinitionId(result, marketItem.SubtypeName);
                        MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
                    }

                    list.Add(marketItem, definition == null ? marketItem.TypeId + "/" + marketItem.SubtypeName : definition.GetDisplayName());
                }
            }

            foreach (var kvp in list.OrderBy(k => k.Value))
            {
                writer.AddPublicLeftTrim(buyColumn - 120, kvp.Value);
                writer.AddPublicRightText(buyColumn, kvp.Key.BuyPrice.ToString("0.00"));
                writer.AddPublicRightText(sellColumn, kvp.Key.SellPrice.ToString("0.00"));
                writer.AddPublicLine();
            }

            writer.UpdatePublic();
        }

        private static void Test(TextPanelWriter writer)
        {
            //var lines = (int)((writer.DisplayLines / 2f) - 0.5f);
            //for (int i = 0; i < lines; i++)
            //    writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "|");
            //writer.AddPublicFill("«", '-', "»");
            //for (int i = 0; i < lines; i++)
            //    writer.AddPublicCenterLine(TextPanelWriter.LcdLineWidth / 2f, "|");
            //return;

            writer.AddPublicLine(string.Format("'{0}' '{1}' '{2}'", writer.FontSize, writer.WidthModifier, writer.DisplayLines));

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

        private static void Testline1(TextPanelWriter writer, string text)
        {
            var size = TextPanelWriter.MeasureString(text);
            writer.AddPublicText(text);
            writer.AddPublicLine("  " + size.ToString());
        }

        private static void Testline2(TextPanelWriter writer, string text)
        {
            var size = TextPanelWriter.MeasureString(text);
            writer.AddPublicLine(text);
            writer.AddPublicLine(size.ToString());
        }
    }
}
