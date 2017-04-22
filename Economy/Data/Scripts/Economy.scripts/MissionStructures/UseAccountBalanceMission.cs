namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    [ProtoContract]
    public class UseAccountBalanceMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Type /bal to connect to network";
        }
    }
}
