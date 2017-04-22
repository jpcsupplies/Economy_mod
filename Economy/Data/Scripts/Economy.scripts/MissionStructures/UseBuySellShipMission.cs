namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //only really applies if we have that feature working yet
    [ProtoContract]
    public class UseBuySellShipMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Buy/Sell a ship/station";
        }
    }
}
