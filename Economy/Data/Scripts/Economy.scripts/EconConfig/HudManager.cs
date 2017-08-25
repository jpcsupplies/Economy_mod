namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EconStructures;
    using Messages;
    using MissionStructures;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;
    using VRageMath;

    /// <summary>
    /// This will manage the Missions.
    /// Currently it is temporarily running from the Client side only, and will need to be controlled server side when finished.
    /// Mission completion condition checks (for a players current mission) should try if possible to run client side  to 
    /// distrubute load over players instead of using server sim
    /// </summary>
    public static class HudManager
    {
        private static int _hudCounter;
        private static int _missionCounter;

        static HudManager()
        {
        }

        public static void UpdateAfterSimulation()
        {
            _hudCounter++;
            _missionCounter++;

            if (_hudCounter > 10) // only update hud details every Xth frame.
            {
                DisplayHud();
                _hudCounter = 0;
            }

            if (_missionCounter > 200)
            {
                UpdateMission();
                _missionCounter = 0;
            }
        }

        /// <summary>
        /// Displays the current hud information.
        /// It should not contain any complex calculation, merly update the Hud as fast as possible.
        /// </summary>
        private static void DisplayHud()
        {
            ClientConfig clientConfig = EconomyScript.Instance.ClientConfig;

            // client config has not been received from server yet.
            if (clientConfig == null)
                return;

            if (clientConfig.ClientHudSettings.ShowHud)
            {
                IMyHudObjectiveLine objective = MyAPIGateway.Utilities.GetObjectiveLine();

                //Hud, displays users balance, trade network name, and optionally faction and free storage space (% or unit?) in cargo and/or inventory
                //may also eventually be used to display info about completed objectives in missions/jobs/bounties/employment etc
                //needs to call this at init (working), and at each call to message handling(working), and on recieving any notification of payment.
                //since other balance altering scenarios such as selling stock requires a command or prompt by player calling this
                //at message handler should update in those scenarios automatically. That should avoid need for a timing loop and have no obvious sim impact

                /* We need a routine populating a field similar the the below regularly, which pushes
                 * updates to the x y z location readout if it is enabled
                 * this will also be critical for checking against mission conditions
                 * once missions are functional, that require a user travelling to a location
                
                 * something like this can be used to populate the trade region part of the hud as well
                 * where we find ourself in a trade region
                 * it could probably hang off the timer code that updates lcds
                 var position = ((IMyEntity)character).WorldMatrix.Translation;
                 var markets = MarketManager.FindMarketsFromLocation(position);
                 */

                //use title here that frees up mission line for actual missions - cargo should list total and used space or just empty space?

                string readout = null;
                if (clientConfig.HudReadout != null)
                {
                    // if the player does not have a controlable body yet, no point displaying a hud.
                    if (MyAPIGateway.Session.Player.Controller != null
                        && MyAPIGateway.Session.Player.Controller.ControlledEntity != null
                        && MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity != null)
                    {
                        Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();

                        readout = string.Format(clientConfig.HudReadout,
                            clientConfig.BankBalance,
                            clientConfig.CurrencyName,
                            position.X,
                            position.Y,
                            position.Z,
                            clientConfig.FactionName);
                    }
                }

                if (clientConfig.ClientHudSettings.ShowFaction)
                {
                    if (objective.Objectives.Count < 1)
                        objective.Objectives.Add(clientConfig.HudObjective);
                    else if (objective.Objectives[0] != clientConfig.HudObjective)
                    {
                        // if we wanted a 2nd mission add it like this MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission");
                        //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add(ClientConfig.LazyMissionText); //testing if my global is available
                        objective.Objectives[0] = clientConfig.HudObjective;
                    }
                }

                if (objective.Title != readout)
                {
                    objective.Title = readout;
                }
            }
            //MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = readout;  //using title not mission text now
        }

        /// <summary>
        /// This will update values to appear on the hud.
        /// </summary>
        /// <returns></returns>
        public static bool UpdateHud()
        {
            ClientConfig clientConfig = EconomyScript.Instance.ClientConfig;

            // client config has not been received from server yet.
            if (clientConfig == null)
                return true;

            if (clientConfig.ClientHudSettings.ShowHud)
            {
                //Hud, displays users balance, trade network name, and optionally faction and free storage space (% or unit?) in cargo and/or inventory
                //may also eventually be used to display info about completed objectives in missions/jobs/bounties/employment etc
                //needs to call this at init (working), and at each call to message handling(working), and on recieving any notification of payment.
                //since other balance altering scenarios such as selling stock requires a command or prompt by player calling this
                //at message handler should update in those scenarios automatically. That should avoid need for a timing loop and have no obvious sim impact

                /* We need a routine populating a field similar the the below regularly, which pushes
                 * updates to the x y z location readout if it is enabled
                 * this will also be critical for checking against mission conditions
                 * once missions are functional, that require a user travelling to a location
                
                 * something like this can be used to populate the trade region part of the hud as well
                 * where we find ourself in a trade region
                 * it could probably hang off the timer code that updates lcds
                 var position = ((IMyEntity)character).WorldMatrix.Translation;
                 var markets = MarketManager.FindMarketsFromLocation(position);
                 */
                /* account.BankBalance.ToString("0.######"); */

                //use title here that frees up mission line for actual missions - cargo should list total and used space or just empty space?
                string readout = clientConfig.TradeNetworkName + ": ";
                if (clientConfig.ClientHudSettings.ShowBalance) readout += "{0:#,##0.0000} {1}";

                string tradeZoneName = "Unknown";
                if (MyAPIGateway.Session.Player.Controller != null
                    && MyAPIGateway.Session.Player.Controller.ControlledEntity != null
                    && MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity != null)
                {
                    Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                    // TODO: Get tradezone from player current position.
                }

                if (clientConfig.ClientHudSettings.ShowRegion) readout += " | Trade region: " + tradeZoneName;

                if (clientConfig.ClientHudSettings.ShowPosition)
                    readout += " | " + "X: {2:F0} Y: {3:F0} Z: {4:F0}";

                if (clientConfig.ClientHudSettings.ShowContractCount)
                    readout += " | Tasks: " + (clientConfig.CompletedMissions-1) + " of 2";
                if (clientConfig.ClientHudSettings.ShowCargoSpace)
                    readout += " | Cargo ? of ?";
                if (clientConfig.ClientHudSettings.ShowFaction)
                {
                    string faction = "Free agent";
                    IMyFaction plFaction;
                    
                    plFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.IdentityId);
                    if (plFaction != null)
                    {
                        faction = plFaction.Name;  //should this show tag or full name? depends on screen size i suppose
                    }
                    readout += " | Agency: {5}";

                    clientConfig.FactionName = faction;

                    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                    clientConfig.HudObjective = clientConfig.LazyMissionText;
                    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add(reply);
                }

                clientConfig.HudReadout = readout;
            }

            return true;  //probably need a catch of some sort for a return false, but anything going wrong here is probably part of another issue.
        }

        public static void FetchMission(int missionId)
        {
            EconomyScript.Instance.ClientConfig.MissionId++;

            MissionBaseStruct currentMission = EconomyScript.Instance.ClientConfig.Missions.FirstOrDefault(m => m.MissionId == missionId);
            if (currentMission == null)
                missionId = -1;
            EconomyScript.Instance.ClientConfig.MissionId = missionId;
            EconomyScript.Instance.ClientConfig.LazyMissionText = currentMission == null ? null : currentMission.GetName();
        }

        public static void GPS(double x,double y, double z, string name, string description, bool create)
        {
            //make a gps point for the objective.  eg GPS(x,y,z,name,description,true)
            //remove an existing GPS point  eg GPS(x,y,z,name,description,false)
            //Helps Automate process for creating/removing investigate/mine here/kill this/destroy/repair/etc missions markers

            //ye not sure how to assign this as the initialised value in a vector need help :) this is my work around
            Vector3D location = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
            location.X = x; location.Y = y; location.Z = z;

            if (create)
            {  //make a new GPS
                var gps = MyAPIGateway.Session.GPS.Create(name, description, location, true, false);
                MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.Player.IdentityId, gps);
            }
            else
            { //remove a GPS
                var list = MyAPIGateway.Session.GPS.GetGpsList(MyAPIGateway.Session.Player.IdentityId);
                foreach (var gps in list)
                {
                    if (gps.Description == description || gps.Name == name || gps.Coords == location)
                    {
                        MyAPIGateway.Session.GPS.RemoveGps(MyAPIGateway.Session.Player.IdentityId, gps);
                    }
                }
            }
        }

        public static void UpdateMission()
        {
            ClientConfig clientConfig = EconomyScript.Instance.ClientConfig;

            // client config has not been received from server yet.
            if (clientConfig == null)
                return;

            // if the player does not have a controlable body yet, no point continuing.
            if (MyAPIGateway.Session.Player.Controller == null
                || MyAPIGateway.Session.Player.Controller.ControlledEntity == null
                || MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity == null)
                return;

            if (clientConfig.ClientHudSettings.ShowFaction)
            {
                Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                string faction = "Free agent";
                IMyFaction plFaction;
                
                plFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.IdentityId);
                if (plFaction != null)
                {
                    faction = plFaction.Name;  //should this show tag or full name? depends on screen size i suppose
                }

                //mission system
                /* 
                 Mission system will use a set of conditions which are set on commencement of mission. conditions not used are nul
                 * target location  eg 1000,1000,1000
                 * target player eg xphoenixxx
                 * target trade zone eg Trader
                 * target ship/station id? destruction or capture/rename would end the mission (much like beacons becoming unlinked on trade zones)
                 * target event eg buy/sell/pay/drop/collect/kill?/capture?
                 * target reward eg 1000 credits or 100 missile crates etc or a prefab id
                 * Lazy missiontext is only using mission hud id  0 or 1 at the moment and changing the text based on clientConfig.missionid and active conditions
                 * this allows us to use the internal hud mission ids with increment for more complex mission chains (eg patrol points multiple objectives etc)
                 * or skip it entirely for single objective missions (eg deliver x item to y location)
                 * on top of this we can use a pick-a-path or choose your own adventure style system where we substitute page number with missionID - 
                 * these style missions will be partially fixed by loading from a mission file allowing admins to design their own story missions
                 * hard coded missions could be the emergency restock contracts which trigger automatically/randomly as stock levels plummet contracts could work as a
                 * seperate system as hud space is limited. eg /contracts or we could work it in as a mission chain under main mission system
                 * this allows for very complicated missions to be assembled using quite simple code (i like simple i understand simple!)
                 * we need to save clientConfig.missionid client side so players can continue missions
                 * if conditions are encoded in hud mission text, we could save that too and restore on rejoin

                 Below are some sample example missions showing a few standard mission types which could be used in a tutorial chain - or auto generated - or created by
                 server admins from a custom mission file.
                 */
                //int MissionPayment = 0;


                MissionBaseStruct currentMission = EconomyScript.Instance.ClientConfig.Missions.FirstOrDefault(m => m.MissionId == clientConfig.MissionId);

                // no mission currently selected. or no valid mission selected.
                if (currentMission == null)
                    return;

                bool success = currentMission.CheckMission();

                if (success)
                {
                    clientConfig.SeenBriefing = false; //reset the check that tests if player has seen briefing and/or had gps created already
                    clientConfig.CompletedMissions++; //increment the completed missions counter (note this is not intended to be 'current' mission just a counter)
                    //it will probably only be used for current mission "chain" tracking later on
                    if (clientConfig.CompletedMissions >= 3)
                    { //demo mission chain is over clean up unnecessary hud sections we added earlier
                        clientConfig.ClientHudSettings.ShowContractCount = false;
                        clientConfig.ClientHudSettings.ShowPosition = false;
                    }

                    MessageMission.SendMissionComplete(currentMission);

                    clientConfig.LazyMissionText = clientConfig.MissionId + " Mission: completed";

                    // TODO: is a bad idea to increment. We need to fetch the next valid mission, or reset to blank.
                    //FetchMission(-1); 
                    FetchMission(0);
                    //FetchMission(clientConfig.MissionId + 1);
                    UpdateHud();
                }
            }
        }

    }
}
