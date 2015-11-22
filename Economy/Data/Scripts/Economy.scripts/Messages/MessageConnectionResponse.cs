namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using Sandbox.ModAPI;

    public class MessageConnectionResponse : MessageBase
    {
        [ProtoMember(1)]
        public bool OldCommunicationVersion;

        [ProtoMember(2)]
        public bool NewCommunicationVersion;

        public override void ProcessClient()
        {
            // stop further requests
            if (EconomyScript.Instance.DelayedConnectionRequestTimer != null)
            {
                EconomyScript.Instance.DelayedConnectionRequestTimer.Stop();
                EconomyScript.Instance.DelayedConnectionRequestTimer.Close();
            }

            if (OldCommunicationVersion)
            {
                if (MyAPIGateway.Session.Player.IsAdmin())
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The version of Economy running on your Server is out of date.\r\nPlease update and restart your server.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The version of Economy running on your Server is out of date.", 5000, Sandbox.Common.MyFontEnum.Blue);
                    // TODO: display OldCommunicationVersion.
                }
                else
                {
                    // TODO: display OldCommunicationVersion.
                }
            }
            if (NewCommunicationVersion)
            {
                // TODO: display NewCommunicationVersion.
                // The server has a newer version!
            }
        }

        public override void ProcessServer()
        {
            // never processed on server
        }

        public static void SendMessage(ulong steamdId, bool oldCommunicationVersion, bool newCommunicationVersion)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageConnectionResponse { OldCommunicationVersion = oldCommunicationVersion, NewCommunicationVersion = newCommunicationVersion });
        }
    }
}
