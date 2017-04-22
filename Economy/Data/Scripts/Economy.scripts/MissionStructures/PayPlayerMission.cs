namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //pays any player or a specified player, or a player in a faction?
    [ProtoContract]
    public class PayPlayerMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Pay a player";
        }
    }
}
