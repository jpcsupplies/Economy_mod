namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //eg turn off a reactor, disable a warhead etc - if you simply want to grind down warhead etc
    [ProtoContract]
    public class BlockDeactivateMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Disable/Turnoff Block";
        }
    }
}
