namespace Economy.scripts.Messages
{
    using EconConfig;
    using EconStructures;
    using ProtoBuf;

    /// <summary>
    /// Used to send updated hud configuration back to the server for storage.
    /// </summary>
    public class MessageHudUpdate : MessageBase
    {
        [ProtoMember(1)]
        public ClientHudSettingsStruct HudSettings;

        public static void SendMessage(ClientHudSettingsStruct hudSettings)
        {
            ConnectionHelper.SendMessageToServer(new MessageHudUpdate
            {
                HudSettings = hudSettings
            });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("Action /hud started by Steam Id '{0}'.", SenderSteamId);

            ClientAccountStruct playerAccount = AccountManager.FindAccount(SenderSteamId);
            if (playerAccount == null)
                return;

            playerAccount.ClientHudSettings = HudSettings;
        }
    }
}
