namespace Economy.scripts.EconStructures
{
    using ProtoBuf;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRageMath;

    [XmlType("Market")]
    [ProtoContract]
    public class MarketStruct
    {
        /// <summary>
        /// The market Identifier.
        /// </summary>
        [ProtoMember(1)]
        public ulong MarketId { get; set; }

        /// <summary>
        /// Indicates that the market is open and operational.
        /// Npc markets are always open.
        /// Player markets can be closed or opened.
        /// </summary>
        [ProtoMember(2)]
        public bool Open { get; set; }

        /// <summary>
        /// The name of the Market.
        /// This should be set when the market is first created.
        /// </summary>
        [ProtoMember(3)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The shape of the market zone.
        /// </summary>
        [ProtoMember(4)]
        public MarketZoneType MarketZoneType { get; set; }

        /// <summary>
        /// The entity that the Market Zone will center upon.
        /// </summary>
        [ProtoMember(5)]
        public long EntityId { get; set; }

        /// <summary>
        /// The location and size of a Spherical Market zone.
        /// </summary>
        [ProtoMember(6)]
        public SerializableBoundingSphereD MarketZoneSphere { get; set; }

        /// <summary>
        /// The location and size of a Box Market zone.
        /// </summary>
        [ProtoMember(7)]
        public BoundingBoxD? MarketZoneBox { get; set; }

        /// <summary>
        /// A list of all possible items in the market, regardless of if they are to be made available.
        /// </summary>
        [ProtoMember(8)]
        public List<MarketItemStruct> MarketItems { get; set; } = new List<MarketItemStruct>();
    }
}
