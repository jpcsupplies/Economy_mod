namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;
    using VRageMath;

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
    //we could also use waypoints/gps points here which are removed as player investigates them for mission chains
    //if we ever implement vanity names/scan database trading this could be used to require a player to /scan a specific location too
    //Lana suggests fly a specific type of ship (large / small / spacesuit) too
    [ProtoContract]
    public class TravelMission : MissionBaseStruct
    {
        /// <summary>
        /// The location and size of a Spherical area.
        /// </summary>
        [ProtoMember(20)]
        public BoundingSphereD? AreaSphere { get; set; }

        /// <summary>
        /// The location and size of a Box area.
        /// </summary>
        [ProtoMember(21)]
        public BoundingBoxD? AreaBox { get; set; }

        public override string GetName()
        {
            Vector3D center = new Vector3D();
            if (AreaSphere.HasValue)
                center = AreaSphere.Value.Center;
            else if (AreaBox.HasValue)
                center = AreaBox.Value.Center;

            return string.Format("Investigate location {0:0},{1:0},{2:0}", center.X, center.Y, center.Z);
        }

        public override string GetDescription()
        {
            Vector3D center = new Vector3D();
            if (AreaSphere.HasValue)
                center = AreaSphere.Value.Center;
            else if (AreaBox.HasValue)
                center = AreaBox.Value.Center;

            return string.Format("We need you to investigate location {0:0},{1:0},{2:0}!\r\nHead on over and take a look around.\r\nA GPS point has been created for you.", center.X, center.Y, center.Z);
        }

        public override string GetSuccessMessage()
        {
            return "You have sucessfully investigated the location.";
        }

        public override bool CheckMission()
        {
            IMyPlayer player;
            if (MyAPIGateway.Players.TryGetPlayer(PlayerId, out player))
            {
                Vector3D position = player.GetPosition();
                return ((AreaSphere.HasValue && AreaSphere.Value.Contains(position) == ContainmentType.Contains) ||
                        (AreaBox.HasValue && AreaBox.Value.Contains(position) == ContainmentType.Contains));
            }

            return false;
        }

        public override void AddGps()
        {
            Vector3D center = new Vector3D();
            if (AreaSphere.HasValue)
                center = AreaSphere.Value.Center;
            else if (AreaBox.HasValue)
                center = AreaBox.Value.Center;

            EconConfig.HudManager.GPS(center.X, center.Y, center.Z, "Mission Objective^" + MissionId, GetName(), true);
        }

        public override void RemoveGps()
        {
            Vector3D center = new Vector3D();
            if (AreaSphere.HasValue)
                center = AreaSphere.Value.Center;
            else if (AreaBox.HasValue)
                center = AreaBox.Value.Center;

            EconConfig.HudManager.GPS(center.X, center.Y, center.Z, "Mission Objective^" + MissionId, GetName(), false);
        }
    }
}