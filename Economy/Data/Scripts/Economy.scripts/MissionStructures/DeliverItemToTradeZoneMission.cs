namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //only applies if we implement emergency restock contract missions
    [ProtoContract]
    public class DeliverItemToTradeZoneMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Deliver X item to Trade zone Y";
        }
    }
}
