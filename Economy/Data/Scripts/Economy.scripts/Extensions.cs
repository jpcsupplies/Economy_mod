namespace Economy.scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public static class Extensions
    {
        /// <summary>
        /// Determines if the player is an Administrator of the active game session.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>True if is specified player is an Administrator in the active game.</returns>
        public static bool IsAdmin(this IMyPlayer player)
        {
            // Offline mode. You are the only player.
            if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE)
            {
                return true;
            }

            // Hosted game, and the player is hosting the server.
            if (player.IsHost())
            {
                return true;
            }

            // determine if client is admin of Dedicated server.
            var clients = MyAPIGateway.Session.GetCheckpoint("null").Clients;
            if (clients != null)
            {
                var client = clients.FirstOrDefault(c => c.SteamId == player.SteamUserId && c.IsAdmin);
                return client != null;
                // If user is not in the list, automatically assume they are not an Admin.
            }

            // clients is null when it's not a dedicated server.
            // Otherwise Treat everyone as Normal Player.

            return false;
        }

        public static void ShowMessage(this IMyUtilities utilities, string sender, string messageText, params object[] args)
        {
            utilities.ShowMessage(sender, string.Format(messageText, args));
        }

        /// <summary>
        /// Determines if the player is an Author/Creator.
        /// This is used expressly for debugging and testing of commands.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsExperimentalCreator(this IMyPlayer player)
        {
            switch (player.SteamUserId)
            {
                case 76561197961224864L:
                    return true;
                case 76561197968837138L:
                    return true;
            }

            return false;
        }

        public static IMyPlayer Player(this IMyIdentity identity)
        {
            var listPlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listPlayers, p => p.PlayerID == identity.PlayerId);
            return listPlayers.FirstOrDefault();
        }

        public static IMyIdentity Identity(this IMyPlayer player)
        {
            var listIdentites = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(listIdentites, p => p.IdentityId == player.IdentityId);
            return listIdentites.FirstOrDefault();
        }

        /// <summary>
        /// Used to find the Character Entity (which is the physical representation in game) from the Player (the network connected human).
        /// This is a kludge as a proper API doesn't exist, even though the game code could easily expose this and save all this processing we are forced to do.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static IMyCharacter GetCharacter(this IMyPlayer player)
        {
            var character = player.Controller.ControlledEntity as IMyCharacter;
            if (character != null)
                return character;

            var cubeBlock = player.Controller.ControlledEntity as IMyCubeBlock;
            if (cubeBlock == null)
                return null;

            var controller = cubeBlock as Sandbox.Game.Entities.MyShipController;
            if (controller != null)
                return controller.Pilot;

            // TODO: test conditions for Cryochamber block.

            // Cannot determine Character controlling MyLargeTurretBase as class is internal.
            // TODO: find if the player is controlling a turret.

            //var charComponent = cubeBlock.Components.Get<MyCharacterComponent>();

            //if (charComponent != null)
            //{
            //    var entity = charComponent.Entity;
            //    MyAPIGateway.Utilities.ShowMessage("Entity", "Good");
            //}
            //var turret = cubeBlock as Sandbox.Game.Weapons.MyLargeTurretBase;
            //var turret = cubeBlock as IMyControllableEntity;

            return null;
        }

        public static bool IsHost(this IMyPlayer player)
        {
            return MyAPIGateway.Multiplayer.IsServerPlayer(player.Client);
        }

        public static IMyPlayer FindPlayerBySteamId(this IMyPlayerCollection collection, ulong steamId)
        {
            var listplayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listplayers, p => p.SteamUserId == steamId);
            return listplayers.FirstOrDefault();
        }

        public static string GetDisplayName(this MyObjectBuilder_Base objectBuilder)
        {
            return MyDefinitionManager.Static.GetDefinition(objectBuilder.GetId()).GetDisplayName();
        }

        public static string GetDisplayName(this MyDefinitionBase definition)
        {
            return definition.DisplayNameEnum.HasValue ? MyTexts.GetString(definition.DisplayNameEnum.Value) : (string.IsNullOrEmpty(definition.DisplayNameString) ? definition.Id.SubtypeName : definition.DisplayNameString);
        }

        public static SerializableVector3 ToSerializableVector3(this Vector3D v)
        {
            return new SerializableVector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        /// <summary>
        /// Creates the objectbuilder in game, and syncs it to the server and all clients.
        /// </summary>
        /// <param name="entity"></param>
        public static void CreateAndSyncEntity(this MyObjectBuilder_EntityBase entity)
        {
            CreateAndSyncEntities(new List<MyObjectBuilder_EntityBase> { entity });
        }

        /// <summary>
        /// Creates the objectbuilders in game, and syncs it to the server and all clients.
        /// </summary>
        /// <param name="entities"></param>
        public static void CreateAndSyncEntities(this List<MyObjectBuilder_EntityBase> entities)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(entities);
            entities.ForEach(item => MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item));
            MyAPIGateway.Multiplayer.SendEntitiesCreated(entities);
        }

        public static MyPhysicalItemDefinition GetDefinition(this MyDefinitionManager definitionManager, string typeId, string subtypeName)
        {
            MyPhysicalItemDefinition definition = null;
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(typeId, out result))
            {
                var id = new MyDefinitionId(result, subtypeName);
                MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition);
            }

            return definition;
        }

        public static List<MyGasProperties> GetGasDefinitions(this MyDefinitionManager definitionManager)
        {
            return definitionManager.GetAllDefinitions().Where(e => e.Id.TypeId == typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties)).Cast<MyGasProperties>().ToList();
        }

        public static Sandbox.ModAPI.IMyInventory GetPlayerInventory(this IMyPlayer player)
        {
            var character = player.GetCharacter();
            if (character == null)
                return null;
            return character.GetPlayerInventory();
        }

        public static Sandbox.ModAPI.IMyInventory GetPlayerInventory(this IMyCharacter character)
        {
            if (character == null)
                return null;
            return ((MyEntity)character).GetInventory();
        }

        #region attached grids

        /// <summary>
        /// Find all grids attached to the specified grid, either by piston, rotor, connector or landing gear.
        /// This will iterate through all attached grids, until all are found.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type">Specifies if all attached grids will be found or only grids that are attached either by piston or rotor.</param>
        /// <returns>A list of all attached grids, including the original.</returns>
        public static List<IMyCubeGrid> GetAttachedGrids(this IMyEntity entity, AttachedGrids type = AttachedGrids.All)
        {
            var cubeGrid = entity as IMyCubeGrid;

            if (cubeGrid == null)
                return new List<IMyCubeGrid>();

            var results = new List<IMyCubeGrid> { cubeGrid };
            GetAttachedGrids(cubeGrid, ref results, type);
            return results;
        }

        private static void GetAttachedGrids(IMyCubeGrid cubeGrid, ref List<IMyCubeGrid> results, AttachedGrids type)
        {
            if (cubeGrid == null)
                return;

            var blocks = new List<IMySlimBlock>();
            cubeGrid.GetBlocks(blocks, b => b != null && b.FatBlock != null && !b.FatBlock.BlockDefinition.TypeId.IsNull);

            foreach (var block in blocks)
            {
                //MyAPIGateway.Utilities.ShowMessage("Block", string.Format("{0}", block.FatBlock.BlockDefinition.TypeId));

                if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorAdvancedStator) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorStator) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorSuspension) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorBase))
                {
                    // The MotorStator which inherits from MotorBase.
                    var motorBase = block.GetObjectBuilder() as MyObjectBuilder_MotorBase;
                    if (motorBase == null || !motorBase.RotorEntityId.HasValue || motorBase.RotorEntityId.Value == 0 || !MyAPIGateway.Entities.EntityExists(motorBase.RotorEntityId.Value))
                        continue;
                    var entityParent = MyAPIGateway.Entities.GetEntityById(motorBase.RotorEntityId.Value).Parent as IMyCubeGrid;
                    if (entityParent == null)
                        continue;
                    if (!results.Any(e => e.EntityId == entityParent.EntityId))
                    {
                        results.Add(entityParent);
                        GetAttachedGrids(entityParent, ref results, type);
                    }
                }
                else if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorAdvancedRotor) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorRotor) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_RealWheel) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Wheel))
                {
                    // The Rotor Part.
                    var motorCube = Support.FindRotorBase(block.FatBlock.EntityId);
                    if (motorCube == null)
                        continue;
                    var entityParent = (IMyCubeGrid)motorCube.Parent;
                    if (!results.Any(e => e.EntityId == entityParent.EntityId))
                    {
                        results.Add(entityParent);
                        GetAttachedGrids(entityParent, ref results, type);
                    }
                }
                else if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_PistonTop))
                {
                    // The Piston Top.
                    var pistonCube = Support.FindPistonBase(block.FatBlock.EntityId);
                    if (pistonCube == null)
                        continue;
                    var entityParent = (IMyCubeGrid)pistonCube.Parent;
                    if (!results.Any(e => e.EntityId == entityParent.EntityId))
                    {
                        results.Add(entityParent);
                        GetAttachedGrids(entityParent, ref results, type);
                    }
                }
                else if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_ExtendedPistonBase) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_PistonBase))
                {
                    var pistonBase = block.GetObjectBuilder() as MyObjectBuilder_PistonBase;
                    if (pistonBase == null || pistonBase.TopBlockId == 0 || !MyAPIGateway.Entities.EntityExists(pistonBase.TopBlockId))
                        continue;
                    var entityParent = MyAPIGateway.Entities.GetEntityById(pistonBase.TopBlockId).Parent as IMyCubeGrid;
                    if (entityParent == null)
                        continue;
                    if (!results.Any(e => e.EntityId == entityParent.EntityId))
                    {
                        results.Add(entityParent);
                        GetAttachedGrids(entityParent, ref results, type);
                    }
                }
                else if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_ShipConnector) && type == AttachedGrids.All)
                {
                    // There isn't a non-Ingame interface for IMyShipConnector at this time.
                    var connector = (Sandbox.ModAPI.Ingame.IMyShipConnector)block.FatBlock;

                    if (connector.IsConnected == false || connector.IsLocked == false || connector.OtherConnector == null)
                        continue;

                    var otherGrid = (IMyCubeGrid)connector.OtherConnector.CubeGrid;

                    if (!results.Any(e => e.EntityId == otherGrid.EntityId))
                    {
                        results.Add(otherGrid);
                        GetAttachedGrids(otherGrid, ref results, type);
                    }
                }
                else if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_LandingGear) && type == AttachedGrids.All)
                {
                    var landingGear = (IMyLandingGear)block.FatBlock;
                    if (landingGear.IsLocked == false)
                        continue;

                    var entity = landingGear.GetAttachedEntity();
                    if (entity == null || !(entity is IMyCubeGrid))
                        continue;

                    var otherGrid = (IMyCubeGrid)entity;
                    if (!results.Any(e => e.EntityId == otherGrid.EntityId))
                    {
                        results.Add(otherGrid);
                        GetAttachedGrids(otherGrid, ref results, type);
                    }
                }
            }

            // Loop through all other grids, find their Landing gear, and figure out if they are attached to <cubeGrid>.
            var allShips = new HashSet<IMyEntity>();
            var checkList = results; // cannot use ref paramter in Lambada expression!?!.
            MyAPIGateway.Entities.GetEntities(allShips, e => e is IMyCubeGrid && !checkList.Contains(e));

            if (type == AttachedGrids.All)
            {
                foreach (IMyCubeGrid ship in allShips)
                {
                    blocks = new List<IMySlimBlock>();
                    ship.GetBlocks(blocks,
                        b =>
                            b != null && b.FatBlock != null && !b.FatBlock.BlockDefinition.TypeId.IsNull &&
                            b.FatBlock is IMyLandingGear);

                    foreach (var block in blocks)
                    {
                        var landingGear = (IMyLandingGear)block.FatBlock;
                        if (landingGear.IsLocked == false)
                            continue;

                        var entity = landingGear.GetAttachedEntity();

                        if (entity == null || entity.EntityId != cubeGrid.EntityId)
                            continue;

                        if (!results.Any(e => e.EntityId == ship.EntityId))
                        {
                            results.Add(ship);
                            GetAttachedGrids(ship, ref results, type);
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Time elapsed since the start of the game.
        /// This is saved in checkpoint, instead of GameDateTime.
        /// </summary>
        /// <remarks>Copied from Sandbox.Game.World.MySession</remarks>
        public static TimeSpan ElapsedGameTime(this IMySession session)
        {
            return MyAPIGateway.Session.GameDateTime - new DateTime(2081, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        #region framework

        /// <summary>
        /// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary&gt;TKey,TValue&lt;.
        /// If the provide key already exists, then the existing key is updated with the newly supplied value.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">key is null</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary&gt;TKey,TValue&lt; is read-only.</exception>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        #endregion
    }
}