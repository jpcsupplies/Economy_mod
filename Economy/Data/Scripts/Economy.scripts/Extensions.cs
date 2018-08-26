namespace Economy.scripts
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public static class Extensions
    {
        /// <summary>
        /// This is an overly complex check to avoid issues that Torch develolpers have caused by utilizing the OnlineMode for other purposes.
        /// </summary>
        /// <returns></returns>
        public static bool IsSinglePlayerOffline(this IMySession session)
        {
            return session.OnlineMode == MyOnlineModeEnum.OFFLINE
                   && session.IsServer // it calls MyAPIGateway.Multiplayer.IsServer.
                   && !MyAPIGateway.Utilities.IsDedicated;
        }

        /// <summary>
        /// Determines if the player is an Administrator of the active game session.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>True if is specified player is an Administrator in the active game.</returns>
        public static bool IsAdmin(this IMyPlayer player)
        {
            // Offline mode. You are the only player.
            if (MyAPIGateway.Session.IsSinglePlayerOffline())
            {
                return true;
            }

            // Hosted game, and the player is hosting the server.
            if (player.IsHost())
            {
                return true;
            }

            return player.PromoteLevel == MyPromoteLevel.Owner ||  // 5 star
                player.PromoteLevel == MyPromoteLevel.Admin ||     // 4 star
                player.PromoteLevel == MyPromoteLevel.SpaceMaster; // 3 star
            // Otherwise Treat everyone as Normal Player.
        }

        public static uint UserSecurityLevel(this IMyPlayer player)
        {
            switch (player.PromoteLevel)
            {
                // 5 star
                case MyPromoteLevel.Owner: return ChatCommandSecurity.Owner;

                // 4 star
                case MyPromoteLevel.Admin: return ChatCommandSecurity.Admin;

                // 3 star
                case MyPromoteLevel.SpaceMaster: return ChatCommandSecurity.SpaceMaster;

                case MyPromoteLevel.Moderator: return ChatCommandSecurity.Moderator;

                case MyPromoteLevel.Scripter: return ChatCommandSecurity.Scripter;

                // normal player.
                case MyPromoteLevel.None: return ChatCommandSecurity.User;

                default: return ChatCommandSecurity.User;
            }
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
            MyAPIGateway.Players.GetPlayers(listPlayers, p => p.IdentityId == identity.IdentityId);
            return listPlayers.FirstOrDefault();
        }

        public static IMyIdentity Identity(this IMyPlayer player)
        {
            var listIdentites = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(listIdentites, p => p.IdentityId == player.IdentityId);
            return listIdentites.FirstOrDefault();
        }

        public static IMyPlayer GetPlayer(this IMyPlayerCollection collection, ulong steamId)
        {
            var players = new List<IMyPlayer>();
            collection.GetPlayers(players, p => p.SteamUserId == steamId);
            return players.FirstOrDefault();
        }

        public static bool TryGetPlayer(this IMyPlayerCollection collection, ulong steamId, out IMyPlayer player)
        {
            var players = new List<IMyPlayer>();
            collection.GetPlayers(players, p => p.SteamUserId == steamId);
            player = players.FirstOrDefault();
            return player != null;
        }

        public static bool TryGetPlayer(this IMyPlayerCollection collection, long playerId, out IMyPlayer player)
        {
            var players = new List<IMyPlayer>();
            collection.GetPlayers(players, p => p.IdentityId == playerId);
            player = players.FirstOrDefault();
            return player != null;
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

        public static MyDefinitionBase GetDefinition(this MyDefinitionManager definitionManager, string typeId, string subtypeName)
        {
            MyObjectBuilderType result;
            if (!MyObjectBuilderType.TryParse(typeId, out result))
                return null;

            var id = new MyDefinitionId(result, subtypeName);
            try
            {
                return MyDefinitionManager.Static.GetDefinition(id);
            }
            catch
            {
                // If a item as been removed, like a mod,
                // or the mod has broken and failed to load, this will return null.
                return null;
            }
        }

        public static List<MyGasProperties> GetGasDefinitions(this MyDefinitionManager definitionManager)
        {
            return definitionManager.GetAllDefinitions().Where(e => e.Id.TypeId == typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties)).Cast<MyGasProperties>().ToList();
        }

        public static IMyInventory GetPlayerInventory(this IMyPlayer player)
        {
            var character = player.Character;
            return character?.GetPlayerInventory();
        }

        public static IMyInventory GetPlayerInventory(this IMyCharacter character)
        {
            return ((MyEntity)character)?.GetInventory();
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

            switch (type)
            {
                case AttachedGrids.Static:
                    // Should include connections via: Rotors, Pistons, Suspension.
                    return MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Mechanical);
                case AttachedGrids.All:
                default:
                    // Should include connections via: Landing Gear, Connectors, Rotors, Pistons, Suspension.
                    return MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Physical);
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

        public static bool TryWordParseBool(this string value, out bool result)
        {
            bool boolTest;
            if (bool.TryParse(value, out boolTest))
            {
                result = boolTest;
                return true;
            }

            if (value.Equals("on", StringComparison.InvariantCultureIgnoreCase) || value.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || value.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                result = true;
                return true;
            }

            if (value.Equals("off", StringComparison.InvariantCultureIgnoreCase) || value.Equals("no", StringComparison.InvariantCultureIgnoreCase) || value.Equals("0", StringComparison.InvariantCultureIgnoreCase))
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }
    }
}