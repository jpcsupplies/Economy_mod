namespace Economy.scripts.MissionStructures
{
    using ProtoBuf;

    //generic no mission status - although we could make a dont die within specific timeframe mission and use that deadline field
    //sort of like a pirates counter-bounty mission if they dont die in that time they get a reward/escape justice
    //eg a player with an auto bounty placed on them gets a timer in deadline showing how long until bounty expires
    [ProtoContract]
    public class StayAliveMission : MissionBaseStruct
    {
        public override string GetName()
        {
            return "Mission: Survive | Deadline: Unlimited";
        }
    }
}
