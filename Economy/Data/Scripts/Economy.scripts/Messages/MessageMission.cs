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
    //assorted tests show this is code broken - as nothing happens when i call this :(
    //have moved all useful code to updatehud bool for now

    
    //Although we could pre-populate the objectives[] array with all possible mission texts
    //there seems to be some issues going forward or backward to specific mission ids
    //it makes more sense to only populate it with texts from the players current mission chains
    //then we can reset the objectives array and repopulate it, then advance to the desired text
    //this also means the mission system can still work if objectives hud ever gets blocked
    // or we need to use mission boxes instead due to a hud conflict

    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = readout;
    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add("Mission: Survive | Deadline: Unlimited");
    //although the mission system mainly runs client side and 
    //proto contracts are somewhat out of my depth here..  ive kept them as in theory mission texts
    //and the players current mission ID field will be likely stored server side for persistence
    //and most likely will be customised to a given server
    //the trouble here is how do I pass my mission chain back to the client for display

    /*
     * Basic summary of logic to go here -
     * Testing: return text based on mission id
     * 
     * Further testing: update mission hud
     * 
     * Final testing: make it pull mission from mission file not a switch
     * 
     * If all good then:     * 
     * 1: look up mission text from server misson file - including any immediate chains?
       2: return this text to client side for storage in the objectives[] array
     * 3: roll / update the mission display to the appropriate position based on players mission ID 
     * 4: sundry logic in regard to win conditions ie isplayer position within 100 metres of objective gps
     * 5: check they are carrying mission related items, joined faction x etc
    
     * */

    [ProtoContract]
    public class MessageMission : MessageBase
    {
        [ProtoMember(1)]
        public long MissionID;

        public static void SendMessage(long missionID)
        {
            ConnectionHelper.SendMessageToServer(new MessageMission { MissionID = missionID });
        }

        public override void ProcessClient()
        {

            // never processed on client (can this be used to update client hud? lets try)
            //appears to never run

            MessageClientTextMessage.SendMessage(SenderSteamId, "mission", (MissionID + "client side"));
        }

        public override void ProcessServer()
        {
            //this is meant to be used to pull open the missions.xml file server side, and return
            //appropriate data.  At the moment its just hardcoded test data
            //we could also just make a local copy client side as part of client connect
            //and remove the need for this server code entirely - which would make my life easier!
            //and probably improve server sim speed marginally!

            //also appears never to run?

            //EconomyScript.Instance.ServerLogger.WriteVerbose("Mission Text request '{0}' from '{1}'", MissionID, SenderSteamId);

            MessageClientTextMessage.SendMessage(SenderSteamId, "mission", (MissionID + "server side"));
                

            
        }

    }
}
