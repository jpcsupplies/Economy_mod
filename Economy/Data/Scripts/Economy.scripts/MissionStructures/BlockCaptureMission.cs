namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //eg change ownership of block to self (or nominated player in case of steal mission)
    //Lana suggests steal a persons spaceship as a mission too
    [ProtoContract]
    public class BlockCaptureMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Capture/Hack Block";
        }
    }
}
