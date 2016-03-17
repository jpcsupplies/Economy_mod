/*
 *  Economy Mod <see cref="EconomyConsts.MajorVer"/>
 *  by PhoenixX (JPC Dev), Screaming Angels (Midspace), Tangentspy
 *  For use with Space Engineers Game
 *  Refer to github issues or steam/git dev guide/wiki or the team notes
 *  for direction what needs to be worked on next
*/

namespace Economy.scripts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Timers;
    using Economy.scripts.EconConfig;
    using Economy.scripts.EconStructures;
    using Economy.scripts.Management;
    using Economy.scripts.Messages;
    using Sandbox.Common;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class EconomyScript : MySessionComponentBase
    {
        #region constants

        const string PayPattern = @"(?<command>/pay)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))\s+(?<value>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<reason>.+)\s*$";
        const string BalPattern = @"(?<command>/bal)(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?";
        const string SeenPattern = @"(?<command>/seen)\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*))";
        const string ValuePattern = @"(?<command>/value)\s+(?:(?<Key>.+)\s+(?<Value>[+-]?((\d+(\.\d*)?)|(\.\d+)))|(?<Key>.+))";

        /// <summary>
        /// sell pattern check for "/sell" at[0], then number or string at [1], then string at [2], then number at [3], then string at [4]
        /// samples(optional): /sell all iron (price) (player/faction) || /sell 10 iron (price) (player/faction) || /sell accept || /sell deny || /sell cancel
        /// </summary>
        const string SellPattern = @"(?<command>/sell)\s+(?:(?<qty>[+-]?((\d+(\.\d*)?)|(\.\d+)))|(?<qtyall>ALL))\s+(?:(?:""(?<item>[^""]|.*?)"")|(?<item>.*(?=\s+\d+\b))|(?<item>.*$))(?:\s+(?<price>[+-]?((\d+(\.\d*)?)|(\.\d+)))(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?)?";

        //set is for admins to configure things like on hand, npc market pricing, toggle blacklist
        //Examples: eg /set 10000 ice    or /set blacklist ice   or /set buy item price

        const string SetPattern = @"(?<command>/set)(?:\s+(?:(?:""(?<market>[^""]|.*?)"")|(?<market>[^\s]*)))?\s+(?:(?<qty>[+-]?((\d+(\.\d*)?)|(\.\d+)))|(?<blacklist>blacklist)|(?<buy>buy)|(?<sell>sell))\s+(?:(?:""(?<item>[^""]|.*?)"")|(?<item>.*(?=\s+\d+\b))|(?<item>.*$))(?:\s+(?<price>[+-]?((\d+(\.\d*)?)|(\.\d+))))?";

        /// <summary>
        ///  buy pattern no "all" required.   reusing sell  
        /// /buy 10 "iron ingot" || /buy 10 "iron ingot" 1 || /buy 10 "iron ingot" 1 fred
        /// </summary>
        const string BuyPattern = @"(?<command>/buy)\s+(?<qty>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?:(?:""(?<item>[^""]|.*?)"")|(?<item>[^\s]*))(?:\s+(?<price>[+-]?((\d+(\.\d*)?)|(\.\d+)))(?:\s+(?:(?:""(?<user>[^""]|.*?)"")|(?<user>[^\s]*)))?)?";

        /// <summary>
        /// pattern defines how to create a new NPC Market.
        /// /npczone add|create {name} {x} {y} {z} {size} {shape}
        /// </summary>
        const string NpcZoneAddPattern = @"(?<command>/npczone)\s+((add)|(create))\s+(?:(?:""(?<name>[^""]|.*?)"")|(?<name>[^\s]*))\s+(?<X>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Y>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Z>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Size>(\d+(\.\d*)?))\s+(?<shape>(round)|(circle)|(sphere)|(spherical)|(box)|(cube)|(cubic))";

        /// <summary>
        /// pattern defines how to delete an existing NPC Market by name.
        /// /npczone del|delete|remove "{name}" or {name}
        /// </summary>
        const string NpcZoneDeletePattern = @"(?<command>/npczone)\s+((del)|(delete)|(remove))\s+(?:(?:""(?<name>[^""]|.*?)"")|(?<name>.*))";

        /// <summary>
        /// pattern defines how to rename an existing NPC Market name.
        /// /npczone ren|rename|name "{nameold}" or {nameold} "{namenew}" or {namenew}
        /// </summary>
        const string NpcZoneRenamePattern = @"(?<command>/npczone)\s+((ren)|(rename)|(name))\s+(?:(?:""(?<nameold>[^""]|.*?)"")|(?<nameold>.*))\s+(?:(?:""(?<namenew>[^""]|.*?)"")|(?<namenew>.*))";

        /// <summary>
        /// pattern defines how to move/resize/reshape an existing NPC Market.
        /// /npczone move|resize|reshape "{name}" or {name} {x} {y} {z} {size} {shape}
        /// </summary>
        const string NpcZoneMovePattern = @"(?<command>/npczone)\s+((move)|(resize)|(reshape))\s+(?:(?:""(?<name>[^""]|.*?)"")|(?<name>[^\s]*))\s+(?<X>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Y>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Z>[+-]?((\d+(\.\d*)?)|(\.\d+)))\s+(?<Size>(\d+(\.\d*)?))\s+(?<shape>(round)|(circle)|(sphere)|(spherical)|(box)|(cube)|(cubic))";

        /// <summary>
        /// pattern defines econfig commands.
        /// </summary>
        const string EconfigPattern = @"^(?<command>/econfig)(?:\s+(?<config>((language)|(TradeNetworkName)|(CurrencyName)|(LimitedRange)|(LimitedSupply)|(EnableLcds)|(EnableNpcTradezones)|(EnablePlayerTradezones)|(EnablePlayerPayments)|(TradeTimeout)|(AccountExpiry)|(StartingBalance)|(LicenceMin)|(LicenceMax)|(ReestablishRatio)|(MaximumPlayerZones)))(?:\s+(?<value>.+))?)?";

        /// <summary>
        /// pattern defines how to register a player trade zone.
        /// </summary>
        const string PlayerZoneAddPattern = @"(?<command>(/tz)|(/tradezone)|(/shop))\s+(register)\s+(?:(?:""(?<zonename>[^""]|.*?)"")|(?<zonename>[^\s]*))\s+(?<Size>(\d+(\.\d*)?))";

        /// <summary>
        /// pattern defines how to unregister or modify specific player trade zone.
        /// </summary>
        const string PlayerZoneModifyPattern = @"(?<command>(/tz)|(/tradezone)|(/shop))\s+(?<key>(unregister)|(open)|(close))\s+(?:(?:""(?<zonename>[^""]|.*?)"")|(?<zonename>[^\s]*))";

        /// <summary>
        /// pattern defines how to use paramterless player trade zone commands.
        /// </summary>
        const string PlayerZoneListPattern = @"(?<command>(/tz)|(/tradezone)|(/shop))\s+(?<key>(list))";

        /// <summary>
        /// pattern defines how to change an item in a player trade zone.
        /// </summary>
        const string PlayerZoneItemPattern = @"(?<command>/tz)\s+(?:(?<blacklist>blacklist)|(?<buy>buy)|(?<sell>sell))\s+(?:(?:""(?<item>[^""]|.*?)"")|(?<item>.*(?=\s+\d+\b))|(?<item>.*$))(?:\s+(?<price>[+-]?((\d+(\.\d*)?)|(\.\d+))))?";

        /// <summary>
        /// pattern defines how to retrieve a price list.
        /// </summary>
        const string PriceListPattern = @"(?<command>/pricelist)(?:(?:\s+""(?<zonename>[^""]|.*?)"")?)(?:\s+(?<key>[^\s]+)|\b)*";

        #endregion

        #region fields

        private bool _isInitialized;
        private bool _isClientRegistered;
        private bool _isServerRegistered;
        private bool _delayedConnectionRequest;
        private Timer _timer1Events; // 1 second.
        private Timer _timer10Events; // 10 seconds.
        private Timer _timer3600Events; // 1 hour.
        private bool _timer1Block;
        private bool _timer10Block;
        private bool _timer3600Block;

        private readonly Action<byte[]> _messageHandler = new Action<byte[]>(HandleMessage);

        public static EconomyScript Instance;

        public TextLogger ServerLogger = new TextLogger(); // This is a dummy logger until Init() is called.
        public TextLogger ClientLogger = new TextLogger(); // This is a dummy logger until Init() is called.
        public Timer DelayedConnectionRequestTimer;

        /// Ideally this data should be persistent until someone buys/sells/pays/joins but
        /// lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        public EconDataStruct Data;
        public EconConfigStruct ServerConfig;

        /// <summary>
        /// This will temporarily store Client side details while the client is connected.
        /// It will receive periodic updates from the server.
        /// </summary>
        public ClientConfig ClientConfig = null;

        /// <summary>
        /// Set manually to true for testing purposes. No need for this function in general.
        /// </summary>
        public bool DebugOn = false;

        public static CultureInfo ServerCulture;

        /// <summary>
        /// Manages the confirmation of players market registrations securely.
        /// </summary>
        public readonly Dictionary<long, MessageMarketManagePlayer> PlayerMarketRegister = new Dictionary<long, MessageMarketManagePlayer>();

        #endregion

        #region attaching events and wiring up

        public override void UpdateAfterSimulation()
        {
            Instance = this;

            // This needs to wait until the MyAPIGateway.Session.Player is created, as running on a Dedicated server can cause issues.
            // It would be nicer to just read a property that indicates this is a dedicated server, and simply return.
            if (!_isInitialized && MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
            {
                DebugOn = MyAPIGateway.Session.Player.IsExperimentalCreator();
                if (MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE)) // pretend single player instance is also server.
                    InitServer();
                if (!MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE) && MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated)
                    InitServer();
                InitClient();
            }

            // Dedicated Server.
            if (!_isInitialized && MyAPIGateway.Utilities != null && MyAPIGateway.Multiplayer != null
                && MyAPIGateway.Session != null && MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
            {
                InitServer();
                return;
            }

            if (_delayedConnectionRequest)
            {
                ClientLogger.WriteInfo("Delayed Connection Request");
                _delayedConnectionRequest = false;
                MessageConnectionRequest.SendMessage(EconomyConsts.ModCommunicationVersion);
            }

            base.UpdateAfterSimulation();
        }

        private void InitClient()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateAfterSimulation().
            _isClientRegistered = true;
            ClientLogger.Init("EconomyClient.Log", false, DebugOn ? 0 : 20); // comment this out if logging is not required for the Client.
            ClientLogger.WriteStart("Economy Client Log Started");
            ClientLogger.WriteInfo("Economy Client Version {0}", EconomyConsts.ModCommunicationVersion);
            if (ClientLogger.IsActive)
                VRage.Utils.MyLog.Default.WriteLine(String.Format("##Mod## Economy Client Logging File: {0}", ClientLogger.LogFile));

            MyAPIGateway.Utilities.MessageEntered += GotMessage;

            if (MyAPIGateway.Multiplayer.MultiplayerActive && !_isServerRegistered) // if not the server, also need to register the messagehandler.
            {
                ClientLogger.WriteStart("RegisterMessageHandler");
                MyAPIGateway.Multiplayer.RegisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);
            }

            DelayedConnectionRequestTimer = new Timer(10000);
            DelayedConnectionRequestTimer.Elapsed += DelayedConnectionRequestTimer_Elapsed;
            DelayedConnectionRequestTimer.Start();

            // let the server know we are ready for connections
            MessageConnectionRequest.SendMessage(EconomyConsts.ModCommunicationVersion);
        }

        private void InitServer()
        {
            _isInitialized = true; // Set this first to block any other calls from UpdateAfterSimulation().
            _isServerRegistered = true;
            ServerLogger.Init("EconomyServer.Log", false, DebugOn ? 0 : 20); // comment this out if logging is not required for the Server.
            ServerLogger.WriteStart("Economy Server Log Started");
            ServerLogger.WriteInfo("Economy Server Version {0}", EconomyConsts.ModCommunicationVersion);
            if (ServerLogger.IsActive)
                VRage.Utils.MyLog.Default.WriteLine(String.Format("##Mod## Economy Server Logging File: {0}", ServerLogger.LogFile));

            ServerLogger.WriteStart("RegisterMessageHandler");
            MyAPIGateway.Multiplayer.RegisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);

            ServerLogger.WriteStart("LoadBankContent");

            ServerConfig = EconDataManager.LoadConfig(); // Load config first.
            Data = EconDataManager.LoadData(ServerConfig.DefaultPrices);

            SetLanguage();

            // start the timer last, as all data should be loaded before this point.
            // TODO: use a single timer, and a counter.
            ServerLogger.WriteStart("Attaching Event 1 timer.");
            _timer1Events = new Timer(1000);
            _timer1Events.Elapsed += Timer1EventsOnElapsed;
            _timer1Events.Start();

            ServerLogger.WriteStart("Attaching Event 10 timer.");
            _timer10Events = new Timer(10000);
            _timer10Events.Elapsed += Timer10EventsOnElapsed;
            _timer10Events.Start();

            ServerLogger.WriteStart("Attaching Event 3600 timer.");
            _timer3600Events = new Timer(3600000);
            _timer3600Events.Elapsed += Timer3600EventsOnElapsed;
            _timer3600Events.Start();
        }

        #endregion

        #region detaching events

        protected override void UnloadData()
        {
            if (_isClientRegistered)
            {
                if (MyAPIGateway.Utilities != null)
                {
                    MyAPIGateway.Utilities.MessageEntered -= GotMessage;
                }

                if (!_isServerRegistered) // if not the server, also need to unregister the messagehandler.
                {
                    ClientLogger.WriteStop("UnregisterMessageHandler");
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);
                }

                if (DelayedConnectionRequestTimer != null)
                {
                    DelayedConnectionRequestTimer.Stop();
                    DelayedConnectionRequestTimer.Close();
                }

                ClientLogger.WriteStop("Log Closed");
                ClientLogger.Terminate();
            }

            if (_isServerRegistered)
            {
                ServerLogger.WriteStop("UnregisterMessageHandler");
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(EconomyConsts.ConnectionId, _messageHandler);

                if (_timer1Events != null)
                {
                    ServerLogger.WriteStop("Stopping Event 1 timer.");
                    _timer1Events.Stop();
                    _timer1Events.Elapsed -= Timer1EventsOnElapsed;
                    _timer1Events.Close();
                    _timer1Events = null;
                }

                if (_timer10Events != null)
                {
                    ServerLogger.WriteStop("Stopping Event 10 timer.");
                    _timer10Events.Stop();
                    _timer10Events.Elapsed -= Timer10EventsOnElapsed;
                    _timer10Events.Close();
                    _timer10Events = null;
                }

                if (_timer3600Events != null)
                {
                    ServerLogger.WriteStop("Stopping Event 3600 timer.");
                    _timer3600Events.Stop();
                    _timer3600Events.Elapsed -= Timer3600EventsOnElapsed;
                    _timer3600Events.Close();
                    _timer3600Events = null;
                }


                Data = null;

                ServerLogger.WriteStop("Log Closed");
                ServerLogger.Terminate();
            }

            base.UnloadData();
        }

        public override void SaveData()
        {
            base.SaveData();

            if (_isServerRegistered)
            {
                if (Data != null)
                {
                    ServerLogger.WriteInfo("Save Data");
                    EconDataManager.SaveData(Data);
                }

                if (ServerConfig != null)
                {
                    ServerLogger.WriteInfo("Save Config");
                    EconDataManager.SaveConfig(ServerConfig);
                }
            }
        }

        #endregion

        #region message handling

        private void GotMessage(string messageText, ref bool sendToOthers)
        {
            try
            {
                // here is where we nail the echo back on commands "return" also exits us from processMessage
                if (ProcessMessage(messageText)) { sendToOthers = false; }
            }
            catch (Exception ex)
            {
                ClientLogger.WriteException(ex);
                MyAPIGateway.Utilities.ShowMessage("Error", "An exception has been logged in the file: {0}", ClientLogger.LogFileName);
            }

            #region hud display
            // Update hud
            if (!UpdateHud()) { MyAPIGateway.Utilities.ShowMessage("Error", "Hud Failed"); }
        }

        public bool UpdateHud()
        {
            // client config has not been received from server yet.
            if (ClientConfig == null)
                return true;

            if (ClientConfig.ShowHud)
            {
                //Hud, displays users balance, trade network name, and optionally faction and free storage space (% or unit?) in cargo and/or inventory
                //may also eventually be used to display info about completed objectives in missions/jobs/bounties/employment etc
                //needs to call this at init (working), and at each call to message handling(working), and on recieving any notification of payment.
                //since other balance altering scenarios such as selling stock requires a command or prompt by player calling this
                //at message handler should update in those scenarios automatically. That should avoid need for a timing loop and have no obvious sim impact

                /* We need a routine populating a field similar the the below regularly, which pushes
                 * updates to the x y z location readout if it is enabled
                 * this will also be critical for checking against mission conditions
                 * once missions are functional, that require a user travelling to a location
                
                 * something like this can be used to populate the trade region part of the hud as well
                 * where we find ourself in a trade region
                 * it could probably hang off the timer code that updates lcds
                 var position = ((IMyEntity)character).WorldMatrix.Translation;
                 var markets = MarketManager.FindMarketsFromLocation(position);
                 */
                /* account.BankBalance.ToString("0.######"); */

                //use title here that frees up mission line for actual missions - cargo should list total and used space or just empty space?
                string readout = ClientConfig.TradeNetworkName + ": ";
                if (ClientConfig.ShowBalance) readout += string.Format("{0:#,##0.0000} {1}", ClientConfig.BankBalance, ClientConfig.CurrencyName);
                if (ClientConfig.ShowRegion) readout += " | Trade region: Unknown";
                if (ClientConfig.ShowXYZ)
                {
                    Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                    readout += " | " + string.Format("X: {0:F0} Y: {1:F0} Z: {2:F0}", position.X, position.Y, position.Z);
                }
                if (ClientConfig.ShowContractCount) readout += " | Contracts: 0";
                if (ClientConfig.ShowCargoSpace) readout += " | Cargo ? of ?";
                if (ClientConfig.ShowFaction)
                {
                    string faction = "Free agent";
                    IMyFaction plFaction;
                    //IMyPlayer Me = MyAPIGateway.Session.Player; why waste the bytes
                    plFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);
                    if (plFaction != null)
                    {
                        faction = plFaction.Name;  //should this show tag or full name? depends on screen size i suppose
                    }
                    readout += " | Agency: " + faction;

                    switch (ClientConfig.MissionId)
                    {
                        case 1:
                            ClientConfig.LazyMissionText = "Mine / Sell some ore";
                            //MissionPayment = 0;
                            //missionGPS = (x,y,z)
                            //create a client gps (caption, missionGPS);
                            //write this gps to some sort of list so we know 
                            //we need to remove it once we get there
                            break;
                        case 2:
                            ClientConfig.LazyMissionText = "Buy something";
                            //MissionPayment = 100;
                            break;
                        case 3:
                            ClientConfig.LazyMissionText = "Pay a player";
                            //MissionPayment = 600;
                            break;
                        case 4:
                            ClientConfig.LazyMissionText = "Check what a ship/station is worth";
                            //MissionPayment = 1000;
                            break;
                        case 5:
                            ClientConfig.LazyMissionText = "Build / Weld Something";
                            //MissionPayment = 10000;
                            break;
                        case 6:
                            ClientConfig.LazyMissionText = "Join a Faction";
                            //MissionPayment = 10000;
                            break;
                        default:
                            ClientConfig.LazyMissionText = ClientConfig.MissionId + " Mission: Survive | Deadline: Unlimited";
                            //"This would be an invalid or unknown mission id";
                            break;
                    }
                    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Clear();
                    MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = ClientConfig.LazyMissionText;
                    //MyAPIGateway.Utilities.GetObjectiveLine().Objectives.Add(reply);
                }
                MyAPIGateway.Utilities.GetObjectiveLine().Title = readout;
            }
            //MyAPIGateway.Utilities.GetObjectiveLine().Objectives[0] = readout;  //using title not mission text now


            return true;  //probably need a catch of some sort for a return false, but anything going wrong here is probably part of another issue.
            //and I am only using a bool to be lazy.  This should probably be HudManager.cs to make it public level 
        }
        #endregion hud display

        private static void HandleMessage(byte[] message)
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("HandleMessage");
            EconomyScript.Instance.ClientLogger.WriteVerbose("HandleMessage");
            ConnectionHelper.ProcessData(message);
        }
        #endregion message handling

        #region timers

        private void DelayedConnectionRequestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ClientConfig == null)
                _delayedConnectionRequest = true;
            else if (ClientConfig != null && DelayedConnectionRequestTimer != null)
            {
                DelayedConnectionRequestTimer.Stop();
                DelayedConnectionRequestTimer.Close();
            }
        }

        private void Timer1EventsOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // DO NOT SET ANY IN GAME API CALLS HERE. AT ALL!
            MyAPIGateway.Utilities.InvokeOnGameThread(delegate
            {
                // Recheck main Gateway properties, as the Game world my be currently shutting down when the InvokeOnGameThread is called.
                if (MyAPIGateway.Players == null || MyAPIGateway.Entities == null || MyAPIGateway.Session == null || MyAPIGateway.Utilities == null)
                    return;

                if (_timer1Block) // prevent other any additional calls into this code while it may still be running.
                    return;

                _timer1Block = true;
                try
                {
                    // Any processing needs to occur in here, as it will be on the main thread, and hopefully thread safe.
                    LcdManager.UpdateLcds();
                    //updates hud items that may change at times other than using chat commands
                    //if (ClientConfig.ShowXYZ || ClientConfig.ShowContractCount || ClientConfig.ShowCargoSpace || ClientConfig.ShowRegion || ClientConfig.ShowFaction) if (!UpdateHud()) { MyAPIGateway.Utilities.ShowMessage("Error", "Hud Failed"); }
                }
                finally
                {
                    _timer1Block = false;
                }
            });
        }

        private void Timer10EventsOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // DO NOT SET ANY IN GAME API CALLS HERE. AT ALL!
            MyAPIGateway.Utilities.InvokeOnGameThread(delegate
            {
                // Recheck main Gateway properties, as the Game world my be currently shutting down when the InvokeOnGameThread is called.
                if (MyAPIGateway.Players == null || MyAPIGateway.Entities == null || MyAPIGateway.Session == null || MyAPIGateway.Utilities == null)
                    return;

                if (_timer10Block) // prevent other any additional calls into this code while it may still be running.
                    return;

                _timer10Block = true;
                try
                {
                    // Any processing needs to occur in here, as it will be on the main thread, and hopefully thread safe.
                    MarketManager.CheckTradeTimeouts();
                }
                finally
                {
                    _timer10Block = false;
                }
            });
        }

        private void Timer3600EventsOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // DO NOT SET ANY IN GAME API CALLS HERE. AT ALL!
            MyAPIGateway.Utilities.InvokeOnGameThread(delegate
            {
                // Recheck main Gateway properties, as the Game world my be currently shutting down when the InvokeOnGameThread is called.
                if (MyAPIGateway.Players == null || MyAPIGateway.Entities == null || MyAPIGateway.Session == null || MyAPIGateway.Utilities == null)
                    return;

                if (_timer3600Block) // prevent other any additional calls into this code while it may still be running.
                    return;

                _timer3600Block = true;
                try
                {
                    // Any processing needs to occur in here, as it will be on the main thread, and hopefully thread safe.
                    // TODO: hourly market processing.
                }
                finally
                {
                    _timer3600Block = false;
                }
            });
        }

        #endregion

        #region command list
        private bool ProcessMessage(string messageText)
        {

            Match match; // used by the Regular Expression to test user input.
                         // this list is going to get messy since the help and commands themself tell user the same thing 
            string[] split = messageText.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // nothing useful was entered.
            if (split.Length == 0)
                return false;

            #region debug
            //used to test whatever crazy stuff im trying to work out
            if (split[0].Equals("/debug", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {

                //test throwing a connection to a foreign server from server ie in lobby worlds or we have moved worlds
                MyAPIGateway.Multiplayer.JoinServer("49.50.248.34:27045");
                

                //advancing mission display test
                //ClientConfig.MissionId++;  //yup that works nicely

                //showing my x y z position test //yup that works too
                //old way to test if on server or player dead (i think) probably doesnt work but will keep handy
                //if(MyAPIGateway.Session.Player.Controller == null || MyAPIGateway.Session.Player.Controller.ControlledEntity == null || MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity == null)
                //return true;

                //var position = player.GetPosition(); // actually lets skip the middle man and grab the entire thing
                //double, float position.X
                /* 
                Vector3D position = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetPosition();
                double X = position.X; double Y = position.Y; double Z = position.Z;
                string whereami = string.Format("[ X: {0:F0} Y: {1:F0} Z: {2:F0} ]",X, Y, Z);
                MyAPIGateway.Utilities.ShowMessage("debug", "You are here: {0}",whereami);
                */
                //ok over it
                return true;
            }
            #endregion debug

            #region tradezone
            // tradezone command
            // https://github.com/jpcsupplies/Economy_mod/issues/44
            // used to manage a players own trade regions
            // player must be within trade region to use these commands.
            // may also be necessary to allow them to specify a market name if mobile
            // markets are implemented
            /*Summary of suggested commands:
             * 4/tz register "name" range (create a market must target station block or ship block - charges a fee, prompts to confirm before billing)
             * 3/tz unregister "name" (deletes a market, moves all remaining stock to cargo space if it fits)
             * 3/tz move "name"  (moves market to currently targeted block, charges a fee - say $1000 or per metre
             * 3/tz close "name" (makes the market closed for business but doesnt delete it or stock)
             * 3/tz open "name" (makes the market available for trading again)
             * 3/tz factionmode on|off|coop (faction mode "on" all leaders of players faction can also control market, "off" only player can control their market, "coop"  treats factions as a coop or company with all members treated like employees (ie buy/sell @ cost)
             * 4-6/tz buyprice "item" price [optional buying qty limit] [optional trade restriction flag]
             * 4-6/tz sellprice "item" price [optional selling qty limit] [optional trade restriction flag]
             * 4-5/tz load|unload qty "item" player (restricted to faction or coop zones - unload  specified item qty into market from you or your ship - dont get paid, message faction owners about delivery, load - allows a shop owner to transfer stock to specified player at no cost. will integrate with mission system later
             * 5/tz restrict buy|sell "item" flag  (sets a restriction flag on buys or sells of this item)
             * 5/tz limit buy|sell "item" amount {sets a limit on the number of a given item to purchase or sell before halting trade in it}
             * 3/tz blacklist item (same as restrict in effect)
             * 
             *Restriction flags:
             * U= unrestricted trade with all)
             * s=trade with self only, same as Y blacklisted)
             * f=trade with own faction only)
             * A=trade with faction and allies only)
             * n= trade with neutral, allied or faction only)
             * 
             *Admin only commands
             *Suggested registration costs  (configurable by admin):
             *per metre radius registration fee 10 credits
             *sales tax rate 0.001% per metre radius (paid to npc pool)
             * /tz fee price
             * /tz tax percentage
             * /tz max maximum_radius_allowed_to_register
             */

            if (split[0].Equals("/tz", StringComparison.InvariantCultureIgnoreCase) || split[0].Equals("/tradezone", StringComparison.InvariantCultureIgnoreCase) || split[0].Equals("/shop", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, PlayerZoneAddPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string tradezoneName = match.Groups["zonename"].Value;
                    decimal size = Convert.ToDecimal(match.Groups["Size"].Value, CultureInfo.InvariantCulture);

                    var selectedBlock = Support.FindLookAtEntity(MyAPIGateway.Session.ControlledObject, false, true, false, false, false, false, false) as IMyCubeBlock;
                    if (selectedBlock == null || selectedBlock.BlockDefinition.TypeId != typeof (MyObjectBuilder_Beacon))
                    {
                        MyAPIGateway.Utilities.ShowMessage("TZ", "You need to target a beacon to register a trade zone.");
                        return true;
                    }

                    if (selectedBlock.GetPlayerRelationToOwner() != MyRelationsBetweenPlayerAndBlock.Owner)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TZ", "You must own the beacon to register it as trade zone.");
                        return true;
                    }

                    MessageMarketManagePlayer.SendRegisterMessage(selectedBlock.EntityId, tradezoneName, size);
                    return true;
                }

                match = Regex.Match(messageText, PlayerZoneModifyPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string keyName = match.Groups["key"].Value;
                    string tradezoneName = match.Groups["zonename"].Value;

                    // going to make the player look at the cube that they need to unregister, in case they have more than 1 in the vicinity.
                    if (keyName.Equals("unregister", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageMarketManagePlayer.SendUnregisterMessage(tradezoneName);
                        return true;
                    }

                    // the player can open or close the closest currently closed market.
                    if (keyName.Equals("open", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageMarketManagePlayer.SendOpenMessage(tradezoneName);
                        return true;
                    }

                    // the player can open or close the closest currently open market.
                    if (keyName.Equals("close", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageMarketManagePlayer.SendCloseMessage(tradezoneName);
                        return true;
                    }

                    MyAPIGateway.Utilities.ShowMessage("TZ", "CHECK2 Unexpected Keyname or Zone");
                }

                match = Regex.Match(messageText, PlayerZoneListPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string keyName = match.Groups["key"].Value;

                    if (keyName.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageMarketManagePlayer.SendListMessage();
                        return true;
                    }
                }

                match = Regex.Match(messageText, PlayerZoneItemPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // TODO: ###############################
                    // This code copied from the /set pattern, and hasn't been tested yet.

                    string itemName = match.Groups["item"].Value.Trim();
                    bool setbuy = match.Groups["buy"].Value.Equals("buy", StringComparison.InvariantCultureIgnoreCase);
                    bool setsell = match.Groups["sell"].Value.Equals("sell", StringComparison.InvariantCultureIgnoreCase);
                    bool blacklist = match.Groups["blacklist"].Value.Equals("blacklist", StringComparison.InvariantCultureIgnoreCase);
                    decimal amount = 0;
                    if (setbuy || setsell) // must be setting a price
                        if (!decimal.TryParse(match.Groups["price"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                            amount = 0;

                    // ok what item are we setting?
                    MyObjectBuilder_Base content;
                    Dictionary<string, MyDefinitionBase> options;
                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options))
                    {
                        if (options.Count == 0)
                            MyAPIGateway.Utilities.ShowMessage("TZ", "Item name not found.");
                        else if (options.Count > 10)
                            MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options.Keys) + " ?", null, "OK");
                        else
                            MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options.Keys) + " ?");
                        return true;
                    }

                    if (blacklist)
                    {
                        MessageMarketManagePlayer.SendBlacklistMessage(content.TypeId.ToString(), content.SubtypeName);
                    }
                    else if (setbuy)
                    {
                        MessageMarketManagePlayer.SendBuyPriceMessage(content.TypeId.ToString(), content.SubtypeName, amount);
                    }
                    else if (setsell)
                    {
                        MessageMarketManagePlayer.SendSellPriceMessage(content.TypeId.ToString(), content.SubtypeName, amount);
                    }

                    return true;


                    // TODO: ###############################
                }

                if (split.Length <= 1) { MyAPIGateway.Utilities.ShowMessage("TradeZone", "Nothing to do? Valid Options register, unregister, move, factionmode, buy/sell|blacklist/restrict/limit,"); }
                else
                { //everything else goes here - note this doesnt allow for spaces in names
                    //ill probably have to either get a regex from midspace or split by "" to extract names
                    //backup command evaluation matrix - not used replaced by regex logix - delete later.
                    /* if (split.Length == 3)
                    {
                        if (split[1].Equals("unregister", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("move", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("close", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("open", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("factionmode", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("blacklist", StringComparison.InvariantCultureIgnoreCase)) { }
                    }
                    if (split.Length == 5)
                    {
                        if (split[1].Equals("restrict", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("limit", StringComparison.InvariantCultureIgnoreCase)) { }

                    }
                    if (split.Length >= 4 && split.Length <= 5)
                    {
                        if (split[1].Equals("load", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("unload", StringComparison.InvariantCultureIgnoreCase)) { }
                    }
                    if (split.Length >= 4 && split.Length <= 6)
                    {
                        if (split[1].Equals("buyprice", StringComparison.InvariantCultureIgnoreCase)) { }
                        if (split[1].Equals("sellprice", StringComparison.InvariantCultureIgnoreCase)) { }
                    } */
                }  
                //something slipped through the cracks lets get out of here before something odd happens.
                return true;
            }
            #endregion tradezone

            #region pay
            // pay command
            // eg /pay bob 50 here is your payment
            // eg /pay "Screaming Angles" 10 fish and chips
            if (split[0].Equals("/pay", StringComparison.InvariantCultureIgnoreCase))
            {   //might need to add a check here preventing normal players paying NPC but not critical since they would be silly to try
                match = Regex.Match(messageText, PayPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessagePayUser.SendMessage(match.Groups["user"].Value,
                        Convert.ToDecimal(match.Groups["value"].Value, CultureInfo.InvariantCulture),
                        match.Groups["reason"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("PAY", "Missing parameter - /pay user amount reason");
                return true;
            }
            #endregion pay

            #region buy
            // buy command
            if (split[0].Equals("/buy", StringComparison.InvariantCultureIgnoreCase))
            {
                //buy command should be pretty basic since we are buying from appropriate market at requested price.
                //if in range of valid trade region
                //examples: /buy 100 uranium (buy 100 uranium starting at lowest offer price if you can afford it, give error if you cant)
                // /buy 100 uranium 0.5 (buy up to 100 uranium if the price is below 0.5, post a buy offer for any remaining amount if price goes over)
                // /buy 100 uranium 0.5 bob (ask bob to sell you 100 uranium at 0.5, or if bob already has a sell offer at or below 0.5, buy only from him)
                // /buy cancel ??? (possible command to close an open buy offer?)
                //
                //  if (no price specified) buy whatever the lowest offer is, accumulating qty, and adding up each price point qty until
                //  sale is concluded at desired qty, or player runs out of money, or there are none left to buy.
                //  if (they place an offer above/below market && have access to offers), post offer, and deduct funds
                //  if they choose to cancel an offer, remove offer and return remaining money

                match = Regex.Match(messageText, BuyPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    decimal buyPrice;
                    bool useMarketSellPrice = false;
                    bool buyFromMerchant = false;
                    bool findOnMarket = false;

                    string itemName = match.Groups["item"].Value;
                    string sellerName = match.Groups["user"].Value;
                    decimal buyQuantity = Convert.ToDecimal(match.Groups["qty"].Value, CultureInfo.InvariantCulture);
                    if (!decimal.TryParse(match.Groups["price"].Value, out buyPrice))
                        // We will use the they price they set at which they will buy from the player.
                        useMarketSellPrice = true; // buyprice will be 0 because TryParse failed.

                    if (string.IsNullOrEmpty(sellerName))
                        buyFromMerchant = true;

                    if (!useMarketSellPrice && buyFromMerchant)
                    {
                        // A price was specified, we actually intend to offer the item for sale at a set price to the market, not the bank.
                        findOnMarket = true;
                        buyFromMerchant = false;
                    }

                    MyObjectBuilder_Base content;
                    Dictionary<string, MyDefinitionBase> options;
                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options))
                    {
                        if (options.Count == 0)
                            MyAPIGateway.Utilities.ShowMessage("BUY", "Item name not found.");
                        else if (options.Count > 10)
                            MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options.Keys) + " ?", null, "OK");
                        else
                            MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options.Keys) + " ?");
                        return true;
                    }

                    MessageBuy.SendMessage(sellerName, buyQuantity, content.TypeId.ToString(), content.SubtypeName, buyPrice, useMarketSellPrice, buyFromMerchant, findOnMarket);
                    return true;
                }

                switch (split.Length)
                {
                    case 1: //ie /buy
                        MyAPIGateway.Utilities.ShowMessage("BUY", "/buy #1 #2 #3 #4");
                        MyAPIGateway.Utilities.ShowMessage("BUY", "#1 is quantity, #2 is item, #3 optional price to offer #4 optional where to buy from");
                        return true;
                }

                MyAPIGateway.Utilities.ShowMessage("BUY", "Nothing nearby to trade with?");
                return true;
            }
            #endregion buy

            #region econfig

            match = Regex.Match(messageText, EconfigPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                MessageConfig.SendMessage(match.Groups["config"].Value, match.Groups["value"].Value);
                return true;
            }

            //Todo https://github.com/jpcsupplies/Economy_mod/issues/88
            // Set options like limited or unlimited stock, range checks size (or on or off), currency name
            // and trading on or off.   Or any other options an admin should be able to set in game.

            // /econfig - Display current settings.
            // /econfig language 4 - change language to 4.
            // /econfig language en - change language to 0 (after some detection).
            // /econfig TradeNetworkName Federated Terran Empire - change the TradeNetworkName
            // /econfig LimitedRange off - change LimitedRange off.
            // /econfig TradeTimeout 00:00:01 - change TradeTimeout to 1 second.
            // /econfig DefaultStartingBalance 5000 - change DefaultStartingBalance to 5000.
            // /econfig TradeZoneLicence 20000 - change TradeZoneLicence cost to 20000.
            // /econfig EnableNpcTradezones on - change Npc Trade zones on.
            // /econfig EnablePlayerTradezones on - change Player created Trade zones on.
            // /econfig EnablePlayerPayments on - change Player payments on.

            #endregion econfig

            #region set
            //Item managment -
            // allows an admin to maintain on hand, blacklist, buy and sell prices of npc market
            // The regex needs fine tuning but should basically work
            //Usage: /set <qty> "item name"   (to set the on hand stock of an item)
            //       /set blacklist "item name" (to toggle blacklist on or off (depending on current state))
            //       /set buy "item name"  <price>
            //       /set sell "item name" <price>
            //       /set * sell "item name" <price>
            //       /set "market zone" sell "item name" <price>
            // we need a /check command maybe to display current blacklist status, price and stock too

            if (split[0].Equals("/set", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                match = Regex.Match(messageText, SetPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string marketZone = match.Groups["market"].Value.Trim();
                    string itemName = match.Groups["item"].Value.Trim();
                    bool setbuy = match.Groups["buy"].Value.Equals("buy", StringComparison.InvariantCultureIgnoreCase);
                    bool setsell = match.Groups["sell"].Value.Equals("sell", StringComparison.InvariantCultureIgnoreCase);
                    bool blacklist = match.Groups["blacklist"].Value.Equals("blacklist", StringComparison.InvariantCultureIgnoreCase);
                    decimal amount = 0;
                    if (!blacklist && !setbuy && !setsell) // must be setting on hand
                        if (!decimal.TryParse(match.Groups["qty"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                            amount = 0;

                    if (setbuy || setsell) // must be setting a price
                        if (!decimal.TryParse(match.Groups["price"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                            amount = 0;

                    //ok what item are we setting?
                    MyObjectBuilder_Base content;
                    Dictionary<string, MyDefinitionBase> options;
                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options))
                    {
                        if (options.Count == 0)
                            MyAPIGateway.Utilities.ShowMessage("SET", "Item name not found.");
                        else if (options.Count > 10)
                            MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options.Keys) + " ?", null, "OK");
                        else
                            MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options.Keys) + " ?");
                        return true;
                    }

                    // TODO: do range checks for the market, using MarketManager.FindMarketsFromLocation()
                    //SendMessage(ulong marketId, string itemTypeId, string itemSubTypeName, SetMarketItemType setType,
                    // decimal itemQuantity, decimal itemBuyPrice, decimal itemSellPrice, bool blackListed)
                    if (blacklist) //we want to black list
                    {
                        MessageSet.SendMessage(EconomyConsts.NpcMerchantId, marketZone, content.TypeId.ToString(), content.SubtypeName, SetMarketItemType.Blacklisted, 0, 0, 0);
                    }
                    else if (setbuy) // do we want to set buy price..?
                    {
                        MessageSet.SendMessageBuy(EconomyConsts.NpcMerchantId, marketZone, content.TypeId.ToString(), content.SubtypeName, amount);
                    }
                    else if (setsell) //or do we want to set sell price..?
                    {
                        MessageSet.SendMessageSell(EconomyConsts.NpcMerchantId, marketZone, content.TypeId.ToString(), content.SubtypeName, amount);
                    }
                    else //no we must want to set on hand?
                    {
                        MessageSet.SendMessageQuantity(EconomyConsts.NpcMerchantId, marketZone, content.TypeId.ToString(), content.SubtypeName, amount);
                    }
                    //whatever we did we are done now
                    return true;
                }

                MyAPIGateway.Utilities.ShowMessage("SET", "/set #1 #2 #3");
                //MyAPIGateway.Utilities.ShowMessage("SET", "#1 is quantity, #2 is item");
                MyAPIGateway.Utilities.ShowMessage("SET", "#1 is quantity or blacklist (or buy or sell), #2 is item, (#3 is price)");
                MyAPIGateway.Utilities.ShowMessage("SET", "eg /set 1 rifle, /set blacklist rifle, /set buy rifle 1000, /set sell rifle 2000");
                return true;
            }
            #endregion set

            #region sell
            // sell command
            if (split[0].Equals("/sell", StringComparison.InvariantCultureIgnoreCase))
            {


                //now we need to check if range is ignored, or perform a range check on [4] to see if it is in selling range
                //if both checks fail tell player nothing is near enough to trade with
                decimal sellQuantity = 0;
                bool sellAll;
                string itemName = "";
                decimal sellPrice = 1;
                bool useMarketBuyPrice = false;
                string buyerName = "";
                bool sellToMerchant = false;
                bool offerToMarket = false;

                match = Regex.Match(messageText, SellPattern, RegexOptions.IgnoreCase);

                 //ok we need to catch the target (player/faction) at [4] or set it to NPC if its null
                //then populate the other fields.  
                //string reply = "match " + match.Groups["qty"].Value + match.Groups["item"].Value + match.Groups["user"].Value + match.Groups["price"].Value;
                if (match.Success)
                {
                    itemName = match.Groups["item"].Value.Trim();
                    buyerName = match.Groups["user"].Value;
                    sellAll = match.Groups["qtyall"].Value.Equals("all", StringComparison.InvariantCultureIgnoreCase);
                    if (!sellAll)
                        sellQuantity = Convert.ToDecimal(match.Groups["qty"].Value, CultureInfo.InvariantCulture);
                    if (!decimal.TryParse(match.Groups["price"].Value, out sellPrice))
                        // We will use the they price they set at which they will buy from the player.
                        useMarketBuyPrice = true;  // sellprice will be 0 because TryParse failed.

                    if (string.IsNullOrEmpty(buyerName))
                        sellToMerchant = true;

                    if (!useMarketBuyPrice && sellToMerchant)
                    {
                        // A price was specified, we actually intend to offer the item for sale at a set price to the market, not the bank.
                        offerToMarket = true;
                        sellToMerchant = false;
                    }

                    MyObjectBuilder_Base content;
                    Dictionary<string, MyDefinitionBase> options;
                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options))
                    {
                        // TODO: filter according to items in player inventory.
                        //options = options.Where(e =>  check inventory ..(e.Value.Id.TypeId.ToString(), e.Value.Id.SubtypeName)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        if (options.Count == 0)
                            MyAPIGateway.Utilities.ShowMessage("SELL", "Item name not found.");
                        else if (options.Count > 10)
                            MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options.Keys) + " ?", null, "OK");
                        else
                            MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options.Keys) + " ?");
                        return true;
                    }

                    if (sellAll)
                    {
                        var character = MyAPIGateway.Session.Player.GetCharacter();
                        // TODO: may have to recheck that character is not null.
                        var inventory = character.GetPlayerInventory();
                        sellQuantity = (decimal)inventory.GetItemAmount(content.GetId());

                        if (sellQuantity == 0)
                        {
                            MyAPIGateway.Utilities.ShowMessage("SELL", "You don't have any '{0}' to sell.", content.GetDisplayName());
                            return true;
                        }
                    }

                    // TODO: add items into holding as part of the sell message, from container Id: inventory.Owner.EntityId.
                    MessageSell.SendSellMessage(buyerName, sellQuantity, content.TypeId.ToString(), content.SubtypeName, sellPrice, useMarketBuyPrice, sellToMerchant, offerToMarket);
                    return true;
                }


                switch (split.Length)
                {
                    // everything below here is not used but kept for reference for later functions
                    case 2: //ie /sell all or /sell cancel or /sell accept or /sell deny
                        if (split[1].Equals("cancel", StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageSell.SendCancelMessage();
                            return true;
                        }
                        if (split[1].Equals("accept", StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageSell.SendAcceptMessage();
                            return true;
                        }
                        if (split[1].Equals("deny", StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageSell.SendDenyMessage();
                            return true;
                        }
                        if (split[1].Equals("collect", StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageSell.SendCollectMessage();
                            return true;
                        }
                        //return false;
                        break;
                        //default: //must be more than 2 and invalid

                        // Accessing EconomyScript.Instance.Config.NpcMerchantName here will cause an exception, as Config is only loaded on the Server, not Clients.
                        // Need to make sure core functionality is run on the Server.

                        /*                        if (buyerName != EconomyScript.Instance.Config.NpcMerchantName && buyerName != "OFFER")
                                                {
                                                    //must be selling to a player (or faction ?)
                                                    //check the item to sell is a valid product, do usual qty type checks etc
                                                    //check the player / faction exists etc etc
                                                    MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We are selling to a player send request prompt or if it is a faction check they are trading this item");
                                                }
                                                else
                                                {
                                                    if (buyerName == EconomyScript.Instance.Config.NpcMerchantName) { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We must be selling to NPC - skip prompts sell immediately at price book price"); }
                                                    if (buyerName == "OFFER") { MyAPIGateway.Utilities.ShowMessage("SELL", "Debug: We must be posting a sell offer to stockmarket - skip prompts post offer UNLESS we match a buy offer at the same price then process that"); }
                                                }*/
                        //return false; 
                }

                MyAPIGateway.Utilities.ShowMessage("SELL", "/sell #1 #2 #3 #4");
                MyAPIGateway.Utilities.ShowMessage("SELL", "#1 is quantity, #2 is item, #3 optional price to offer #4 optional where to sell to");
                return true;
            }
            #endregion sell

            #region seen
            // seen command
            if (split[0].Equals("/seen", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, SeenPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessagePlayerSeen.SendMessage(match.Groups["user"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("SEEN", "Who are we looking for?");
                return true;
            }
            #endregion seen

            #region hud
            //command to allow players to customise their hud?             
            if (split[0].Equals("/hud", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length == 1 || split.Length >= 4 || (split.Length == 2 && split[1].Equals("help", StringComparison.InvariantCultureIgnoreCase)))
                {
                    MyAPIGateway.Utilities.ShowMessage("HUD", "Controls various aspects of hud display. See '/ehelp hud' for more details.");
                }
                if (split.Length == 2)
                {
                    if (split[1].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowHud = false; MyAPIGateway.Utilities.GetObjectiveLine().Hide(); }
                    if (split[1].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowHud = true; MyAPIGateway.Utilities.GetObjectiveLine().Show(); }
                }
                if (split.Length == 3)
                {
                    if (split[1].Equals("balance", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowBalance = true; }
                    if (split[1].Equals("balance", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowBalance = false; }
                    if (split[1].Equals("region", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowRegion = true; }
                    if (split[1].Equals("region", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowRegion = false; }
                    if (split[1].Equals("GPS", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowXYZ = true; }
                    if (split[1].Equals("GPS", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowXYZ = false; }
                    if (split[1].Equals("contracts", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowContractCount = true; }
                    if (split[1].Equals("contracts", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowContractCount = false; }
                    if (split[1].Equals("cargo", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowCargoSpace = true; }
                    if (split[1].Equals("cargo", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowCargoSpace = false; }
                    if (split[1].Equals("agency", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("on", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowFaction = true; }
                    if (split[1].Equals("agency", StringComparison.InvariantCultureIgnoreCase) && split[2].Equals("off", StringComparison.InvariantCultureIgnoreCase)) { ClientConfig.ShowFaction = false; }
                }
                return true;
            }

            #endregion hud

            #region bal
            // bal command
            if (split[0].Equals("/bal", StringComparison.InvariantCultureIgnoreCase))
            {
                //pull current mission text
                if (MyAPIGateway.Utilities.GetObjectiveLine().CurrentObjective == "Type /bal to connect to network") if (!UpdateHud()) { MyAPIGateway.Utilities.ShowMessage("Error", "Hud Failed"); };
                match = Regex.Match(messageText, BalPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    MessageBankBalance.SendMessage(match.Groups["user"].Value);
                else
                    MyAPIGateway.Utilities.ShowMessage("BAL", "Incorrect parameters");
                return true;
            }
            #endregion bal

            #region value
            // value command for looking up the table price of an item.
            // eg /value itemname optionalqty

            if (split[0].Equals("/value", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, ValuePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var itemName = match.Groups["Key"].Value;
                    var strAmount = match.Groups["Value"].Value;
                    MyObjectBuilder_Base content;
                    Dictionary<string, MyDefinitionBase> options;

                    // Search for the item and find one match only, either by exact name or partial name.
                    if (!Support.FindPhysicalParts(itemName, out content, out options))
                    {
                        if (options.Count == 0)
                            MyAPIGateway.Utilities.ShowMessage("VALUE", "Item name not found.");
                        else if (options.Count > 10)
                            MyAPIGateway.Utilities.ShowMissionScreen("Item not found", itemName, " ", "Did you mean:\r\n" + String.Join(", ", options.Keys) + " ?", null, "OK");
                        else
                            MyAPIGateway.Utilities.ShowMessage("Item not found. Did you mean", String.Join(", ", options.Keys) + " ?");
                        return true;
                    }
                    if (content != null)
                    {
                        decimal amount;
                        if (!decimal.TryParse(strAmount, out amount))
                            amount = 1; // if it cannot parse it, assume it is 1. It may not have been specified.

                        if (amount < 0) // if a negative value is provided, make it 1.
                            amount = 1;

                        if (content.TypeId != typeof(MyObjectBuilder_Ore) && content.TypeId != typeof(MyObjectBuilder_Ingot))
                        {
                            // must be whole numbers.
                            amount = Math.Round(amount, 0);
                        }

                        // Primary checks for the component are carried out Client side to reduce processing time on the server. not that 2ms matters but if 
                        // there is thousands of these requests at once one day in "space engineers the MMO" or on some auto-trading bot it might become a problem
                        MessageMarketItemValue.SendMessage(content.TypeId.ToString(), content.SubtypeName, amount, content.GetDisplayName());
                        return true;
                    }

                    MyAPIGateway.Utilities.ShowMessage("VALUE", "Unknown Item. Could not find the specified name.");
                }
                else
                    MyAPIGateway.Utilities.ShowMessage("VALUE", "You need to specify something to value eg /value ice");
                return true;
            }
            #endregion value

            #region pricelist

            if (split[0].Equals("/pricelist", StringComparison.InvariantCultureIgnoreCase))
            {
                match = Regex.Match(messageText, PriceListPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // if the first parameter is surrounded by quotes, search for a market with that name.
                    // it could also just search on any string in first paramater for market
                    // but if a market is named ore or ingot etc this may cause problems

                    bool showOre = false;
                    bool showIngot = false;
                    bool showComponent = false;
                    bool showAmmo = false;
                    bool showTools = false;
                    bool showGasses = false;
                    string findZoneName = match.Groups["zonename"].Value;

                    for (var i = 0; i < match.Groups["key"].Captures.Count; i++)
                    {
                        string str = match.Groups["key"].Captures[i].Value;
                        if (str.StartsWith("ore", StringComparison.InvariantCultureIgnoreCase))
                            showOre = true;
                        if (str.StartsWith("ingot", StringComparison.InvariantCultureIgnoreCase))
                            showIngot = true;
                        if (str.StartsWith("component", StringComparison.InvariantCultureIgnoreCase))
                            showComponent = true;
                        if (str.StartsWith("ammo", StringComparison.InvariantCultureIgnoreCase))
                            showAmmo = true;
                        if (str.StartsWith("tool", StringComparison.InvariantCultureIgnoreCase))
                            showTools = true;
                        if (str.StartsWith("gas", StringComparison.InvariantCultureIgnoreCase))
                            showGasses = true;
                    }

                    MessageMarketPriceList.SendMessage(showOre, showIngot, showComponent, showAmmo, showTools, showGasses, findZoneName);
                    return true;
                }
            }

            #endregion

            #region worth
            // worth command
            if (split[0].Equals("/worth", StringComparison.InvariantCultureIgnoreCase))
            {
                var selectedShip = Support.FindLookAtEntity(MyAPIGateway.Session.ControlledObject, true, false, false, false, false, false, false) as IMyCubeGrid;
                if (selectedShip != null)
                {
                    MessageWorth.SendMessage(selectedShip.EntityId);
                    return true;
                }
                MyAPIGateway.Utilities.ShowMessage("WORTH", "You need to target a ship or station to value.");
                return true;
            }
            #endregion

            #region npc trade zones

            // npc command.  For Admins only.
            if (split[0].Equals("/npczone", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                // Example: /npczone list
                //          /npczone add/create <name> <x> <y> <z> <size> <shape>
                //          /npczone delete/remove <name>

                match = Regex.Match(messageText, NpcZoneAddPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string marketName = match.Groups["name"].Value;
                    decimal x = Convert.ToDecimal(match.Groups["X"].Value, CultureInfo.InvariantCulture);
                    decimal y = Convert.ToDecimal(match.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    decimal z = Convert.ToDecimal(match.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    decimal size = Convert.ToDecimal(match.Groups["Size"].Value, CultureInfo.InvariantCulture);
                    string shapeName = match.Groups["shape"].Value;

                    MarketZoneType shape = MarketZoneType.FixedSphere;
                    switch (shapeName)
                    {
                        case "sphere":
                        case "spherical":
                        case "round":
                        case "circle":
                            shape = MarketZoneType.FixedSphere;
                            break;
                        case "box":
                        case "cube":
                        case "cubic":
                            shape = MarketZoneType.FixedBox;
                            break;
                    }

                    MessageMarketManageNpc.SendAddMessage(marketName, x, y, z, size, shape);
                    return true;
                }

                match = Regex.Match(messageText, NpcZoneDeletePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string marketName = match.Groups["name"].Value;
                    MessageMarketManageNpc.SendDeleteMessage(marketName);
                    return true;
                }

                match = Regex.Match(messageText, NpcZoneRenamePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    MessageMarketManageNpc.SendRenameMessage(match.Groups["nameold"].Value, match.Groups["namenew"].Value);
                    return true;
                }

                match = Regex.Match(messageText, NpcZoneMovePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string marketName = match.Groups["name"].Value;
                    decimal x = Convert.ToDecimal(match.Groups["X"].Value, CultureInfo.InvariantCulture);
                    decimal y = Convert.ToDecimal(match.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    decimal z = Convert.ToDecimal(match.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    decimal size = Convert.ToDecimal(match.Groups["Size"].Value, CultureInfo.InvariantCulture);
                    string shapeName = match.Groups["shape"].Value;

                    MarketZoneType shape = MarketZoneType.FixedSphere;
                    switch (shapeName)
                    {
                        case "sphere":
                        case "spherical":
                        case "round":
                        case "circle":
                            shape = MarketZoneType.FixedSphere;
                            break;
                        case "box":
                        case "cube":
                        case "cubic":
                            shape = MarketZoneType.FixedBox;
                            break;
                    }

                    MessageMarketManageNpc.SendMoveMessage(marketName, x, y, z, size, shape);
                    return true;
                }

                if (split.Length > 1 && split[1].Equals("list", StringComparison.InvariantCultureIgnoreCase))
                {
                    MessageMarketManageNpc.SendListMessage();
                    return true;
                }

                // TODO: report back bad command.
                MyAPIGateway.Utilities.ShowMessage("/npczone list/[add]/remove zone [x y z radius shape]", "Manages your NPC trading zone portal locations.");
                return true;
            }

            #endregion

            #region accounts
            // accounts command.  For Admins only.
            if (split[0].Equals("/accounts", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                //add a day/week/month filter to accounts   OR just sort accounts list by date? OR accept a integer for number of days to filter by?
                if (split.Length <= 1) { MessageListAccounts.SendMessage(); }
                else {
                    switch (split[1].ToLowerInvariant())
                    {
                        case "date":
                            //sort accounts by date - (by default sort by name?)
                            return true;
                        case "today":
                            //show players seen today
                            return true;
                        case "day":
                            //show players seen today
                            return true;
                        case "week":
                            //show players seen last 7 days
                            return true;
                        case "month":
                            //show players seen last 30 days
                            return true;
                    }
                }
                return true;
                // don't respond to non-admins.
            }
            #endregion accounts

            #region reset
            // reset command.  For Admins only.
            if (split[0].Equals("/reset", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {
                MessageResetAccount.SendMessage();
                return true;
                // don't respond to non-admins.
            }
            #endregion reset

            #region ver
            //ver reply
            if (split[0].Equals("/ver", StringComparison.InvariantCultureIgnoreCase))
            {
                string versionreply = EconomyConsts.MajorVer + " " + EconomyConsts.ModCommunicationVersion;
                MyAPIGateway.Utilities.ShowMessage("VER", versionreply);
                return true;
            }
            #endregion ver

            #region news system
            //news reply - early proof of concept work https://github.com/jpcsupplies/Economy_mod/issues/70
            //this should fire a sub with the parameter as the message to be displayed, that way if we get other things like bounties or
            //navigation hazard warnings and random frontier news working, we can just throw the generated news report at 
            //the global news sub to make it display
            //clearly this needs to run server side and trigger on all online players screens - 
            //this test will only display on the current admins screen however
            //MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "Warning", "This is only a placeholder mod it is not functional yet!", null, "Close");
            if (split[0].Equals("/global", StringComparison.InvariantCultureIgnoreCase) && MyAPIGateway.Session.Player.IsAdmin())
            {

                string announce = "Date Now " + messageText;
                //messageTexts needs the "/global" removed, and add a missionbox newline on each \n  then display the formatted
                //message in a mission box. Probably need a way to add new lines from chat eg line1^Line2^Line3
                MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "News:", announce, null, "Close");
                return true;
            }

            //this is the normal players command to bring up the news log (newest to oldest) and/or last major news message
            if (split[0].Equals("/news", StringComparison.InvariantCultureIgnoreCase))
            {

                string announce = "No news is good news?";
                //messageTexts Displays the last 10 or so /global messages with time stamp or recent events (such as attacks, new markets being registered etc)
                MyAPIGateway.Utilities.ShowMissionScreen("Economy", "", "News:", announce, null, "Close");
                return true;
            }
            #endregion news system

            #region help
            // help command
            if (split[0].Equals("/ehelp", StringComparison.InvariantCultureIgnoreCase))
            {
                if (split.Length <= 1)
                {
                    //did we just type ehelp? show what else they can get help on
                    //might be better to make a more detailed help reply here using mission window later on
                    MyAPIGateway.Utilities.ShowMessage("ehelp", "Commands: ehelp, bal, pay, seen, buy, sell, value, ver, lcd, news, worth, pricelist");
                    if (MyAPIGateway.Session.Player.IsAdmin())
                    {
                        MyAPIGateway.Utilities.ShowMessage("Admin ehelp", "Commands: accounts, bal player, reset, set, global, pay player +/-any_amount");
                    }
                    MyAPIGateway.Utilities.ShowMessage("ehelp", "Try '/ehelp command' for more informations about specific command");
                    return true;
                }
                else
                {
                    string helpreply = "."; //this reply is too big need to move it to pop up \r\n
                    switch (split[1].ToLowerInvariant())
                    {
                        // did we type /ehelp help ?
                        case "help":
                            MyAPIGateway.Utilities.ShowMessage("/ehelp #", "Displays help on the specified command [#].");
                            return true;
                        // did we type /help buy etc
                        case "pay":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/pay X Y Z Pays player [x] amount [Y] [for reason Z]");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /pay bob 100 being awesome");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "for larger player names used quotes eg \"bob the builder\"");
                            if (MyAPIGateway.Session.Player.IsAdmin())
                            {
                                MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "Admins can add or remove any amount from a player, including negative");
                            }
                            return true;
                        case "worth":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/worth  Tells you the value of the target object in relation to how much the components used to create it are worth.");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Useful if you want to sell a ship or station and need a price. Example: /worth");
                            return true;
                        case "seen":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/seen X Displays time and date that economy plugin last saw player X");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /seen bob");
                            return true;
                        case "global":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "/global displays a mission window message to all online players"); return true; }
                            else { return false; }
                        case "accounts":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "/accounts displays all player balances"); return true; }
                            else { return false; }
                        case "reset":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "/reset resets your balance to 100"); return true; }
                            else { return false; }
                        case "set":
                            if (MyAPIGateway.Session.Player.IsAdmin()) { MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "/set # X  Sets the on hand amount of item (X) to amount  (#) in the NPC market. Eventually will set other settings too."); return true; }
                            else { return false; }
                        case "bal":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/bal Displays your bank balance");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /bal");
                            if (MyAPIGateway.Session.Player.IsAdmin())
                            {
                                MyAPIGateway.Utilities.ShowMessage("Admin eHelp", "Admins can also view another player. eg. /bal bob");
                            }
                            return true;
                        case "buy":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/buy W X Y Z - Purchases a quantity [W] of item [X] [at price Y] [from player Z]");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /buy 20 Ice ");
                            return true;
                        case "lcd":
                            helpreply = "Controls various LCD Information Feeds\r\n" +
                                "Prerequisites: An LCD. \r\n" +
                                "Firstly name the LCD you want to use [Economy]\r\n" +
                                "Edit the public title and insert one or more of the following keywords -\r\n" +
                                "component  (displays all component buy/sell prices)\r\n" +
                                "ore        (displays all ore buy/sell prices)\r\n" +
                                "ingot      (displays all ingot buy/sell prices)\r\n" +
                                "Tools      (displays all tool buy/sell prices)\r\n" +
                                "ammo       (displays all ammunition buy/sell prices)\r\n" +
                                "gas etc may also be supported in future\r\n" +
                                "stock      (shows amount of stock on hand of above)\r\n";
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "keywords component, ore, ingot, tools, ammo, stock");
                            MyAPIGateway.Utilities.ShowMissionScreen("Economy Help", "", "LCD Usage", helpreply, null, "Cool");
                            return true;
                        case "sell":
                            helpreply = "/sell W X Y Z \r\n Sells quantity [W] of item [X] [at price Y] [to player Z]\r\n" +
                                " Eg sell to another player /sell 1 rifle 10 Bob\r\n" +
                                " or Sell to NPC (or nearest market) Eg: /sell 20 Ice \r\n" +
                                "/sell cancel (Cancels any sell offers you did)\r\n" +
                                "/sell collect (collects items from failed sell offers)\r\n" +
                                "/sell acccept|deny (accepts or rejects a sell offer made to you)\r\n";
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /sell 20 Ice [optional price] [optional player]");
                            MyAPIGateway.Utilities.ShowMissionScreen("Economy Help", "", "sell command", helpreply, null, "Close");
                            return true;
                        case "hud":
                            helpreply = "Turns on or off various HUD readouts\r\n" +
                                " Eg /hud agency on  or /hud off\r\n" +
                                " /hud on|off (Turn entire hud on or off)\r\n" +
                                " /hud balance on|off (Bank balance display on or off)\r\n" +
                                " /hud region on|off (Name of current trader region on or off\r\n" +
                                " /hud GPS on|off (Show Galactic Positioning System XYZ coords\r\n" +
                                " /hud contracts on|off (Number of active jobs/subsidies)\r\n" +
                                " /hud cargo on|off (Display available cargo space for trading)\r\n" +
                                " /hud agency on|off (Display your current agency/faction)\r\n";
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/hud #|balance|region|GPS|contracts|cargo|agency  #on/off");
                            MyAPIGateway.Utilities.ShowMissionScreen("Economy Help", "", "hud command", helpreply, null, "Groovy");
                            return true;
                        case "value":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/value X Y - Looks up item [X] of optional quantity [Y] and reports the buy and sell value.");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /value Ice 20    or   /value ice");
                            return true;
                        case "news":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/news Displays a news log of the last few signifiant server events");
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Eg, Pirate attacks, bounties, server competitions etc Example: /news");
                            return true;
                        case "ver":
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "/ver Displays the diagnostic version of running Economy script");
                            return true;
                        case "npczone":
                            helpreply = "/npczone list   -  Displays list of all defined NPC market portals\r\n" +
                                "/npczone add [name] [x] [y] [z] [size(radius) #] [shape(box/sphere)]  -  Add a new NPC market zone\r\n" +
                                " shape can be sphere or box. Eg /npczone add GunShop 1000 2000 4000 200 box\r\n" +
                                " /npczone delete [zone name]  - removes the named zone eg. /npczone delete freds\r\n" +
                                " /npczone rename oldname newname  -  change the ID name of the trade zone\r\n" +
                                " /npczone move [name] [x] [y] [z] [size(radius) #] [shape(box/sphere)]  -  move/resize the name trade zone\r\n";
                            if (MyAPIGateway.Session.Player.IsAdmin())
                            {   //but only if you are admin!
                                MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /npczone (add)/list/([remove])/[rename oldname newname] ([zone]) (x y z size shape)");
                                MyAPIGateway.Utilities.ShowMissionScreen("Economy Help", "", "npczone command", helpreply, null, "Close");
                                return true;
                            }
                            else return false; //i know that is mean...

                        case "pricelist":
                            helpreply = "/pricelist X\r\n Displays current market zone prices of item type [X]\r\n" +
                                "Eg X can be one or more of ore, ingot, component, ammo, tools\r\n" +
                                " ie /pricelist ore ingot (for ore AND ingot prices)\r\n" +
                                " /pricelist ore  (for ore prices only)\r\n" +
                                " /pricelist ingot (for ingot prices only)\r\n" +
                                " /pricelist component (lists component pricing only)\r\n" +
                                " /pricelist ammo (for ammunition prices only)\r\n" +
                                " /pricelist tools (lists tool prices only)\r\n";
                            MyAPIGateway.Utilities.ShowMessage("eHelp", "Example: /pricelist [optional item type] ore/ingot/component/ammmo/tools etc");
                            MyAPIGateway.Utilities.ShowMissionScreen("Economy Help", "", "pricelist command", helpreply, null, "Close");
                            return true;

                    }
                }
            }
            #endregion help

            // it didnt start with help or anything else that matters so return false and get us out of here;
            return false;
        }
        #endregion command list

        #region SetLanguage

        /// <summary>
        /// Sets the CultureInfo to use when formatting numbers and dates on the server, and the text resources when fetching names of game objects to display or send back to players.
        /// </summary>
        internal void SetLanguage()
        {
            MyTexts.LanguageDescription language = MyTexts.Languages.ContainsKey(ServerConfig.Language) ? MyTexts.Languages[ServerConfig.Language] : MyTexts.Languages[0];

            // Make sure it's up-to-date with correct value.
            ServerConfig.Language = (int)language.Id;

            ServerCulture = CultureInfo.GetCultureInfo(language.FullCultureName);

            // Load resources for that language.
            // This may acutally interfere with other mods or the server itself that are dependant the Text resources of the game.
            MyTexts.Clear();
            MyTexts.LoadTexts(Path.Combine(MyAPIGateway.Utilities.GamePaths.ContentPath, "Data", "Localization"), language.CultureName, language.SubcultureName);
        }

        #endregion
    }
}