namespace Economy.scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
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
            return MyDefinitionManager.Static.GetPhysicalItemDefinition(objectBuilder).GetDisplayName();
        }

        public static string GetDisplayName(this MyPhysicalItemDefinition defintion)
        {
            return defintion.DisplayNameEnum.HasValue ? MyTexts.GetString(defintion.DisplayNameEnum.Value) : defintion.DisplayNameString;
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

        public static IMyInventory GetPlayerInventory(this IMyPlayer player)
        {
            var character = player.GetCharacter();
            if (character == null)
                return null;
            return character.GetPlayerInventory();
        }

        public static IMyInventory GetPlayerInventory(this IMyCharacter character)
        {
            if (character == null)
                return null;
            return ((Sandbox.Game.Entities.MyEntity)character).GetInventory();
        }
    }
}