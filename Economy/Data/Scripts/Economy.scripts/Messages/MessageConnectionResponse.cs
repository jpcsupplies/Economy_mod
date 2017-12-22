namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.Game;

    /// <summary>
    /// The contains the inital handshake response, and shouldn't contain any other custom code as it will affect 
    /// the structure of the serialized class if other classes are properties of this and are modified.
    /// This is so that version information is always passed intact.
    /// </summary>
    [ProtoContract]
    public class MessageConnectionResponse : MessageBase
    {
        [ProtoMember(201)]
        public bool IsOldCommunicationVersion;

        [ProtoMember(202)]
        public bool IsNewCommunicationVersion;

        [ProtoMember(203)]
        public uint UserSecurity { get; set; }

        // Client config needs to be handled by the mod, not the ModCore.

        public static void SendMessage(ulong steamdId, int clientModCommunicationVersion, int serverModCommunicationVersion, uint userSecurity)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Sending Connection Response.");
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageConnectionResponse
            {
                IsOldCommunicationVersion = clientModCommunicationVersion < serverModCommunicationVersion,
                IsNewCommunicationVersion = clientModCommunicationVersion > serverModCommunicationVersion,
                UserSecurity = userSecurity
            });
        }

        public override void ProcessClient()
        {
            EconomyScript.Instance.ClientLogger.WriteInfo("Processing MessageConnectionResponse");

            // stop further requests
            if (EconomyScript.Instance.DelayedConnectionRequestTimer != null 
                && EconomyScript.Instance.ClientConfig != null)
            {
                EconomyScript.Instance.DelayedConnectionRequestTimer.Stop();
                EconomyScript.Instance.DelayedConnectionRequestTimer.Close();
            }

            if (EconomyScript.Instance.IsConnected)
                return;

            if (IsOldCommunicationVersion)
            {
                MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The version of Economy running on your Server is wrong.\r\nPlease update and restart your server.");
                MyAPIGateway.Utilities.ShowNotification("Warning: The version of Economy running on your Server is wrong.", 5000, MyFontEnum.Blue);
                // TODO: display OldCommunicationVersion.

                // The server has a newer version!
                EconomyScript.Instance.ClientLogger.WriteInfo("Warning: The Economy mod is currently unavailable as it is out of date. Please check to make sure you have downloaded the latest version of the mod.");
            }
            if (IsNewCommunicationVersion)
            {
                if (MyAPIGateway.Session.Player.IsAdmin())
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The version of Economy running on your Server is out of date.\r\nPlease update and restart your server.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The version of Economy running on your Server is out of date.", 5000, MyFontEnum.Blue);
                    // TODO: display NewCommunicationVersion.
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The Economy mod is currently unavailable as it is out of date.\r\nPlease contact your server Administrator.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The Economy mod is currently unavailable as it is out of date.", 5000, MyFontEnum.Blue);
                    // TODO: display NewCommunicationVersion.
                }

                EconomyScript.Instance.ClientLogger.WriteInfo("Warning: The Economy mod is currently unavailable as it is out of date on the server. Please contact your server Administrator to make sure they have the latest version of the mod.");
            }
            EconomyScript.Instance.IsConnected = true;
        }

        public override void ProcessServer()
        {
            // never processed on server
        }
    }
}
