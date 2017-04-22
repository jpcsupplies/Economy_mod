namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    /// <summary>
    /// player joins ANY faction OR a SPECIFIC faction
    /// </summary>
    [ProtoContract]
    public class JoinFactionMission : MissionBaseStruct
    {
        [ProtoMember(20)]
        public long FactionId { get; set; }

        public override string GetName()
        {
            return "Join a Faction";
        }
    }
}
