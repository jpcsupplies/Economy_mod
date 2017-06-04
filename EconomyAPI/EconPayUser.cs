namespace EconomyAPI
{
    using ProtoBuf;

    public class EconPayUser : EconInterModBase
    {
        [ProtoMember(1)]
        public ulong FromPlayerIdentity;

        [ProtoMember(2)]
        public ulong ToPlayerIdentity;

        [ProtoMember(3)]
        public decimal TransactionAmount;

        [ProtoMember(4)]
        public string Reason;

        /// <summary>
        /// Player <paramref name="fromPlayerIdentity"/> pays player <paramref name="toPlayerIdentity"/> the amount of <paramref name="transactionAmount"/> for the reason of <paramref name="reason"/>.
        /// </summary>
        /// <param name="fromPlayerIdentity"></param>
        /// <param name="toPlayerIdentity"></param>
        /// <param name="transactionAmount"></param>
        /// <param name="callbackModChannel">Optional callback channel for the Economy Mod to send responses to.</param>
        /// <param name="transactionId">Optional identifier to keep track of specific responses initiated by the consuming mod.</param>
        /// <param name="reason"></param>
        public static void SendMessage(ulong fromPlayerIdentity, ulong toPlayerIdentity, decimal transactionAmount, string reason, long callbackModChannel = 0, long transactionId = 0)
        {
            EconInterModBase econMessage = new EconPayUser { FromPlayerIdentity = fromPlayerIdentity, ToPlayerIdentity = toPlayerIdentity, TransactionAmount = transactionAmount, Reason = reason, CallbackModChannel = callbackModChannel, TransactionId = transactionId };
            econMessage.SendEconomyMessage();
        }

        public override void ProcessEconomyCallback()
        {
            // this is processed by the Economy Mod.
        }
    }
}
