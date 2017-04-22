namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //sells an item to another player or buys something from someone direct (ie not from a trade zone)
    [ProtoContract]
    public class TradeWithPlayerMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Buy or Sell an item directly with another player";
        }
    }
}
