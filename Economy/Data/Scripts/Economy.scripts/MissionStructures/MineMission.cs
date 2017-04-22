namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //MissionPayment = 0;
    //missionGPS = (x,y,z)
    //create a client gps (caption, missionGPS);
    //write this gps to some sort of list so we know 
    //we need to remove it once we get there
    //could require it to be a specific ore or any ore

    [ProtoContract]
    public class MineMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Mine / Sell some ore";
        }
    }
}
