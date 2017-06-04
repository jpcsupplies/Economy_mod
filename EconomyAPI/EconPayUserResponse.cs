namespace EconomyAPI
{
    using ProtoBuf;
    using Sandbox.ModAPI;

    public enum EconPayUserMessage
    {
        PaymentsNotEnabled = 0,
        InvalidRequest = 1,
        NoSenderAccount = 2,
        NoRepientAccount = 3,
        InsufficientFunds = 4,
        Success = 5
    }

    public class EconPayUserResponse : EconInterModBase
    {
        [ProtoMember(1)]
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
