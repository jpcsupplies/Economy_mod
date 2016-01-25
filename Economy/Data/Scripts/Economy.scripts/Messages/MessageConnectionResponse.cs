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

        [ProtoMember(3)]
        public string TradeNetworkName;

        public static void SendMessage(ulong steamdId, bool oldCommunicationVersion, bool newCommunicationVersion, string tradeNetworkName)
        {
            ConnectionHelper.SendMessageToPlayer(steamdId, new MessageConnectionResponse
            {
                OldCommunicationVersion = oldCommunicationVersion,
                NewCommunicationVersion = newCommunicationVersion,
                TradeNetworkName = tradeNetworkName
            });
        }

        public override void ProcessClient()
        {
            // stop further requests
            if (EconomyScript.Instance.DelayedConnectionRequestTimer != null)
            {
                EconomyScript.Instance.DelayedConnectionRequestTimer.Stop();
                EconomyScript.Instance.DelayedConnectionRequestTimer.Close();
            }
#region Initialise trade network hud
            MyAPIGateway.Utilities.GetObjectiveLine().Title = EconomyConsts.TradeNetworkName;
            MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
            MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Type /bal to connect to network");
            // if we wanted a 2nd mission add it like this MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission");
            MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission: Survive | Deadline: Unlimited");
              

            MyAPIGateway.Utilities.ShowMessage("Economy", "Network Connected!");
            MyAPIGateway.Utilities.ShowMessage("Economy", "Welcome to the {0} Trade Network!", TradeNetworkName);
            MyAPIGateway.Utilities.ShowMessage("Economy", "Type '/ehelp' for more informations about available commands");

            // if we need to switch to next mission use this MyAPIGateway.Utilities.GetObjectiveLine().AdvanceObjective();
            //text of current objective useful for showmissionscreen string etc MyAPIGateway.Utilities.GetObjectiveLine().CurrentObjective;
            // bool?? MyAPIGateway.Utilities.GetObjectiveLine().Equals;
            // turn hud off MyAPIGateway.Utilities.GetObjectiveLine().Hide();
            //change objectives text in array MyAPIGateway.Utilities.GetObjectiveLine().Objectives[element]="blah";
            // increment decrement not sure how MyAPIGateway.Utilities.GetObjectiveLine().Visible;
            // probably easier to clear() then repopulate, i cant see how to decriment properly 
            //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.add|remove etc
            MyAPIGateway.Utilities.GetObjectiveLine().Show();
#endregion Initialise trade network hud

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
    }
}
