namespace Economy.scripts.Messages
{
    using System;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    // ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
    [ProtoInclude(2, typeof(MessageConnectionRequest))]
    [ProtoInclude(3, typeof(MessageConnectionResponse))]
    [ProtoInclude(4, typeof(MessagePayUser))]
    [ProtoInclude(5, typeof(MessagePlayerSeen))]
    [ProtoInclude(6, typeof(MessageBankBalance))]
    [ProtoInclude(7, typeof(MessageUpdateClient))]
    [ProtoInclude(8, typeof(MessageMarketItemValue))]
    [ProtoInclude(9, typeof(MessageMarketManageNpc))]
    [ProtoInclude(10, typeof(MessageMarketManagePlayer))]
    [ProtoInclude(11, typeof(MessageMarketPriceList))]
    [ProtoInclude(12, typeof(MessageMission))]
    [ProtoInclude(13, typeof(MessageListAccounts))]
    [ProtoInclude(14, typeof(MessageResetAccount))]
    [ProtoInclude(15, typeof(MessageClientDialogMessage))]
    [ProtoInclude(16, typeof(MessageClientTextMessage))]
    [ProtoInclude(17, typeof(MessageSell))]
    [ProtoInclude(18, typeof(MessageBuy))]
    [ProtoInclude(19, typeof(MessageSet))]
    [ProtoInclude(20, typeof(MessageConfig))]
    [ProtoInclude(21, typeof(MessageWorth))]
    [ProtoInclude(22, typeof(MessageRewardAccount))]
    [ProtoInclude(23, typeof(MessageShipSale))]
    [ProtoInclude(24, typeof(MessageClientSound))]
    [ProtoInclude(25, typeof(MessageHudUpdate))]
    public abstract class MessageBase
    {
        /// <summary>
        /// The SteamId of the message's sender. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
        /// </summary>
        [ProtoMember(101)]
        public ulong SenderSteamId;

        /// <summary>
        /// The display name of the message sender.
        /// </summary>
        [ProtoMember(102)]
        public string SenderDisplayName;

        /// <summary>
        /// The current display language of the sender.
        /// </summary>
        [ProtoMember(103)]
        public int SenderLanguage;

        /// <summary>
        /// Defines on which side the message should be processed. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
        /// </summary>
        [ProtoMember(104)]
        public MessageSide Side;

        public MessageBase()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
                SenderSteamId = MyAPIGateway.Multiplayer.ServerId;
            if (MyAPIGateway.Session.Player != null)
                SenderSteamId = MyAPIGateway.Session.Player.SteamUserId;
        }

        /*
        [ProtoAfterDeserialization]
        void InvokeProcessing() // is not invoked after deserialization from xml
        {
            EconomyScript.Instance.ServerLogger.Write("START - Processing");
            switch (Side)
            {
                case MessageSide.ClientSide:
                    ProcessClient();
                    break;
                case MessageSide.ServerSide:
                    ProcessServer();
                    break;
            }
            EconomyScript.Instance.ServerLogger.Write("END - Processing");
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
            EconomyScript.Instance.ClientLogger.WriteVerbose("Received - {0}", this.GetType().Name);
            try
            {
                ProcessClient();
            }
            catch (Exception ex)
            {
                // TODO: send error to server and notify admins
                EconomyScript.Instance.ClientLogger.WriteException(ex, "Could not process message on Client.");
            }
        }

        private void InvokeServerProcessing()
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("Received - {0}", this.GetType().Name);
            try
            {
                ProcessServer();
            }
            catch (Exception ex)
            {
                EconomyScript.Instance.ServerLogger.WriteException(ex, "Could not process message on Server.");
            }
        }

        public abstract void ProcessClient();
        public abstract void ProcessServer();
    }
}
