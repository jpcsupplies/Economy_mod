namespace Economy.scripts.Messages
{
    using ProtoBuf;

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

        public override void ProcessServer()
        {
            // This is processed by the Caller.
        }

        public static void SendMessage(long callbackModChannel, long transactionId, EconPayUserMessage message)
        {
            EconPayUserResponse response = new EconPayUserResponse { Message = message, TransactionId = transactionId };
            response.SendResponseMessage(callbackModChannel, transactionId);
        }
    }
}
