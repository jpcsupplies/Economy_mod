namespace Economy.scripts.Messages
{
    using System;
    using System.Xml.Serialization;
    using ProtoBuf;
    using Sandbox.ModAPI;

    // ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
    [XmlInclude(typeof(EconPayUser))]
    [XmlInclude(typeof(EconPayUserResponse))]
    [ProtoContract]
    /*
    // ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
    [ProtoInclude(1, typeof(EconPayUser))]
	//*/
    public abstract class EconInterModBase
    {
        [ProtoMember(1)]
        public long CallbackModChannel;

        [ProtoMember(2)]
        public long TransactionId;

        public void InvokeProcessing()
        {
            EconomyScript.Instance.ServerLogger.WriteVerbose("Received - {0}", this.GetType().Name);
            try
            {
                ProcessServer();
            }
            catch (Exception ex)
            {
                EconomyScript.Instance.ServerLogger.WriteException(ex);
            }
        }

        public abstract void ProcessServer();

        public void SendResponseMessage(long callbackModChannel, long transactionId)
        {
            // a channel of zero means it hasn't been set by the caller.
            if (callbackModChannel == 0)
                return;

            EconomyScript.Instance.ServerLogger.WriteStart("Sending Reponse: {0}, Channel={1}, Transaction={2}", this.GetType().Name, callbackModChannel, transactionId);
            var xml = MyAPIGateway.Utilities.SerializeToXML(new EconMessageContainer { Content = this });
            MyAPIGateway.Utilities.SendModMessage(callbackModChannel, xml);

            // Not supported as yet.
            //byte[] byteData = MyAPIGateway.Utilities.SerializeToBinary(econMessage);
            //MyAPIGateway.Utilities.SendModMessage(EconInterModId, byteData);
        }
    }
}
