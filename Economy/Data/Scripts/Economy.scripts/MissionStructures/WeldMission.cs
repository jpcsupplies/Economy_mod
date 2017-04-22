namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //doesnt need to be a powered or owned block like activate mission
    //could have a placed blocks counter eg they have to build 27 blocks (equivalent to a one block room)
    //Lana suggests it could be help repair another players space ship too
    //Or require them to build a large/small space ship
    [ProtoContract]
    public class WeldMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Build / Weld Something";
        }
    }
}
