using System.Collections.Generic;
using System.Linq;

using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;

namespace Economy
{
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

        public static IMyPlayer Player(this IMyIdentity identity)
        {
            var listplayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listplayers, p => p.PlayerID == identity.PlayerId);
            return listplayers.FirstOrDefault();
        }

        public static bool IsHost(this IMyPlayer player)
        {
            return MyAPIGateway.Multiplayer.IsServerPlayer(player.Client);
        }
    }
}