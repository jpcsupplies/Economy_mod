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
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.ModAPI;

    /// <summary>
    /// Controls processing of buy and selling of a ship or station, including attached rotor and piston parts.
    /// Will not include landing gear or connector parts.
    /// </summary>
    [ProtoContract]
    public class MessageShipSale : MessageBase
    {
        [ProtoMember(201)]
        public long EntityId;

        [ProtoMember(202)]
        public string Ctype;

        [ProtoMember(203)]
        public decimal Amount;

        public static void SendMessage(long entityId, string ctype, decimal amount)
        {
            ConnectionHelper.SendMessageToServer(new MessageShipSale { EntityId = entityId, Ctype = ctype, Amount = amount, });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("ShipSale Request for {0} from '{1}'", EntityId, SenderSteamId);

            if (!EconomyScript.Instance.ServerConfig.ShipTrading)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Trading of ships is not enabled.");
                return;
            }

            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
            var character = player.GetCharacter();
            if (character == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "You are dead. You cant trade ships while dead.");
                return;
            }

            if (!MyAPIGateway.Entities.EntityExists(EntityId))
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Sorry, the entity no longer exists!");
                return;
            }

            var selectedShip = MyAPIGateway.Entities.GetEntityById(EntityId) as IMyCubeGrid;

            if (selectedShip == null)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Sorry, the entity no longer exists!");
                return;
            }
            if (Ctype == "sell")
            {
                if (Amount > 0)
                {
                    decimal amount = 0;
                    var check = ShipManager.CheckSellOrder(selectedShip.EntityId);
                    if (check == 0)
                    {
                        int terminalBlocks = 0;
                        int armorBlocks = 0;
                        int gridCount = 0;
                        int owned = 0;

                        MyAPIGateway.Parallel.StartBackground(delegate ()
                        // Background processing occurs within this block.
                        {
                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:background start");

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
                                        if (block.FatBlock == null)
                                        {
                                            armorBlocks++;
                                        }
                                        else
                                        {
                                            if (block.FatBlock.OwnerId != 0)
                                            {
                                                terminalBlocks++;
                                                if (block.FatBlock.OwnerId == player.IdentityId)
                                                    owned++;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                EconomyScript.Instance.ServerLogger.WriteException(ex);
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ShipSale", "Failed and died. Please contact the administrator.");
                            }

                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:background end");
                        }, delegate ()
                        // when the background processing is finished, this block will run foreground.
                        {
                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:foreground");

                            try
                            {
                                if (owned > (terminalBlocks * (EconomyConsts.ShipOwned / 100)))
                                {
                                    ShipManager.CreateSellOrder(player.IdentityId, SenderSteamId, selectedShip.EntityId, Amount);
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship put up for sale for " + Amount);
                                }
                                else
                                {
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "You need to own more than {0}% of the ship to sell it.", EconomyConsts.ShipOwned);
                                }
                            }
                            catch (Exception ex)
                            {
                                EconomyScript.Instance.ServerLogger.WriteException(ex);
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ShipSale", "Failed and died. Please contact the administrator.");
                            }
                        });
                    }
                    else
                        MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship already for sale for " + check + ".");
                }
                else
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "You need to specify a price");
                }
            }
            else if (Ctype == "cancel")
            {
                var check = ShipManager.CheckSellOrder(selectedShip.EntityId);
                if (check != 0)
                {
                    var owner = ShipManager.GetOwner(selectedShip.EntityId);
                    if (owner == SenderSteamId)
                    {
                        var removed = ShipManager.Remove(selectedShip.EntityId, SenderSteamId);
                        if (removed)
                            MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship sale Removed.");
                    }
                    else
                        MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Your are not the sale creator.");
                }
                else
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship not for sale.");
            }
            else if (Ctype == "buy")
            {
                var check = ShipManager.CheckSellOrder(selectedShip.EntityId);
                if (check != 0)
                {
                    if (check == Amount)
                    {
                        int terminalBlocks = 0;
                        int armorBlocks = 0;
                        int gridCount = 0;
                        int owned = 0;


                        MyAPIGateway.Parallel.StartBackground(delegate ()
                        // Background processing occurs within this block.
                        {
                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:background start");

                            try
                            {
                                var grids = selectedShip.GetAttachedGrids(AttachedGrids.Static);
                                gridCount = grids.Count;
                                var owner = ShipManager.GetOwner(selectedShip.EntityId);
                                var ownerid = ShipManager.GetOwnerId(selectedShip.EntityId);
                                foreach (var grid in grids)
                                {
                                    var blocks = new List<IMySlimBlock>();
                                    grid.GetBlocks(blocks);

                                    foreach (var block in blocks)
                                    {
                                        if (block.FatBlock == null)
                                        {
                                            armorBlocks++;
                                        }
                                        else
                                        {
                                            if (block.FatBlock.OwnerId != 0)
                                            {
                                                terminalBlocks++;
                                                if (block.FatBlock.OwnerId == ownerid)
                                                    owned++;
                                            }
                                        }
                                    }
                                }

                                // this checks the ownership, to make sure the seller (still) owns more than half of the terminal blocks at the time of purchase by the new owner.
                                if (owned > (terminalBlocks / 2))
                                {
                                    var accountseller = AccountManager.FindAccount(owner);
                                    var accountbuyer = AccountManager.FindAccount(SenderSteamId);
                                    if (accountbuyer.BankBalance >= Amount)
                                    {
                                        accountbuyer.BankBalance -= Amount;
                                        accountbuyer.Date = DateTime.Now;

                                        accountseller.BankBalance += Amount;
                                        accountseller.Date = DateTime.Now;

                                        MessageUpdateClient.SendAccountMessage(accountbuyer);
                                        MessageUpdateClient.SendAccountMessage(accountseller);

                                        // Using the identity list is a crap way, but since we don't have access to BuiltBy for non-functional blocks, this has to do.
                                        var listIdentites = new List<IMyIdentity>();
                                        MyAPIGateway.Players.GetAllIdentites(listIdentites);

                                        foreach (var grid in grids)
                                        {
                                            grid.ChangeGridOwnership(player.IdentityId, MyOwnershipShareModeEnum.Faction);

                                            foreach (IMyIdentity identity in listIdentites)
                                            {
                                                if (identity.IdentityId != player.IdentityId)
                                                {
                                                    // The current API doesn't allow the setting of the BuiltBy to anything but an existing Identity (player or NPC).
                                                    // This also doesn't appear to sync to all clients, but it remains valid on the server.
                                                    ((MyCubeGrid)grid).TransferBlocksBuiltByID(identity.IdentityId, player.IdentityId);
                                                }
                                            }
                                        }
                                        var removed = ShipManager.Remove(selectedShip.EntityId, owner);
                                        MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship purchased.");
                                    }
                                    else
                                    {
                                        MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "You cant afford that.");
                                    }
                                }
                                else
                                {
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "The seller no longer owns more than 50% of the ship.");
                                }

                            }
                            catch (Exception ex)
                            {
                                EconomyScript.Instance.ServerLogger.WriteException(ex);
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ShipSale", "Failed and died. Please contact the administrator.");
                            }

                            // remove from sale
                            // remove money and give previous owner

                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:background end");
                        }, delegate ()
                        // when the background processing is finished, this block will run foreground.
                        {
                            EconomyScript.Instance.ServerLogger.WriteInfo("ShipSale:foreground");

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
                                var shipSale = ShipManager.CheckSellOrder(selectedShip.EntityId);

                                // stations can be both large and small grids now.
                                str.AppendFormat("{0}: {1}\r\n", selectedShip.GridSizeEnum + (selectedShip.IsStatic ? "Station" : " Ship"), selectedShip.DisplayName);
                                str.AppendFormat("Grids={2}\r\nArmor Blocks={0}\r\nTerminal Blocks={1}\r\n", armorBlocks, terminalBlocks, gridCount);
                                str.AppendLine("-----------------------------------");
                                if (shipSale != 0)
                                    str.AppendFormat("Sale Price: {0:#,##0.00000} {1}.\r\n", shipSale, EconomyScript.Instance.ServerConfig.CurrencyName);
                                else
                                    str.AppendLine("Sale Price: Not for Sale.\r\n");
                                //	MessageClientDialogMessage.SendMessage(SenderSteamId, "ShipSale", selectedShip.DisplayName, str.ToString());
                            }
                            catch (Exception ex)
                            {
                                EconomyScript.Instance.ServerLogger.WriteException(ex);
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ShipSale", "Failed and died. Please contact the administrator.");
                            }
                        });
                    }
                    else
                    {
                        MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship is on sale for " + check);
                    }
                }
                else
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SHIPSALE", "Ship not for sale");
                }
            }
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
