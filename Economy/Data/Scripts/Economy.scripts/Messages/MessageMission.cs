namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using MissionStructures;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage;
    using VRageMath;

    /*
     * Basic summary of logic to go here -
     * Testing: return text based on mission id
     * 
     * Further testing: update mission hud
     * 
     * Final testing: make it pull mission from mission file not a switch
     * 
     * If all good then:     * 
     * 1: look up mission text from server misson file - including any immediate chains?
       2: return this text to client side for storage in the objectives[] array
     * 3: roll / update the mission display to the appropriate position based on players mission ID 
     * 4: sundry logic in regard to win conditions ie isplayer position within 100 metres of objective gps
     * 5: check they are carrying mission related items, joined faction x etc
    
     * */

    [ProtoContract]
    public class MessageMission : MessageBase
    {
        #region properties

        [ProtoMember(1)]
        public PlayerMissionManage CommandType;

        [ProtoMember(2)]
        public long MissionId;

        [ProtoMember(3)]
        public MissionBaseStruct Mission;

        #endregion

        #region send messages

        public static void SendMissionComplete(MissionBaseStruct mission)
        {
            mission.RemoveGps();
            EconomyScript.Instance.ClientConfig.Missions.Remove(mission);

            ConnectionHelper.SendMessageToServer(new MessageMission { CommandType = PlayerMissionManage.MissionComplete, Mission = mission });

            string msg = mission.GetSuccessMessage();
            if (mission.Reward != 0)
                msg += string.Format("\r\n{0} {1} Transferred to your account.", mission.Reward, EconomyScript.Instance.ClientConfig.CurrencyName);
            MyAPIGateway.Utilities.ShowMissionScreen("Mission:" + mission.MissionId, "", "Completed", msg, null, "Okay");
        }

        public static void SendMessage(long missionId)
        {
            ConnectionHelper.SendMessageToServer(new MessageMission { CommandType = PlayerMissionManage.Test, MissionId = missionId });
        }

        public static void SendCreateSampleMissions()
        {
            ConnectionHelper.SendMessageToServer(new MessageMission { CommandType = PlayerMissionManage.AddSample });
        } 

        #endregion

        public override void ProcessClient()
        {
            switch (CommandType)
            {
                case PlayerMissionManage.AddMission:
                    EconomyScript.Instance.ClientConfig.Missions.Add(Mission);
                    Mission.AddGps();
                    MyAPIGateway.Utilities.ShowMissionScreen("Mission", Mission.MissionId + " : ", Mission.GetName(), Mission.GetDescription(), null, "Yes Sir!");
                    Mission.SeenBriefing = true;
                    EconomyScript.Instance.ClientConfig.MissionId = Mission.MissionId;
                    EconomyScript.Instance.ClientConfig.LazyMissionText = Mission.GetName();
                    HudManager.UpdateHud();
                    break;

                default:
                    //MessageClientTextMessage.SendMessage(SenderSteamId, "mission", (MissionId + "client side"));
                    break;
            }
        }

        public override void ProcessServer()
        {
            // update our own timestamp here
            AccountManager.UpdateLastSeen(SenderSteamId, SenderLanguage);
            EconomyScript.Instance.ServerLogger.WriteVerbose("Manage Player Mission from '{0}'", SenderSteamId);

            switch (CommandType)
            {
                case PlayerMissionManage.Test:
                    //EconomyScript.Instance.ServerLogger.WriteVerbose("Mission Text request '{0}' from '{1}'", MissionID, SenderSteamId);
                    MessageClientTextMessage.SendMessage(SenderSteamId, "mission", (MissionId + " server side"));
                    break;

                case PlayerMissionManage.AddSample:
                    {
                        var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        var playerMatrix = player.Character.WorldMatrix;
                        //var position = player.Character.GetPosition();
                        Vector3D position = playerMatrix.Translation + playerMatrix.Forward * 60f;

                        MissionBaseStruct newMission = CreateMission(new TravelMission
                        {
                            AreaSphere = new BoundingSphereD(position, 50),
                            //AreaSphere = new BoundingSphereD(new Vector3D(0, 0, 0), 50),
                            Reward = 100,
                        }, SenderSteamId);

                        ConnectionHelper.SendMessageToPlayer(SenderSteamId, new MessageMission { CommandType = PlayerMissionManage.AddMission, Mission = newMission });
                    }
                    break;

                case PlayerMissionManage.AddMission:
                    // Nothing should happen here, because the server sends missions to the client, not the other way.
                    break;

                case PlayerMissionManage.SyncMission:
                    // TODO: sync details back from the client to the server.
                    break;

                case PlayerMissionManage.DeleteMission:
                    // TODO: Delete the mission.
                    break;

                case PlayerMissionManage.MissionComplete:
                    // This should process the mission reward if appropriate and then delete from server.
                    // We aren't archiving finished missions.

                    MissionBaseStruct serverinstanceMission = GetMission(Mission.MissionId);

                    if (serverinstanceMission != null && serverinstanceMission.PlayerId == SenderSteamId)
                    {
                        var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                        if (player != null)
                        {
                            // we look up our bank record based on our Steam Id/
                            // create balance if not one already, then add our reward and update client.
                            var myaccount = AccountManager.FindOrCreateAccount(SenderSteamId, player.DisplayName, SenderLanguage);

                            EconomyScript.Instance.Data.CreditBalance -= serverinstanceMission.Reward;
                            myaccount.BankBalance += serverinstanceMission.Reward;
                            myaccount.Date = DateTime.Now;

                            MessageUpdateClient.SendAccountMessage(myaccount);
                            MessageClientSound.SendMessage(SenderSteamId, "SoundBlockObjectiveComplete");
                        }

                        RemoveMission(serverinstanceMission);
                    }

                    break;
            }
        }

        private void CreateSampleMissions()
        {
            // TODO: this is a temporary structure, before we move this to a configurable data store that can be modified and persisted.
            // Missions will be stored on the server, but only current mission will be passed to the client.
            // the following are an example of each potential mission type, in a custom missions system these are the types 
            // of missions available for admins to create, or as a generic set of tutorial missions to teach player how to use economy

            CreateMission(new StayAliveMission
            {
                MissionId = 0,
                Reward = 0
            }, 0);

            CreateMission(new UseAccountBalanceMission
            {
                MissionId = 1,
                Reward = 10
            }, 0);

            CreateMission(new MineMission
            {
                MissionId = 2,
                Reward = 0
            }, 0);

            CreateMission(new BuySomethingMission
            {
                MissionId = 3,
                Reward = 100
            }, 0);

            CreateMission(new PayPlayerMission
            {
                MissionId = 4,
                Reward = 600
            }, 0);

            CreateMission(new TradeWithPlayerMission
            {
                MissionId = 5,
                Reward = 600
            }, 0);

            CreateMission(new UseWorthMission
            {
                MissionId = 6,
                Reward = 1000
            }, 0);

            CreateMission(new WeldMission
            {
                MissionId = 7,
                Reward = 10000
            }, 0);

            CreateMission(new JoinFactionMission
            {
                MissionId = 8,
                Reward = 10000
            }, 0);

            CreateMission(new TravelMission
            {
                MissionId = 9,
                AreaSphere = new BoundingSphereD(new Vector3D(0, 0, 0), 50),
                Reward = 100
            }, 0);

            CreateMission(new KillPlayerMission
            {
                MissionId = 10,
                TargetEntityId = 0,
                Reward = 10000
            }, 0);

            CreateMission(new UseBuySellShipMission
            {
                MissionId = 11,
                Reward = 10000
            }, 0);

            CreateMission(new DeliverItemToTradeZoneMission
            {
                MissionId = 12,
                Reward = 10000
            }, 0);

            CreateMission(new BlockDeactivateMission
            {
                MissionId = 13,
                Reward = 10000
            }, 0);

            CreateMission(new BlockActivateMission
            {
                MissionId = 14,
                Reward = 10000
            }, 0);

            CreateMission(new BlockDestroyMission
            {
                MissionId = 15,
                Reward = 10000
            }, 0);

            CreateMission(new BlockCaptureMission
            {
                MissionId = 16,
                Reward = 10000
            }, 0);
        }

        private static readonly FastResourceLock ExecutionLock = new FastResourceLock();

        private static MissionBaseStruct CreateMission(MissionBaseStruct mission, ulong assignToPlayer)
        {
            using (ExecutionLock.AcquireExclusiveUsing())
            {
                int newMissionId = 1;
                if (EconomyScript.Instance.Data.Missions.Count != 0)
                    newMissionId = EconomyScript.Instance.Data.Missions.Max(m => m.MissionId) + 1;

                mission.MissionId = newMissionId;
                mission.PlayerId = assignToPlayer;

                EconomyScript.Instance.Data.Missions.Add(mission);
            }
            return mission;
        }

        private static void RemoveMission(MissionBaseStruct mission)
        {
            using (ExecutionLock.AcquireExclusiveUsing())
            {
                EconomyScript.Instance.Data.Missions.Remove(mission);
            }
        }

        private static MissionBaseStruct GetMission(int missionId)
        {
            using (ExecutionLock.AcquireExclusiveUsing())
            {
                return EconomyScript.Instance.Data.Missions.FirstOrDefault(m => m.MissionId == missionId);
            }
        }
    }
}
