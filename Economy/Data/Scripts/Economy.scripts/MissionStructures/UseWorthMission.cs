namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //runs the worth command on anything - or on a specified ID or location?
    [ProtoContract]
    public class UseWorthMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Check what a ship/station is worth";
        }
    }
}
