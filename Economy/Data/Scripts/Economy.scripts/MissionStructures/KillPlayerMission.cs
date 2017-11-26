namespace Economy.scripts.MissionStructures
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;

    //This class of mission would need to check the target player(s) are near the current player, and check if they are alive or not.
    //Kills would be credited by "guilt by association" if a player for whom there is a bounty mission dies while near another player
    //with a bounty on them; then they get the credit for the kill.   It would need some additional checks for example -
    //By killer I mean a player with a currently active kill/bounty mission to kill the player who just died.
    //1: is the killer piloting a ship or turret,  2: is the killer holding a tool or weapon that can kill a player, 
    //3: is the killer currently on a mission to kill them - or in some profession or state that qualifies for bountys. (eg if we add professions)
    //Kill credit ranges could also vary.  A player not piloting a ship but holding a tool/weapon 50-200 metres, 
    //we may need a way to detect mod/workshop weapons (possible future bug)
    //a player piloting a ship maybe 1000 metres
    //Bounties could also be open missions where any player in a faction other than the person who died get a credit for witnessing/causing the death
    //in this way we could potentially run automatic bounties on players who are pirates/criminals etc. eg if a pirate player kills a player on  trading mission
    //the pirate automatically has their bountry increased - be cool if we can scan players and auto show bounties in hud if profession used
    //Admittedly this would be more effective on a perminent death server. But is still fun if not.
    //Where multiple players are nearby we could simply pay an equal share; although this does present some exploitable problems - 
    //This effect could be limited to only players holding a bounty on the dead player allowing for team bounty hunting.

    //Ideally bounties on AI ships/stations would be useful too, but how can you detect if they have been defeated?
    //run an ispowered() check on all ship fragments with that ship/station ID?  Or check the command block is still hostile and powered?

    //Lana suggests kill missions on monsters too.. eg kill 20 spiders
    [ProtoContract]
    public class KillPlayerMission : MissionBaseStruct
    {
        [ProtoMember(201)]
        private long _targetEntityId { get; set; }

        /// <summary>
        /// The target entity.
        /// </summary>
        [XmlIgnore]
        public long TargetEntityId
        {
            get { return _targetEntityId; }

            set
            {
                _targetEntityId = value;
                Refresh();
            }
        }

        public string TargetName { get; set; }

        private void Refresh()
        {
            var listPlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(listPlayers, p => p.IdentityId == TargetEntityId);
            IMyPlayer player = listPlayers.FirstOrDefault();

            if (player == null)
                TargetName = null;
            else
                TargetName = player.DisplayName;
        }

        public override string GetName()
        {
            return string.Format("Bounty {0} for killing player {1}", Reward, TargetName);
        }

        public override string GetDescription()
        {
            if (TargetName == null)
                return "No player targeted";

            return string.Format("Your mission is to kill {0}.", TargetName);
        }
    }
}
