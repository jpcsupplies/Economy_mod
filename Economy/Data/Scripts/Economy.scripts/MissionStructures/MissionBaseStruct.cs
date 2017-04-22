namespace Economy.scripts.MissionStructures
{
    using System;
    using System.Xml.Serialization;
    using ProtoBuf;

    [XmlType("Mission")]
    [XmlInclude(typeof(BlockActivateMission))]
    [XmlInclude(typeof(BlockCaptureMission))]
    [XmlInclude(typeof(BlockDeactivateMission))]
    [XmlInclude(typeof(BlockDestroyMission))]
    [XmlInclude(typeof(BuySomethingMission))]
    [XmlInclude(typeof(DeliverItemToTradeZoneMission))]
    [XmlInclude(typeof(JoinFactionMission))]
    [XmlInclude(typeof(KillPlayerMission))]
    [XmlInclude(typeof(MineMission))]
    [XmlInclude(typeof(PayPlayerMission))]
    [XmlInclude(typeof(StayAliveMission))]
    [XmlInclude(typeof(TradeWithPlayerMission))]
    [XmlInclude(typeof(TravelMission))]
    [XmlInclude(typeof(UseAccountBalanceMission))]
    [XmlInclude(typeof(UseBuySellShipMission))]
    [XmlInclude(typeof(UseWorthMission))]
    [XmlInclude(typeof(WeldMission))]
    [ProtoContract]
    public abstract class MissionBaseStruct
    {
        /// <summary>
        /// Unique identifier of the mission.
        /// </summary>
        [ProtoMember(1)]
        public int MissionId { get; set; }

        /// <summary>
        /// Indicates what sort of player/group the mission is assigned to.
        /// </summary>
        [ProtoMember(2)]
        public MissionAssignmentType AssignmentType { get; set; }

        /// <summary>
        /// The player the mission is assigned to.
        /// </summary>
        [ProtoMember(3)]
        public ulong PlayerId { get; set; }

        /// <summary>
        /// An identifier is used when the same mission is assigned to many people.
        /// When one individual wins, some rule may be applied to the other missions.
        /// </summary>
        // Wish we could use System.Guid, except it is not allowed in ModAPI.
        [ProtoMember(4)]
        public Int64 GroupMissionId { get; set; }

        /// <summary>
        /// Indicates who can complete the mission and recieve the reward out of the assigned players.
        /// </summary>
        [ProtoMember(5)]
        public MissionWinRule WinRule { get; set; }

        /// <summary>
        /// How much credit is recieved when the mission is completed sucessfully.
        /// </summary>
        [ProtoMember(6)]
        public decimal Reward { get; set; }

        /// <summary>
        /// When the Mission was created and listed.
        /// </summary>
        [ProtoMember(7)]
        public DateTime OfferDate { get; set; }

        /// <summary>
        /// The Date/Time that a Mission will expire (if it expires).
        /// </summary>
        [ProtoMember(8)]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// If the assigned player has been presented with the Briefing yet.
        /// </summary>
        [ProtoMember(9)]
        public bool SeenBriefing { get; set; }

        /// <summary>
        /// The short name of the mission, to appear in the Hud.
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return string.Empty;
        }

        /// <summary>
        /// A full description of the mission parameters.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDescription()
        {
            return string.Empty;
        }

        /// <summary>
        /// Message displayed when the mission is completed sucessfully.
        /// Note, that it may be concatenated with other text, including the reward.
        /// </summary>
        public virtual string GetSuccessMessage()
        {
            return string.Empty;
        }

        /// <summary>
        /// Checks if the mission has met it's criteria and someone has won.
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckMission()
        {
            return false;
        }

        /// <summary>
        /// Checks and adds GPS coorindate to player if required by the Mission.
        /// </summary>
        public virtual void AddGps()
        {
        }

        public virtual void RemoveGps()
        {
        }
    }
}
