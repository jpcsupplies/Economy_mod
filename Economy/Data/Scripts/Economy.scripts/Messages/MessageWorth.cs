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
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

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
            EconomyScript.Instance.ServerLogger.Write("Worth Request for {0} from '{1}'", EntityId, SenderSteamId);

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

            // TODO: counters.
            int terminalBlocks = 0;
            int armorBlocks = 0;
            decimal totalValue = 0;
            int gridCount = 0;

            var accumulatedComponents = new Dictionary<MyDefinitionId, decimal>();

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

                                if (!accumulatedComponents.ContainsKey(component.Definition.Id))
                                    accumulatedComponents.Add(component.Definition.Id, 0);
                                accumulatedComponents[component.Definition.Id] += component.Count;
                            }

                            // This will subtract off components missing from a partially built cube.
                            // This also includes the Construction Inventory.
                            var missingComponents = new Dictionary<string, int>();
                            block.GetMissingComponents(missingComponents);
                            foreach (var kvp in missingComponents)
                            {
                                var definitionid = new MyDefinitionId(typeof(MyObjectBuilder_Component), kvp.Key);
                                accumulatedComponents[definitionid] -= kvp.Value;
                            }

                            #endregion

                            #region Go through all other Inventories for components/items.

                            if (block.FatBlock != null)
                            {
                                var cube = (Sandbox.Game.Entities.MyEntity)block.FatBlock;
                                for (var i = 0; i < cube.InventoryCount; i++)
                                {
                                    var inventory = cube.GetInventory(i);
                                    var list = inventory.GetItems();
                                    foreach (var item in list)
                                    {
                                        var id = item.Content.GetId();
                                        if (!accumulatedComponents.ContainsKey(id))
                                            accumulatedComponents.Add(id, 0);
                                        accumulatedComponents[id] += (decimal)item.Amount;
                                    }
                                }
                            }

                            #endregion

                            // TODO: Go through Gasses for tanks and bottles and cockpits.

                            foreach (var kvp in accumulatedComponents)
                            {
                                //EconomyScript.Instance.ServerLogger.Write("Component Count '{0}' '{1}' x {2}.", kvp.Key.TypeId, kvp.Key.SubtypeName, kvp.Value);

                                var item = market.MarketItems.FirstOrDefault(e => e.TypeId == kvp.Key.TypeId.ToString() && e.SubtypeName == kvp.Key.SubtypeName);
                                if (item == null)
                                {
                                    EconomyScript.Instance.ServerLogger.Write("Component Item could not be found in Market for Worth '{0}' '{1}'.", kvp.Key.TypeId, kvp.Key.SubtypeName);
                                    // can ignore for worth.
                                }
                                else
                                {
                                    totalValue += kvp.Value * item.SellPrice; // TODO: check if we use the sell or buy price.
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EconomyScript.Instance.ServerLogger.WriteException(ex);
                }

                TextLogger.WriteGameLog("## Econ ## Worth:background end");
            }, delegate ()
            // when the background processing is finished, this block will run foreground.
            {
                TextLogger.WriteGameLog("## Econ ## Worth:foreground");

                var str = new StringBuilder();
                str.AppendFormat("Armors={0}   Terminals={1}  Grids={2}\r\n", armorBlocks, terminalBlocks, gridCount);

                foreach (var kvp in accumulatedComponents)
                {
                    MyPhysicalItemDefinition definition = null;
                    MyDefinitionManager.Static.TryGetPhysicalItemDefinition(kvp.Key, out definition);
                    str.AppendFormat("'{0}'  x {1}.\r\n", definition.GetDisplayName(), kvp.Value);
                }

                var prefix = string.Format("{0:#,##0.00000}", totalValue);

                str.AppendLine("-----------------------------------");

                str.AppendFormat(" Ship: {0}\r\n", selectedShip.DisplayName);
                str.AppendFormat(" Final Value: {0:#,##0.00000} {1}\r\n", totalValue, EconomyScript.Instance.Config.CurrencyName);
                MessageClientDialogMessage.SendMessage(SenderSteamId, "WORTH", prefix, str.ToString());
            });
        }
    }
}
