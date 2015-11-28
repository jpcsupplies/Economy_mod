namespace Economy.scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public static class Support
    {
        #region fields

        private static MyPhysicalItemDefinition[] _physicalItems;
        private static Dictionary<string, MyPhysicalItemDefinition> _physicalItemNames;
        private static Dictionary<string, MyPhysicalItemDefinition> _oreList;
        private static Dictionary<string, MyPhysicalItemDefinition> _ingotList;
        private static Dictionary<MyDefinitionId, MyObjectBuilder_Base> _producedTypeList;
        private static bool _hasBuiltComponentList;

        #endregion

        /// <summary>
        /// Builds a list of the in game components and their localized names for searching so 
        /// that subsequent calls to FindPhysicalParts(...) don't have to build a lists every time.
        /// </summary>
        private static void BuildComponentLists()
        {
            if (_hasBuiltComponentList)
                return;

            EconomyScript.Instance.ClientLogger.Write("BuildComponentLists");

            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            _producedTypeList = new Dictionary<MyDefinitionId, MyObjectBuilder_Base>();
            _physicalItems = physicalItems.Where(item => item.Public).ToArray();  // Limit to public items.  This will remove the CubePlacer. :)

            // TODO: This list is generated ONCE, and not generated again during the session, so if an Item has its Blacklist state changed mid-game, it may show up or not show up.
            // Cannot call MarketManager here, because it is only accessible to the server, not Client.
            // Filter out the server Blacklisted items from the options.
            //_physicalItems = _physicalItems.Where(e => !MarketManager.IsItemBlacklistedOnServer(e.Id.TypeId.ToString(), e.Id.SubtypeName)).ToArray();

            // Make sure all Public Physical item names are unique, so they can be properly searched for.
            _physicalItemNames = new Dictionary<string, MyPhysicalItemDefinition>();
            foreach (var item in _physicalItems)
            {
                var baseName = item.GetDisplayName();
                var uniqueName = baseName;
                var index = 1;
                while (_physicalItemNames.ContainsKey(uniqueName))
                {
                    index++;
                    uniqueName = string.Format("{0}{1}", baseName, index);
                }
                _physicalItemNames.Add(uniqueName, item);

                var content = MyObjectBuilderSerializer.CreateNewObject(item.Id);
                _producedTypeList.Add(item.Id, content);
            }

            _oreList = _physicalItemNames.Where(p => p.Value.Id.TypeId == typeof(MyObjectBuilder_Ore)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _ingotList = _physicalItemNames.Where(p => p.Value.Id.TypeId == typeof(MyObjectBuilder_Ingot)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _hasBuiltComponentList = true;
        }

        /// <summary>
        /// Find the physical object of the specified name or partial name.
        /// </summary>
        /// <param name="itemName">The name of the physical object to find.</param>
        /// <param name="objectBuilder">The object builder of the physical object, ready for use.</param>
        /// <param name="options">Returns a list of potential matches if there was more than one of the same or partial name.</param>
        /// <returns>Returns true if a single exact match was found.</returns>

        public static bool FindPhysicalParts(string itemName, out MyObjectBuilder_Base objectBuilder, out Dictionary<string, MyPhysicalItemDefinition> options)
        {
            BuildComponentLists();
            itemName = itemName.Trim();
            var itemNames = itemName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // prefix the search term with 'ore' to find this ore name.
            if (itemNames.Length > 1 && itemNames[0].Equals("ore", StringComparison.InvariantCultureIgnoreCase))
            {
                var findName = itemName.Substring(4).Trim();

                var exactMatchOres = _oreList.Where(ore => ore.Value.Id.SubtypeName.Equals(findName, StringComparison.InvariantCultureIgnoreCase)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (exactMatchOres.Count == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ore() { SubtypeName = exactMatchOres.First().Value.Id.SubtypeName };
                    options = new Dictionary<string, MyPhysicalItemDefinition>();
                    return true;
                }
                if (exactMatchOres.Count > 1)
                {
                    objectBuilder = null;
                    options = exactMatchOres;
                    return false;
                }

                var partialMatchOres = _oreList.Where(ore => ore.Value.Id.SubtypeName.IndexOf(findName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (partialMatchOres.Count == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ore() { SubtypeName = partialMatchOres.First().Value.Id.SubtypeName };
                    options = new Dictionary<string, MyPhysicalItemDefinition>();
                    return true;
                }
                if (partialMatchOres.Count > 1)
                {
                    objectBuilder = null;
                    options = partialMatchOres;
                    return false;
                }

                objectBuilder = null;
                options = new Dictionary<string, MyPhysicalItemDefinition>();
                return false;
            }

            // prefix the search term with 'ingot' to find this ingot name.
            if (itemNames.Length > 1 && itemNames[0].Equals("ingot", StringComparison.InvariantCultureIgnoreCase))
            {
                var findName = itemName.Substring(6).Trim();

                var exactMatchIngots = _ingotList.Where(ingot => ingot.Value.Id.SubtypeName.Equals(findName, StringComparison.InvariantCultureIgnoreCase)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (exactMatchIngots.Count == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ingot() { SubtypeName = exactMatchIngots.First().Value.Id.SubtypeName };
                    options = new Dictionary<string, MyPhysicalItemDefinition>();
                    return true;
                }
                if (exactMatchIngots.Count > 1)
                {
                    objectBuilder = null;
                    options = exactMatchIngots;
                    return false;
                }

                var partialMatchIngots = _ingotList.Where(ingot => ingot.Value.Id.SubtypeName.IndexOf(findName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (partialMatchIngots.Count == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ingot() { SubtypeName = partialMatchIngots.First().Value.Id.SubtypeName };
                    options = new Dictionary<string, MyPhysicalItemDefinition>();
                    return true;
                }
                if (partialMatchIngots.Count > 1)
                {
                    objectBuilder = null;
                    options = partialMatchIngots;
                    return false;
                }

                objectBuilder = null;
                options = new Dictionary<string, MyPhysicalItemDefinition>();
                return false;
            }

            // full name match.
            var res = _physicalItemNames.FirstOrDefault(s => s.Key != null && s.Key.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

            // need a good method for finding partial name matches.
            if (res.Key == null)
            {
                var matches = _physicalItemNames.Where(s => s.Key != null && s.Key.StartsWith(itemName, StringComparison.InvariantCultureIgnoreCase)).Distinct().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (matches.Count == 1)
                {
                    res = matches.FirstOrDefault();
                }
                else
                {
                    matches = _physicalItemNames.Where(s => s.Key != null && s.Key.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) >= 0).Distinct().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    if (matches.Count == 1)
                    {
                        res = matches.FirstOrDefault();
                    }
                    else if (matches.Count > 1)
                    {
                        objectBuilder = null;
                        options = matches;
                        return false;
                    }
                }
            }

            if (res.Key != null)
            {
                if (res.Value != null)
                {
                    objectBuilder = MyObjectBuilderSerializer.CreateNewObject(res.Value.Id.TypeId, res.Value.Id.SubtypeName);
                    options = new Dictionary<string, MyPhysicalItemDefinition>();
                    return true;
                }
            }

            objectBuilder = null;
            options = new Dictionary<string, MyPhysicalItemDefinition>();
            return false;
        }

        public static MyObjectBuilder_Base ProducedType(MyDefinitionId definitionId)
        {
            BuildComponentLists();

            // Cannot check the Type of the item, without having to use MyObjectBuilderSerializer.CreateNewObject()
            // This is because MyObjectBuilderSerializer.ProducedType() is not available as a public method, and we cannot do a Refelection for the Base Type.
            // So intead, we store a basic type of each object in memory created by MyObjectBuilderSerializer.CreateNewObject(), to allow us to do Type checks.
            MyObjectBuilder_Base value;
            if (_producedTypeList.TryGetValue(definitionId, out value))
                return value;
            return null;
        }

        /// <summary>
        /// check the seller is in range of a valid trade region or player
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static bool RangeCheck(IMyPlayer player1, IMyPlayer player2)
        {
            //if (limited range setting is false or My location works out To be within 2500 of a valid trade area)
            //lookup the location of target name and compare with location of seller
            //there has to be an easy way to do this, the GPSs use it..
            // TODO: implement. https://github.com/jpcsupplies/Economy_mod/issues/49

            var character1 = player1.GetCharacter();
            var character2 = player2.GetCharacter();

            if (character1 == null || character2 == null)
                // one player or the other doesn't exist in game.
                return false;

            Vector3D position1 = ((IMyEntity)character1).GetPosition();
            Vector3D position2 = ((IMyEntity)character2).GetPosition();

            // TODO: check broadcast antenna. There doesn't appear to be any accessible API to help at this stage.
            // MyAntennaSystem.GetPlayerRelayedBroadcasters(MyCharacter playerCharacter, MyEntity interactedEntityRepresentative, HashSet<MyDataBroadcaster> output)
            // MyAntennaSystem.CheckConnection(MyIdentity sender, MyIdentity receiver)
            // MySyncCharacter.CheckPlayerConnection(ulong senderSteamId, ulong receiverSteamId)

            var distance = Vector3D.Distance(position1, position2);

            // so did it come within our default range
            return distance <= EconomyConsts.DefaultTradeRange;
        }

        #region Inventory

        public static bool InventoryAdd(IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            var gasContainer = content as MyObjectBuilder_GasContainerObject;
            if (gasContainer != null)
                gasContainer.GasLevel = 1f;

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, Content = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, (MyObjectBuilder_PhysicalObject)inventoryItem.Content, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }

        public static void InventoryDrop(IMyEntity entity, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            Vector3D position;

            if (entity is IMyCharacter)
                position = entity.WorldMatrix.Translation + entity.WorldMatrix.Forward * 1.5f + entity.WorldMatrix.Up * 1.5f; // Spawn item 1.5m in front of player.
            else
                position = entity.WorldMatrix.Translation + entity.WorldMatrix.Forward * 1.5f; // Spawn item 1.5m in front of player in cockpit.

            MyObjectBuilder_FloatingObject floatingBuilder = new MyObjectBuilder_FloatingObject();
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            var gasContainer = content as MyObjectBuilder_GasContainerObject;
            if (gasContainer != null)
                gasContainer.GasLevel = 1f;

            floatingBuilder.Item = new MyObjectBuilder_InventoryItem() { Amount = amount, Content = content };
            floatingBuilder.PersistentFlags = MyPersistentEntityFlags2.InScene; // Very important

            floatingBuilder.PositionAndOrientation = new MyPositionAndOrientation()
            {
                Position = position,
                Forward = entity.WorldMatrix.Forward.ToSerializableVector3(),
                Up = entity.WorldMatrix.Up.ToSerializableVector3(),
            };

            floatingBuilder.CreateAndSyncEntity();
        }

        #endregion

        #region Find Cube in Grid

        public static IMyCubeBlock FindRotorBase(long entityId, IMyCubeGrid parent = null)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCubeGrid);

            foreach (var entity in entities)
            {
                var cubeGrid = (IMyCubeGrid)entity;

                if (cubeGrid == null)
                    continue;

                var blocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blocks, block => block != null && block.FatBlock != null &&
                    (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorAdvancedStator) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorStator) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorSuspension) ||
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_MotorBase)));

                foreach (var block in blocks)
                {
                    var motorBase = block.GetObjectBuilder() as MyObjectBuilder_MotorBase;

                    if (motorBase == null || !motorBase.RotorEntityId.HasValue || motorBase.RotorEntityId.Value == 0 || !MyAPIGateway.Entities.EntityExists(motorBase.RotorEntityId.Value))
                        continue;

                    if (motorBase.RotorEntityId == entityId)
                        return block.FatBlock;
                }
            }

            return null;
        }

        public static IMyCubeBlock FindPistonBase(long entityId, IMyCubeGrid parent = null)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCubeGrid);

            foreach (var entity in entities)
            {
                var cubeGrid = (IMyCubeGrid)entity;

                if (cubeGrid == null)
                    continue;

                var blocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blocks, block => block != null && block.FatBlock != null &&
                    block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_PistonBase));

                foreach (var block in blocks)
                {
                    var pistonBase = block.GetObjectBuilder() as MyObjectBuilder_PistonBase;

                    if (pistonBase == null || pistonBase.TopBlockId == 0 || !MyAPIGateway.Entities.EntityExists(pistonBase.TopBlockId))
                        continue;

                    if (pistonBase.TopBlockId == entityId)
                        return block.FatBlock;
                }
            }

            return null;
        }

        public static IMyEntity FindLookAtEntity(Sandbox.ModAPI.Interfaces.IMyControllableEntity controlledEntity, bool findShips, bool findCubes, bool findPlayers, bool findAsteroids, bool findPlanets, bool findReplicable, bool ignoreProjection)
        {
            IMyEntity entity;
            double distance;
            Vector3D hitPoint;
            FindLookAtEntity(controlledEntity, true, ignoreProjection, out entity, out distance, out hitPoint, findShips, findCubes, findPlayers, findAsteroids, findPlanets, findReplicable);
            return entity;
        }

        public static void FindLookAtEntity(Sandbox.ModAPI.Interfaces.IMyControllableEntity controlledEntity, bool ignoreOccupiedGrid, bool ignoreProjection, out IMyEntity lookEntity, out double lookDistance, out Vector3D hitPoint, bool findShips, bool findCubes, bool findPlayers, bool findAsteroids, bool findPlanets, bool findReplicable)
        {
            const float range = 5000000;
            Matrix worldMatrix;
            Vector3D startPosition;
            Vector3D endPosition;
            IMyCubeGrid occupiedGrid = null;

            if (controlledEntity.Entity.Parent == null)
            {
                worldMatrix = controlledEntity.GetHeadMatrix(true, true, false); // dead center of player cross hairs, or the direction the player is looking with ALT.
                startPosition = worldMatrix.Translation + worldMatrix.Forward * 0.5f;
                endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 0.5f);
            }
            else
            {
                occupiedGrid = controlledEntity.Entity.GetTopMostParent() as IMyCubeGrid;
                worldMatrix = controlledEntity.Entity.WorldMatrix;
                // TODO: need to adjust for position of cockpit within ship.
                startPosition = worldMatrix.Translation + worldMatrix.Forward * 1.5f;
                endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 1.5f);
            }

            var entites = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entites, e => e != null);

            var list = new Dictionary<IMyEntity, double>();
            var ray = new RayD(startPosition, worldMatrix.Forward);

            foreach (var entity in entites)
            {
                if (findShips || findCubes)
                {
                    var cubeGrid = entity as Sandbox.ModAPI.IMyCubeGrid;

                    if (cubeGrid != null)
                    {
                        if (ignoreOccupiedGrid && occupiedGrid != null && occupiedGrid.EntityId == cubeGrid.EntityId)
                            continue;

                        // Will ignore Projected grids, new grid/cube placement, and grids in middle of copy/paste.
                        if (ignoreProjection && cubeGrid.Physics == null)
                            continue;

                        // check if the ray comes anywhere near the Grid before continuing.    
                        if (ray.Intersects(entity.WorldAABB).HasValue)
                        {
                            var hit = cubeGrid.RayCastBlocks(startPosition, endPosition);
                            if (hit.HasValue)
                            {
                                var distance = (startPosition - cubeGrid.GridIntegerToWorld(hit.Value)).Length();
                                var block = cubeGrid.GetCubeBlock(hit.Value);

                                if (block.FatBlock != null && findCubes)
                                    list.Add(block.FatBlock, distance);
                                else if (findShips)
                                    list.Add(entity, distance);
                            }
                        }
                    }
                }

                if (findPlayers)
                {
                    var controller = entity as Sandbox.ModAPI.Interfaces.IMyControllableEntity;
                    if (controlledEntity.Entity.EntityId != entity.EntityId && controller != null && ray.Intersects(entity.WorldAABB).HasValue)
                    {
                        var distance = (startPosition - entity.GetPosition()).Length();
                        list.Add(entity, distance);
                    }
                }

                if (findReplicable)
                {
                    var replicable = entity as Sandbox.Game.Entities.MyReplicableEntity;
                    if (replicable != null && ray.Intersects(entity.WorldAABB).HasValue)
                    {
                        var distance = (startPosition - entity.GetPosition()).Length();
                        list.Add(entity, distance);
                    }
                }

                if (findAsteroids)
                {
                    var voxelMap = entity as IMyVoxelMap;
                    if (voxelMap != null)
                    {
                        var aabb = new BoundingBoxD(voxelMap.PositionLeftBottomCorner, voxelMap.PositionLeftBottomCorner + voxelMap.Storage.Size);
                        var hit = ray.Intersects(aabb);
                        if (hit.HasValue)
                        {
                            var center = voxelMap.PositionLeftBottomCorner + (voxelMap.Storage.Size / 2);
                            var distance = (startPosition - center).Length();  // use distance to center of asteroid.
                            list.Add(entity, distance);
                        }
                    }
                }

                if (findPlanets)
                {
                    // Looks to be working against Git and public release.
                    var planet = entity as Sandbox.Game.Entities.MyPlanet;
                    if (planet != null)
                    {
                        var aabb = new BoundingBoxD(planet.PositionLeftBottomCorner, planet.PositionLeftBottomCorner + planet.Size);
                        var hit = ray.Intersects(aabb);
                        if (hit.HasValue)
                        {
                            var center = planet.WorldMatrix.Translation;
                            var distance = (startPosition - center).Length(); // use distance to center of planet.
                            list.Add(entity, distance);
                        }
                    }
                }
            }

            if (list.Count == 0)
            {
                lookEntity = null;
                lookDistance = 0;
                hitPoint = Vector3D.Zero;
                return;
            }

            // find the closest Entity.
            var item = list.OrderBy(f => f.Value).First();
            lookEntity = item.Key;
            lookDistance = item.Value;
            hitPoint = startPosition + (Vector3D.Normalize(ray.Direction) * lookDistance);
        }

        #endregion

        ///// <summary>
        ///// Must be called by the Client for correct localization.
        ///// </summary>
        ///// <param name="typeId"></param>
        ///// <param name="subtypeName"></param>
        ///// <returns></returns>
        //public static string GetDisplayName(string typeId, string subtypeName)
        //{
        //    MyObjectBuilderType result;
        //    if (MyObjectBuilderType.TryParse(typeId, out result))
        //    {
        //        var id = new MyDefinitionId(result, subtypeName);
        //        MyPhysicalItemDefinition definition;
        //        if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition))
        //            return definition.GetDisplayName();
        //    }
        //    return "";
        //}
    }
}
