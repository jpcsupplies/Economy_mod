namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //buy an item or any item from any trade zone or a specific trade zone
    [ProtoContract]
    public class BuySomethingMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Buy something";
        }
    }
}
