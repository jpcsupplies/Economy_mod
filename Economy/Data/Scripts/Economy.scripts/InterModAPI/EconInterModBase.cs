namespace Economy.scripts.Messages
{
    using Economy.scripts;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System;

    // ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
    [ProtoContract]
    [ProtoInclude(1, typeof(EconPayUser))]
    [ProtoInclude(2, typeof(EconPayUserResponse))]
    public abstract class EconInterModBase
    {
        [ProtoMember(101)]
        public long CallbackModChannel;

        [ProtoMember(102)]
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
            byte[] byteData = MyAPIGateway.Utilities.SerializeToBinary(this);
            MyAPIGateway.Utilities.SendModMessage(callbackModChannel, byteData);
        }
    }
}
