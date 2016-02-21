namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;
    //using System.Collections.Generic;
    //using System.Text;
    //using System.Threading.Tasks;

    //This is a placeholder for generic mission texts.
    //Adapted off the bank balance script
    //non functional and should be returning a string to the hud not display a message on screen
    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = readout;
    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission: Survive | Deadline: Unlimited");
    
    //Although we could pre-populate the objectives[] array with all possible mission texts
    //there seems to be some issues going forward or backward to specific mission ids
    //it makes more sense to only populate it with texts from the players current mission chains
    //then we can reset the objectives array and repopulate it, then advance to the desired text
    //this also means the mission system can still work if objectives hud ever gets blocked
    // or we need to use mission boxes instead due to a hud conflict

    //although the mission system mainly runs client side and 
    //proto contracts are somewhat out of my depth here..  ive kept them as in theory mission texts
    //and the players current mission ID field will be likely stored server side for persistence
    //and most likely will be customised to a given server
    //the trouble here is how do I pass my mission chain back to the client for display

    /*
     * Basic summary of logic to go here -
     * 1: look up mission text from server misson file - including any immediate chains
       2: return this text to client side for storage in the objectives[] array
     * 3: roll / update the mission display to the appropriate position based on players mission ID 
     * 4: sundry logic in regard to win conditions ie isplayer position within 100 metres of objective gps
     * 5: check they are carrying mission related items, joined faction x etc
    
     * */

    [ProtoContract]
    public class MessageMission : MessageBase
    {
        [ProtoMember(1)]
        //public string UserName;
        public long MissionID;

        public static void SendMessage(long missionID)
        {
            ConnectionHelper.SendMessageToServer(new MessageMission { MissionID = missionID });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
 

            EconomyScript.Instance.ServerLogger.WriteVerbose("Mission Text request '{0}' from '{1}'", MissionID, SenderSteamId);
            string reply;

            if (MissionID <= 0) //did we get a mission id?  
            {
                // nope its 0 probably should show the generic survive mission
                //return "Mission: Survive | Deadline: Unlimited";
            }
            else // ok seems to be a valid integer over 0
            {
                //var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);
                //if (player != null && player.IsAdmin()) // hold on there, are we an admin first?
                //select case missionID
                // case 1
                //  return "mission 1 text"
                // case 2
                //  return "mission 2 text"
                //etc

            }
        }

    }
}
