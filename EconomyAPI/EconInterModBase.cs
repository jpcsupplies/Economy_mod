namespace EconomyAPI
{
    using ProtoBuf;
    using Sandbox.ModAPI;

    /// <summary>
    /// Base class to manage the Economy API. This should not be changed.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(EconPayUser))]
    [ProtoInclude(2, typeof(EconPayUserResponse))]
    public abstract class EconInterModBase
    {
        /// <summary>
        /// This is the specific channel used by the Economy Mod for communication. Do not change it.
        /// </summary>
        const long EconInterModChannel = 913846912;

        [ProtoMember(101)]
        public long CallbackModChannel;

        [ProtoMember(102)]
        public long TransactionId;

        public abstract void ProcessEconomyCallback();

        public void SendEconomyMessage()
        {
            byte[] byteData = MyAPIGateway.Utilities.SerializeToBinary(this);
            MyAPIGateway.Utilities.SendModMessage(EconInterModChannel, byteData);
        }
    }
}
