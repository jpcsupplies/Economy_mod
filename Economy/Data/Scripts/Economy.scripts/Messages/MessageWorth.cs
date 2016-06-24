namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EconConfig;
    using Economy.scripts;
    using Economy.scripts.EconStructures;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
   // using IMyOxygenTank = Sandbox.ModAPI.Ingame.IMyOxygenTank;

    /// <summary>
    /// Will value a grid (ship or station), including attached rotor and piston parts.
    /// Will not include landing gear or connector parts.
    /// </summary>
    [ProtoContract]
    public class MessageWorth : MessageBase
    {
        [ProtoMember(1)]
        public long EntityId;

        public static void SendMessage(long entityId)
        {
            ConnectionHelper.SendMessageToServer(new MessageWorth { EntityId = entityId });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Worth Request for {0} from '{1}'", EntityId, SenderSteamId);

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            var character = player.GetCharacter();
            if (character == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "You are dead. You get market items values while dead.");
                return;
            }
            var position = ((IMyEntity)character).WorldMatrix.Translation;

            var markets = MarketManager.FindMarketsFromLocation(position);

            // TODO: find market with best Buy price that isn't blacklisted.

            var market = markets.FirstOrDefault();
            string marketDetail = null;
            if (market == null)
            {
                market = EconomyScript.Instance.Data.Markets.FirstOrDefault(m => m.MarketId == EconomyConsts.NpcMerchantId);
                if (market != null)
                    marketDetail = string.Format("No markets in range, using default market '{0}' for appraisal.", market.DisplayName);
            }

            if (market == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "That market does not exist.");
                return;
            }

            if (marketDetail == null)
                marketDetail = string.Format("Using market '{0}' for appraisal.", market.DisplayName);

            if (!MyAPIGateway.Entities.EntityExists(EntityId))
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "Sorry, the entity no longer exists!");
                return;
            }

            var selectedShip = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeGrid;

            if (selectedShip == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "Sorry, the entity no longer exists!");
                return;
            }

            int terminalBlocks = 0;
            int armorBlocks = 0;
            decimal shipValue = 0;
            decimal inventoryValue = 0;
            int gridCount = 0;

            var gridComponents = new Dictionary<MyDefinitionId, decimal>();
            var inventoryComponents = new Dictionary<MyDefinitionId, decimal>();

            MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "Calculating the worth...");

            MyAPIGateway.Parallel.StartBackground(delegate ()
            // Background processing occurs within this block.
            {
                TextLogger.WriteGameLog("## Econ ## Worth:background start");
                //EconomyScript.Instance.ServerLogger.Write("Validating and Updating Config.");

                try
                {
                    var grids = selectedShip.GetAttachedGrids(AttachedGrids.Static);
                    gridCount = grids.Count;
                    foreach (var grid in grids)
                    {
                        var blocks = new List<IMySlimBlock>();
                        grid.GetBlocks(blocks);

                        foreach (var block in blocks)
                        {
                            MyCubeBlockDefinition blockDefintion;
                            if (block.FatBlock == null)
                            {
                                armorBlocks++;
                                blockDefintion = MyDefinitionManager.Static.GetCubeBlockDefinition(block.GetObjectBuilder());
                            }
                            else
                            {
                                terminalBlocks++;
                                blockDefintion = MyDefinitionManager.Static.GetCubeBlockDefinition(block.FatBlock.BlockDefinition);
                            }

                            //EconomyScript.Instance.ServerLogger.Write("Cube Worth '{0}' '{1}' {2} {3}.", blockDefintion.Id.TypeId, blockDefintion.Id.SubtypeName, block.BuildIntegrity, block.BuildLevelRatio);

                            #region Go through component List based on construction level.

                            foreach (var component in blockDefintion.Components)
                            {
                                //EconomyScript.Instance.ServerLogger.Write("Component Worth '{0}' '{1}' x {2}.", component.Definition.Id.TypeId, component.Definition.Id.SubtypeName, component.Count);

                                if (!gridComponents.ContainsKey(component.Definition.Id))
                                    gridComponents.Add(component.Definition.Id, 0);
                                gridComponents[component.Definition.Id] += component.Count;
                            }

                            // This will subtract off components missing from a partially built cube.
                            // This also includes the Construction Inventory.
                            var missingComponents = new Dictionary<string, int>();
                            block.GetMissingComponents(missingComponents);
                            foreach (var kvp in missingComponents)
                            {
                                var definitionid = new MyDefinitionId(typeof(MyObjectBuilder_Component), kvp.Key);
                                gridComponents[definitionid] -= kvp.Value;
                            }

                            #endregion

                            if (block.FatBlock != null)
                            {
                                var cube = (MyEntity)block.FatBlock;

                                #region Go through Gasses for tanks and cockpits.

                                var tank = cube as IMyOxygenTank;
                                var gasTankDefintion = blockDefintion as MyGasTankDefinition;

                                if (gasTankDefintion != null && tank != null)
                                {
                                    decimal volume = (decimal)gasTankDefintion.Capacity * (decimal)tank.GetOxygenLevel();
                                    if (!inventoryComponents.ContainsKey(gasTankDefintion.StoredGasId))
                                        inventoryComponents.Add(gasTankDefintion.StoredGasId, 0);
                                    inventoryComponents[gasTankDefintion.StoredGasId] += volume;
                                    //MessageClientTextMessage.SendMessage(SenderSteamId, "GAS tank", "{0} detected {1}", gasTankDefintion.StoredGasId, volume);
                                }

                                // Check through Cockpits.
                                var cockpit = cube as Sandbox.Game.Entities.MyCockpit;  // For some reason, the o2 is on the MyCockpit Class. There is no Interface.
                                if (cockpit != null)
                                {
                                    // Hardcoded, because Oxygen and Hydrogen do not have available defintions.
                                    var oxygenDefintion = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
                                    if (!inventoryComponents.ContainsKey(oxygenDefintion))
                                        inventoryComponents.Add(oxygenDefintion, 0);
                                    inventoryComponents[oxygenDefintion] += (decimal)cockpit.OxygenAmount;
                                    //MessageClientTextMessage.SendMessage(SenderSteamId, "COCKPIT tank", "{0} detected {1}", null, cockpit.OxygenAmount);
                                }

                                #endregion

                                #region Go through all other Inventories for components/items.

                                // Inventory check based on normal game access.
                                var relation = block.FatBlock.GetUserRelationToOwner(player.PlayerID);
                                if (relation != MyRelationsBetweenPlayerAndBlock.Enemies
                                    && relation != MyRelationsBetweenPlayerAndBlock.Neutral)
                                {
                                    for (var i = 0; i < cube.InventoryCount; i++)
                                    {
                                        var inventory = cube.GetInventory(i);
                                        var list = inventory.GetItems();
                                        foreach (var item in list)
                                        {
                                            var id = item.Content.GetId();
                                            if (!inventoryComponents.ContainsKey(id))
                                                inventoryComponents.Add(id, 0);
                                            inventoryComponents[id] += (decimal)item.Amount;

                                            // Go through Gas bottles.
                                            var gasContainer = item.Content as MyObjectBuilder_GasContainerObject;
                                            if (gasContainer != null)
                                            {
                                                var defintion = (MyOxygenContainerDefinition)MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content.GetId());
                                                decimal volume = (decimal)defintion.Capacity * (decimal)gasContainer.GasLevel;
                                                if (!inventoryComponents.ContainsKey(defintion.StoredGasId))
                                                    inventoryComponents.Add(defintion.StoredGasId, 0);
                                                inventoryComponents[defintion.StoredGasId] += volume;
                                                //MessageClientTextMessage.SendMessage(SenderSteamId, "GAS bottle", "{0} detected {1}", defintion.StoredGasId, volume);
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                        }
                    }
                    shipValue += SumComponents(market, gridComponents);
                    inventoryValue += SumComponents(market, inventoryComponents);
                }
                catch (Exception ex)
                {
                    EconomyScript.Instance.ServerLogger.WriteException(ex);
                    MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "Failed and died. Please contact the administrator.");
                }

                TextLogger.WriteGameLog("## Econ ## Worth:background end");
            }, delegate ()
            // when the background processing is finished, this block will run foreground.
            {
                TextLogger.WriteGameLog("## Econ ## Worth:foreground");

                try
                {
                    var str = new StringBuilder();

                    //foreach (var kvp in gridComponents)
                    //{
                    //    MyDefinitionBase definition = null;
                    //    MyDefinitionManager.Static.TryGetDefinition(kvp.Key, out definition);
                    //    str.AppendFormat("'{0}' x {1}.\r\n", definition == null ? kvp.Key.SubtypeName : definition.GetDisplayName(), kvp.Value);
                    //}

                    //foreach (var kvp in inventoryComponents)
                    //{
                    //    MyDefinitionBase definition = null;
                    //    MyDefinitionManager.Static.TryGetDefinition(kvp.Key, out definition);
                    //    str.AppendFormat("'{0}' x {1}.\r\n", definition == null ? kvp.Key.SubtypeName : definition.GetDisplayName(), kvp.Value);
                    //}

                    //var prefix = string.Format("{0:#,##0.00000}", totalValue);

                    str.AppendLine(marketDetail);
                    str.AppendFormat("{0}: {1}\r\n", selectedShip.IsStatic ? "Station" : selectedShip.GridSizeEnum.ToString() + " Ship", selectedShip.DisplayName);
                    str.AppendFormat("Grids={2}\r\nArmor Blocks={0}\r\nTerminal Blocks={1}\r\n", armorBlocks, terminalBlocks, gridCount);
                    str.AppendLine("-----------------------------------");
                    str.AppendFormat("Ship Value: {0:#,##0.00000} {1}.\r\n", shipValue, EconomyScript.Instance.ServerConfig.CurrencyName);
                    str.AppendFormat("Inventory Value: {0:#,##0.00000} {1}.\r\n", inventoryValue, EconomyScript.Instance.ServerConfig.CurrencyName);
                    str.AppendFormat("Final Value: {0:#,##0.00000} {1}.\r\n", shipValue + inventoryValue, EconomyScript.Instance.ServerConfig.CurrencyName);
                    MessageClientDialogMessage.SendMessage(SenderSteamId, "WORTH", selectedShip.DisplayName, str.ToString());
                }
                catch (Exception ex)
                {
                    EconomyScript.Instance.ServerLogger.WriteException(ex);
                    MessageClientTextMessage.SendMessage(SenderSteamId, "WORTH", "Failed and died. Please contact the administrator.");
                }
            });
        }

        private static decimal SumComponents(MarketStruct market, Dictionary<MyDefinitionId, decimal> accumulatedComponents)
        {
            decimal total = 0;
            foreach (var kvp in accumulatedComponents)
            {
                //EconomyScript.Instance.ServerLogger.Write("Component Count '{0}' '{1}' x {2}.", kvp.Key.TypeId, kvp.Key.SubtypeName, kvp.Value);

                var item = market.MarketItems.FirstOrDefault(e => e.TypeId == kvp.Key.TypeId.ToString() && e.SubtypeName == kvp.Key.SubtypeName);
                if (item == null)
                {
                    EconomyScript.Instance.ServerLogger.WriteWarning("Component Item could not be found in Market for Worth '{0}' '{1}'.", kvp.Key.TypeId, kvp.Key.SubtypeName);
                    // can ignore for worth.
                }
                else
                {
                    total += kvp.Value * item.SellPrice; // TODO: check if we use the sell or buy price.
                }
            }
            return total;
        }
    }
}
