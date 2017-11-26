namespace EconomyAPI
{
    using ProtoBuf;

    public enum EconPayUserMessage : byte
    {
        PaymentsNotEnabled = 0,
        InvalidRequest = 1,
        NoSenderAccount = 2,
        NoRepientAccount = 3,
        InsufficientFunds = 4,
        Success = 5
    }

    [ProtoContract]
    public class EconPayUserResponse : EconInterModBase
    {
        [ProtoMember(201)]
        public EconPayUserMessage Message;

        public override void ProcessEconomyCallback()
        {
            // TODO: for the caller to utilize.
            // Sample...
            //MyAPIGateway.Utilities.ShowMessage("Callback", string.Format("EconPayUserResponse: {0}, TransactionId {1}", Message, TransactionId));
            VRage.Utils.MyLog.Default.WriteLine(string.Format("Callback EconPayUserResponse: {0}, TransactionId {1}", Message, TransactionId));
        }
    }
}
