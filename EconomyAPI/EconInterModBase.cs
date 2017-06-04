namespace EconomyAPI
{
    using System.Xml.Serialization;
    using ProtoBuf;
    using Sandbox.ModAPI;

    /// <summary>
    /// Base class to manage the Economy API. This should not be changed.
    /// </summary>
    [XmlInclude(typeof(EconPayUser))]
    [XmlInclude(typeof(EconPayUserResponse))]
    [ProtoContract]
    public abstract class EconInterModBase
    {
        /// <summary>
        /// This is the specific channel used by the Economy Mod for communication. Do not change it.
        /// </summary>
        const long EconInterModChannel = 913846912;

        [ProtoMember(1)]
        public long CallbackModChannel;

        [ProtoMember(2)]
        public long TransactionId;

        public abstract void ProcessEconomyCallback();

        public void SendEconomyMessage()
        {
            var xml = MyAPIGateway.Utilities.SerializeToXML(new EconMessageContainer { Content = this });
            MyAPIGateway.Utilities.SendModMessage(EconInterModChannel, xml);

            // Not supported as yet.
            //byte[] byteData = MyAPIGateway.Utilities.SerializeToBinary(econMessage);
            //MyAPIGateway.Utilities.SendModMessage(EconInterModId, byteData);
        }
    }
}
