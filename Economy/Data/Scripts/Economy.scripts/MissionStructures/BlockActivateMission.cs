namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //eg a block at a given location is of a given type and turned on/ ispowered()?
    [ProtoContract]
    public class BlockActivateMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Enable/Turnon/Build or Repair Block";
        }
    }
}
