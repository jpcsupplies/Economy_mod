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

        public List<MarketItemStruct> MarketItems;
    }
}
