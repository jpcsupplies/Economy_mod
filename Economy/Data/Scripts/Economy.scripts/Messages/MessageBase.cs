﻿namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    // ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
    [XmlInclude(typeof(MessageIncomingMessageParts))]
    [XmlInclude(typeof(MessageConnectionRequest))]
    [XmlInclude(typeof(MessagePayUser))]
    [XmlInclude(typeof(MessagePlayerSeen))]
    [XmlInclude(typeof(MessageBankBalance))]
    [XmlInclude(typeof(MessageListAccounts))]
    [XmlInclude(typeof(MessageResetAccount))]
    [XmlInclude(typeof(MessageClientDialogMessage))]
    [XmlInclude(typeof(MessageClientTextMessage))]
    [ProtoContract]
    public abstract class MessageBase
    {
        /// <summary>
        /// The SteamId of the message's sender. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
        /// </summary>
        [ProtoMember(1)]
        public ulong SenderSteamId;


        [ProtoMember(2)]
        public string SenderDisplayName;

        /// <summary>
        /// Defines on which side the message should be processed. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
        /// </summary>
        [ProtoMember(3)]
        public MessageSide Side;

        /*
        [ProtoAfterDeserialization]
        void InvokeProcessing() // is not invoked after deserialization from xml
        {
            Logger.Debug("START - Processing");
            switch (Side)
            {
                case MessageSide.ClientSide:
                    ProcessClient();
                    break;
                case MessageSide.ServerSide:
                    ProcessServer();
                    break;
            }
            Logger.Debug("END - Processing");
        }
        */

        public void InvokeProcessing()
        {
            switch (Side)
            {
                case MessageSide.ClientSide:
                    InvokeClientProcessing();
                    break;
                case MessageSide.ServerSide:
                    InvokeServerProcessing();
                    break;
            }
        }

        private void InvokeClientProcessing()
        {
            EconomyScript.Instance.ClientLogger.Write("Received - {0}", this.GetType().Name);
            try
            {
                ProcessClient();
            }
            catch (Exception ex)
            {
                // TODO: send error to server and notify admins
                EconomyScript.Instance.ClientLogger.WriteException(ex);
            }
        }

        private void InvokeServerProcessing()
        {
            EconomyScript.Instance.ServerLogger.Write("Received - {0}", this.GetType().Name);
            try
            {
                ProcessServer();
            }
            catch (Exception ex)
            {
                EconomyScript.Instance.ServerLogger.WriteException(ex);
            }
        }

        public abstract void ProcessClient();
        public abstract void ProcessServer();
    }
}
