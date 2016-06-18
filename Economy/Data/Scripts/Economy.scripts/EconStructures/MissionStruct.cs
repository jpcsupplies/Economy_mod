namespace Economy.scripts.EconStructures
{
    using System.Xml.Serialization;
    using VRageMath;

    [XmlType("Mission")]
    public class MissionStruct
    {
        /// <summary>
        /// Unique identifier of the mission.
        /// </summary>
        public int MissionId { get; set; }

        /// <summary>
        /// The short name of the mission, to appear in the Hud.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A full description of the mission parameters.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Message displayed when the mission is completed sucessfully.
        /// Note, that it may be concatenated with other text, including the reward.
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// The type of mission.
        /// </summary>
        public MissionType Designation { get; set; }

        /// <summary>
        /// The target entity.
        /// </summary>
        public long TargetEntityId { get; set; }

        /// <summary>
        /// The location and size of a Spherical area.
        /// </summary>
        public BoundingSphereD? AreaSphere { get; set; }

        /// <summary>
        /// The location and size of a Box area.
        /// </summary>
        public BoundingBoxD? AreaBox { get; set; }

        /// <summary>
        /// How much credit is recieved when the mission is completed sucessfully.
        /// </summary>
        public decimal Reward { get; set; }

        /// <summary>
        /// Who the mission is assigned to currently.
        /// </summary>
        public ulong[] Assigned { get; set; }
    }
}
