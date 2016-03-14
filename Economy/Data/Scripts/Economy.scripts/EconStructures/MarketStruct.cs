namespace Economy.scripts.EconStructures
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRageMath;

    [XmlType("Market")]
    public class MarketStruct
    {
        /// <summary>
        /// The market Identifier.
        /// </summary>
        public ulong MarketId { get; set; }

        /// <summary>
        /// Indicates that the market is open and operational.
        /// Npc markets are always open.
        /// Player markets can be closed or opened.
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// The name of the Market.
        /// This should be set when the market is first created.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The shape of the market zone.
        /// </summary>
        public MarketZoneType MarketZoneType { get; set; }

        /// <summary>
        /// The entity that the Market Zone will center upon.
        /// </summary>
        public long EntityId { get; set; }

        /// <summary>
        /// The location and size of a Spherical Market zone.
        /// </summary>
        public BoundingSphereD? MarketZoneSphere { get; set; }

        /// <summary>
        /// The location and size of a Box Market zone.
        /// </summary>
        public BoundingBoxD? MarketZoneBox { get; set; }

        /// <summary>
        /// A list of all possible items in the market, regardless of if they are to be made available.
        /// </summary>
        public List<MarketItemStruct> MarketItems;
    }
}
