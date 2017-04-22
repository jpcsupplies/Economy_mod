namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //eg destroy/remove warhead, remove reactor etc - grinding or blowing up
    [ProtoContract]
    public class BlockDestroyMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Destroy Block";
        }
    }
}
