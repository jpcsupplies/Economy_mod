namespace Economy.scripts.Messages
{
    using EconStructures;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.Game;

    public class MessageConnectionResponse : MessageBase
    {
        [ProtoMember(1)]
        public bool OldCommunicationVersion;

        [ProtoMember(2)]
        public bool NewCommunicationVersion;

        [ProtoMember(3)]
        public ClientConfig ClientConfig;

        public static void SendMessage(ulong steamdId, bool oldCommunicationVersion, bool newCommunicationVersion, ClientConfig clientConfig)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageConnectionResponse
            {
                OldCommunicationVersion = oldCommunicationVersion,
                NewCommunicationVersion = newCommunicationVersion,
                ClientConfig = clientConfig
            });
        }

        public override void ProcessClient()
        {
            EconomyScript.Instance.ClientLogger.WriteInfo("Processing MessageConnectionResponse");

            // stop further requests
            if (EconomyScript.Instance.DelayedConnectionRequestTimer != null)
            {
                EconomyScript.Instance.DelayedConnectionRequestTimer.Stop();
                EconomyScript.Instance.DelayedConnectionRequestTimer.Close();
            }

            // config has been received already.
            if (EconomyScript.Instance.ClientConfig != null)
                return;

            if (ClientConfig == null)
            {
                if (MyAPIGateway.Session.Player.IsAdmin())
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The version of Economy running on your Server is out of date.\r\nPlease update and restart your server.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The version of Economy running on your Server is out of date.", 5000, MyFontEnum.Blue);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The Economy mod is currently unavailable as it is out of date.\r\nPlease contact your server Administrator.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The Economy mod is currently unavailable as it is out of date.", 5000, MyFontEnum.Blue);
                }

                EconomyScript.Instance.ClientLogger.WriteInfo("Warning: The Economy mod is currently unavailable as it is out of date. Please check to make sure you have downloaded the latest version of the mod.");
                return;
            }

            EconomyScript.Instance.ClientConfig = ClientConfig;

            EconomyScript.Instance.ClientLogger.WriteInfo("ClientConfig received: Opened {0}  Balance: {1}", ClientConfig.OpenedDate, ClientConfig.BankBalance);

            #region Initialise trade network hud

            // Set's up the hud for use.
            MyAPIGateway.Utilities.GetObjectiveLine().Title = EconomyScript.Instance.ClientConfig.TradeNetworkName;
            MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
            MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("");

            MyAPIGateway.Utilities.ShowMessage("Economy", "Network Connected!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Welcome to the {0} Trade Network!", EconomyScript.Instance.ClientConfig.TradeNetworkName);
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/ehelp' for more informations about available commands");

            // if we need to switch to next mission use this MyAPIGateway.Utilities.GetObjectiveLine().AdvanceObjective();
            //text of current objective useful for showmissionscreen string etc MyAPIGateway.Utilities.GetObjectiveLine().CurrentObjective;
            // bool?? MyAPIGateway.Utilities.GetObjectiveLine().Equals;
            // turn hud off MyAPIGateway.Utilities.GetObjectiveLine().Hide();
            //change objectives text in array MyAPIGateway.Utilities.GetObjectiveLine().Objectives[element]="blah";
            // increment decrement not sure how MyAPIGateway.Utilities.GetObjectiveLine().Visible;
            // probably easier to clear() then repopulate, i cant see how to decriment properly 
            //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.add|remove etc
            if (ClientConfig.ShowHud) { MyAPIGateway.Utilities.GetObjectiveLine().Show(); }

            #endregion Initialise trade network hud

            if (OldCommunicationVersion)
            {
                if (MyAPIGateway.Session.Player.IsAdmin())
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The version of Economy running on your Server is out of date.\r\nPlease update and restart your server.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The version of Economy running on your Server is out of date.", 5000, MyFontEnum.Blue);
                    // TODO: display OldCommunicationVersion.
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMissionScreen("Economy", "Warning", " ", "The Economy mod is currently unavailable as it is out of date.\r\nPlease contact your server Administrator.");
                    MyAPIGateway.Utilities.ShowNotification("Warning: The Economy mod is currently unavailable as it is out of date.", 5000, MyFontEnum.Blue);
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
    }
}
