﻿namespace Economy.scripts.EconConfig
{
    using System.Collections.Generic;
    using EconStructures;
    using Messages;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;
    using VRageMath;

    /// <summary>
    /// This will manage the Missions.
    /// Currently it is temporarily running from the Client side only, and will need to be controlled server side when finished.
    /// </summary>
    public static class HudManager
    {
        private static string _oldReadout;
        private static string _oldObjective;
        private static int _hudCounter;
        private static int _missionCounter;
        private static List<MissionStruct> Missions = new List<MissionStruct>();

        static HudManager()
        {
            // TODO: this is a temporary structure, before we move this to a configurable data store that can be modified and persisted.
            // Missions will be stored on the server, but only current mission will be passed to the client.
            Missions.Add(new MissionStruct
            {
                MissionId = 0,
                Designation = MissionType.None,
                Name = "Mission: Survive | Deadline: Unlimited",
                Reward = 0
            });

            Missions.Add(new MissionStruct{
                MissionId = 1,
                Designation = MissionType.Mine,
                Name = "Mine / Sell some ore",
                Reward = 0
                //MissionPayment = 0;
                //missionGPS = (x,y,z)
                //create a client gps (caption, missionGPS);
                //write this gps to some sort of list so we know 
                //we need to remove it once we get there
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 2,
                Designation = MissionType.BuySomething,
                Name = "Buy something",
                Reward = 100
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 3,
                Designation = MissionType.PayPlayer,
                Name = "Pay a player",
                Reward = 600
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 4,
                Designation = MissionType.ShipWorth,
                Name = "Check what a ship/station is worth",
                Reward = 1000
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 5,
                Designation = MissionType.Weld,
                Name = "Build / Weld Something",
                Reward = 10000
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 6,
                Designation = MissionType.JoinFaction,
                Name = "Join a Faction",
                Reward = 10000
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 7,
                Designation = MissionType.TravelToArea,
                Name = "Investigate location 0,0,0",
                SuccessMessage = "You have sucessfully investigated the location.",
                AreaSphere = new BoundingSphereD(new Vector3D(0, 0, 0), 50),
                Reward = 1000
                //or in the case of a chain mission we add / advance to the next part of the mission chain
                //the next chain could be generated to be different depending how the previous part was completed
                //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission"); or 
                // if we need to switch to next mission in chain -  MyAPIGateway.Utilities.GetObjectiveLine().AdvanceObjective();
                //text of current objective useful for showmissionscreen string etc MyAPIGateway.Utilities.GetObjectiveLine().CurrentObjective
                //danger here is on mission chains - we need to reset the entire mission chain data and position in the hud - if we advance objective 
                //any subsequent new missions will be written to element 1 and we will still be on element 2 or 3 from the old mission.
                //I really need a lot more info on the inner workings of the mission system here..
                //either that or i avoid using advanceobjective entirely but that will mean we need missionID to be a 2 dimensional array, to track 
                //mission and chain position.. which sounds like a safer bet - but wastes memory since the mission system has an element variable already.
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 8,
                Designation = MissionType.KillPlayer,
                Name = "Bounty XX for killing player YY",
                Reward = 10000
                //This class of mission would need to check the target player(s) are near the current player, and check if they are alive or not.
                //Kills would be credited by "guilt by association" if a player for whom there is a bounty mission dies while near another player
                //with a bounty on them; then they get the credit for the kill.   It would need some additional checks for example -
                //By killer I mean a player with a currently active kill/bounty mission to kill the player who just died.
                //1: is the killer piloting a ship or turret,  2: is the killer holding a tool or weapon that can kill a player, 
                //3: is the killer currently on a mission to kill them - or in some profession or state that qualifies for bountys. (eg if we add professions)
                //Kill credit ranges could also vary.  A player not piloting a ship but holding a tool/weapon 50-200 metres, 
                //a player piloting a ship maybe 1000 metres
                //Bounties could also be open missions where any player in a faction other than the person who died get a credit for witnessing/causing the death
                //in this way we could potentially run automatic bounties on players who are pirates/criminals etc.
                //Admittedly this would be more effective on a perminent death server. But is still fun if not.
                //Where multiple players are nearby we could simply pay an equal share; although this does present some exploitable problems - 
                //This effect could be limited to only players holding a bounty on the dead player allowing for team bounty hunting.

                //Ideally bounties on AI ships/stations would be useful too, but how can you detect if they have been defeated?
                //run an ispowered() check on all ship fragments with that ship/station ID?  Or check the command block is still hostile and powered?
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 9,
                Designation = MissionType.BuySellShip,
                Name = "Buy/Sell a ship/station",
                Reward = 10000
                //only really applies if we have that feature working yet
            });

            Missions.Add(new MissionStruct
            {
                MissionId = 10,
                Designation = MissionType.DeliverItemToTradeZone,
                Name = "Deliver X item to Trade zone Y",
                Reward = 10000
                //only applies if we implement emergency restock contract missions
            });
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

            if (clientConfig.ShowHud)
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

                //use title here that frees up mission line for actual missions - cargo should list total and used space or just empty space?

                string readout = null;
                if (clientConfig.HudReadout != null)
                {
                    Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();

                    readout = string.Format(clientConfig.HudReadout,
                        clientConfig.BankBalance,
                        clientConfig.CurrencyName,
                        position.X,
                        position.Y,
                        position.Z);
                }

                if (clientConfig.ShowFaction)
                {
                    if (_oldObjective != clientConfig.HudObjective)
                    {
                        //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                        MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = clientConfig.HudObjective;
                        //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add(reply);
                        _oldObjective = clientConfig.HudObjective;
                    }
                }

                if (_oldReadout != readout)
                {
                    MyAPIGateway.Utilities.GetObjectiveLine().Title = readout;
                    _oldReadout = readout;
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

            if (clientConfig.ShowHud)
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
                if (clientConfig.ShowBalance) readout += "{0:#,##0.0000} {1}";

                Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                if (clientConfig.ShowRegion) readout += " | Trade region: Unknown";     // TODO: Get tradezone from player current position.

                if (clientConfig.ShowXYZ)
                    readout += " | " + "X: {2:F0} Y: {3:F0} Z: {4:F0}";

                if (clientConfig.ShowContractCount)
                    readout += " | Contracts: 0";
                if (clientConfig.ShowCargoSpace)
                    readout += " | Cargo ? of ?";
                if (clientConfig.ShowFaction)
                {
                    string faction = "Free agent";
                    IMyFaction plFaction;
                    //IMyPlayer Me = MyAPIGateway.Session.Player; why waste the bytes
                    plFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);
                    if (plFaction != null)
                    {
                        faction = plFaction.Name;  //should this show tag or full name? depends on screen size i suppose
                    }
                    readout += " | Agency: " + faction;

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

            if (missionId >= Missions.Count)
                missionId = 0;

            EconomyScript.Instance.ClientConfig.MissionId = missionId;
            EconomyScript.Instance.ClientConfig.LazyMissionText = Missions[EconomyScript.Instance.ClientConfig.MissionId].Name;
        }

        public static void UpdateMission()
        {
            ClientConfig clientConfig = EconomyScript.Instance.ClientConfig;

            // client config has not been received from server yet.
            if (clientConfig == null)
                return;

            if (clientConfig.ShowFaction)
            {
                Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                string faction = "Free agent";
                IMyFaction plFaction;
                //IMyPlayer Me = MyAPIGateway.Session.Player; why waste the bytes
                plFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);
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

                var mission = Missions[clientConfig.MissionId];

                bool success = false;
                switch (mission.Designation)
                {
                    case MissionType.TravelToArea:
                    {
                        success = ((mission.AreaSphere.HasValue && mission.AreaSphere.Value.Contains(position) == ContainmentType.Contains) ||
                                   (mission.AreaBox.HasValue && mission.AreaBox.Value.Contains(position) == ContainmentType.Contains));
                        break;
                    }
                }

                if (success)
                {
                    string msg = mission.SuccessMessage;

                    if (mission.Reward != 0)
                    {
                        MessageRewardAccount.SendMessage(mission.Reward);
                        msg += "\r\n1000 Credits Transferred to your account.";
                    }

                    MyAPIGateway.Utilities.ShowMissionScreen("Mission", "", "Completed", msg, null, "Okay");

                    clientConfig.LazyMissionText = clientConfig.MissionId + " Mission: completed";

                    // TODO: is a bad idea to increment. We need to fetch the next valid mission, or reset to blank.
                    FetchMission(0);
                    //FetchMission(clientConfig.MissionId + 1);
                    UpdateHud();
                }
            }
        }

    }
}
