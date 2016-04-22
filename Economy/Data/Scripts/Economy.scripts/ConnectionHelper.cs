namespace Economy.scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Economy.scripts.Messages;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;

    /// <summary>
    /// Conains useful methods and fields for organizing the connections.
    /// </summary>
    public static class ConnectionHelper
    {
        #region fields

        public static List<byte> ClientMessageCache = new List<byte>();
        public static Dictionary<ulong, List<byte>> ServerMessageCache = new Dictionary<ulong, List<byte>>();

        private const int MAX_MESSAGE_SIZE = 4096; 

        #endregion

        #region connections to server

        /// <summary>
        /// Creates and sends an entity with the given information for the server. Never call this on DS instance!
        /// </summary>

        public static void SendMessageToServer(MessageBase message)
        {
            message.Side = MessageSide.ServerSide;
            message.SenderSteamId = MyAPIGateway.Session.Player.SteamUserId;
            message.SenderDisplayName = MyAPIGateway.Session.Player.DisplayName;
            message.SenderLanguage = (int)MyAPIGateway.Session.Config.Language;
            try
            {
                var xml = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() {Content = message});
                byte[] byteData = System.Text.Encoding.Unicode.GetBytes(xml);
                if (byteData.Length <= MAX_MESSAGE_SIZE)
                    MyAPIGateway.Multiplayer.SendMessageToServer(EconomyConsts.ConnectionId, byteData);
                else
                    SendMessageParts(byteData, MessageSide.ServerSide);
            }
            catch (Exception ex)
            {
                EconomyScript.Instance.ClientLogger.WriteException(ex);
                //TODO: send exception detail to Server.
            }
        }

        #endregion

        #region connections to all

        /// <summary>
        /// Creates and sends an entity with the given information for the server and all players.
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessageToAll(MessageBase message)
        {
            message.SenderSteamId = MyAPIGateway.Session.Player.SteamUserId;
            message.SenderDisplayName = MyAPIGateway.Session.Player.DisplayName;
            message.SenderLanguage = (int)MyAPIGateway.Session.Config.Language;

            if (!MyAPIGateway.Multiplayer.IsServer)
                SendMessageToServer(message);
            SendMessageToAllPlayers(message);
        }

        #endregion

        #region connections to clients

        public static void SendMessageToPlayer(ulong steamId, MessageBase message)
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("SendMessageToPlayer {0} {1} {2}.", steamId, message.Side, message.GetType().Name);
            message.Side = MessageSide.ClientSide;
            try
            {
                var xml = MyAPIGateway.Utilities.SerializeToXML(new MessageContainer() {Content = message});
                byte[] byteData = System.Text.Encoding.Unicode.GetBytes(xml);
                if (byteData.Length <= MAX_MESSAGE_SIZE)
                    MyAPIGateway.Multiplayer.SendMessageTo(EconomyConsts.ConnectionId, byteData, steamId);
                else
                    SendMessageParts(byteData, MessageSide.ClientSide, steamId);
            }
            catch (Exception ex)
            {
                EconomyScript.Instance.ClientLogger.WriteException(ex);
                //TODO: send exception detail to Server.
            }
        }

        public static void SendMessageToAllPlayers(MessageBase messageContainer)
        {
            //MyAPIGateway.Multiplayer.SendMessageToOthers(StandardClientId, System.Text.Encoding.Unicode.GetBytes(ConvertData(content))); <- does not work as expected ... so it doesn't work at all?
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p != null);
            foreach (IMyPlayer player in players)
                SendMessageToPlayer(player.SteamUserId, messageContainer);
        }

        # endregion

        #region processing

        /// <summary>
        /// Server side execution of the actions defined in the data.
        /// </summary>
        /// <param name="dataString"></param>
        public static void ProcessData(string dataString)
        {
            EconomyScript.Instance.ServerLogger.WriteStart("Start Message Serialization");
            MessageContainer message = null;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromXML<MessageContainer>(dataString);
            }
            catch
            {
                EconomyScript.Instance.ServerLogger.WriteError("Message cannot Deserialize");
                return;
            }

            EconomyScript.Instance.ServerLogger.WriteStop("End Message Serialization");

            if (message != null && message.Content != null)
            {
                try
                {
                    message.Content.InvokeProcessing();
                }
                catch (Exception e)
                {
                    EconomyScript.Instance.ServerLogger.WriteError("Processing message exception. Side: {0}, Exception: {1}", message.Content.Side, e.ToString());
                }
                return;
            }
        }


        public static void ProcessData(byte[] rawData)
        {
            ProcessData(System.Text.Encoding.Unicode.GetString(rawData));
        }

        #endregion

        /// <summary>
        /// Calculates how many bytes can be stored in the given message.
        /// </summary>
        /// <param name="message">The message in which the bytes will be stored.</param>
        /// <returns>The number of bytes that can be stored until the message is too big to be sent.</returns>
        public static int GetFreeByteElementCount(MessageIncomingMessageParts message)
        {
            message.Content = new byte[1];
            var xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
            var oneEntry = System.Text.Encoding.Unicode.GetBytes(xmlText).Length;

            message.Content = new byte[4];
            xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
            var twoEntries = System.Text.Encoding.Unicode.GetBytes(xmlText).Length;

            // we calculate the difference between one and two entries in the array to get the count of bytes that describe one entry
            // we divide by 3 because 3 entries are stored in one block of the array
            var difference = (double)(twoEntries - oneEntry) / 3d;

            // get the size of the message without any entries
            var freeBytes = MAX_MESSAGE_SIZE - oneEntry - Math.Ceiling(difference);

            int count = (int)Math.Floor((double)freeBytes / difference);

            // finally we test if the calculation was right
            message.Content = new byte[count];
            xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
            var finalLength = System.Text.Encoding.Unicode.GetBytes(xmlText).Length;
            EconomyScript.Instance.ServerLogger.WriteVerbose(string.Format("FinalLength: {0}", finalLength));
            if (MAX_MESSAGE_SIZE >= finalLength)
                return count;
            else
                throw new Exception(string.Format("Calculation failed. OneEntry: {0}, TwoEntries: {1}, Difference: {2}, FreeBytes: {3}, Count: {4}, FinalLength: {5}", oneEntry, twoEntries, difference, freeBytes, count, finalLength));
        }

        private static void SendMessageParts(byte[] byteData, MessageSide side, ulong receiver = 0)
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("SendMessageParts {0} {1} {2}.", byteData.Length, side, receiver);

            var byteList = byteData.ToList();

            while (byteList.Count > 0)
            {
                // we create an empty message part
                var messagePart = new MessageIncomingMessageParts()
                {
                    Side = side,
                    SenderSteamId = side == MessageSide.ServerSide ? MyAPIGateway.Session.Player.SteamUserId : 0,
                    SenderDisplayName = side == MessageSide.ServerSide ? MyAPIGateway.Session.Player.DisplayName : "",
                    SenderLanguage = side == MessageSide.ServerSide ? (int)MyAPIGateway.Session.Config.Language : 0,
                    LastPart = false,
                };

                try
                {
                    // let's check how much we could store in the message
                    int freeBytes = GetFreeByteElementCount(messagePart);

                    int count = freeBytes;

                    // we check if that might be the last message
                    if (freeBytes > byteList.Count)
                    {
                        messagePart.LastPart = true;

                        // since we changed LastPart, we should make sure that we are still able to send all the stuff
                        if (GetFreeByteElementCount(messagePart) > byteList.Count)
                        {
                            count = byteList.Count;
                        }
                        else
                            throw new Exception("Failed to send message parts. The leftover could not be sent!");
                    }

                    // fill the message with content
                    messagePart.Content = byteList.GetRange(0, count).ToArray();
                    var xmlPart = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = messagePart });
                    var bytes = System.Text.Encoding.Unicode.GetBytes(xmlPart);

                    // and finally send the message
                    switch (side)
                    {
                        case MessageSide.ClientSide:
                            if (MyAPIGateway.Multiplayer.SendMessageTo(EconomyConsts.ConnectionId, bytes, receiver))
                                byteList.RemoveRange(0, count);
                            else
                                throw new Exception("Failed to send message parts to client.");
                            break;
                        case MessageSide.ServerSide:
                            if (MyAPIGateway.Multiplayer.SendMessageToServer(EconomyConsts.ConnectionId, bytes))
                                byteList.RemoveRange(0, count);
                            else
                                throw new Exception("Failed to send message parts to server.");
                            break;
                    }

                }
                catch (Exception ex)
                {
                    EconomyScript.Instance.ServerLogger.WriteException(ex);
                    EconomyScript.Instance.ClientLogger.WriteException(ex);
                    return;
                }
            }
        }
    }
}
