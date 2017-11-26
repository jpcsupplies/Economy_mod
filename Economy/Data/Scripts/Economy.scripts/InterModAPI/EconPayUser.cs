namespace Economy.scripts.Messages
{
    using Economy.scripts;
    using Economy.scripts.EconConfig;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System;

    [ProtoContract]
    public class EconPayUser : EconInterModBase
    {
        [ProtoMember(201)]
        public ulong FromPlayerIdentity;

        [ProtoMember(202)]
        public ulong ToPlayerIdentity;

        [ProtoMember(203)]
        public decimal TransactionAmount;

        [ProtoMember(204)]
        public string Reason;

        public override void ProcessServer()
        {
            if (!EconomyScript.Instance.ServerConfig.EnablePlayerPayments)
            {
                EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.PaymentsNotEnabled);
                return;
            }

            // can't pay yourself.
            if (FromPlayerIdentity == ToPlayerIdentity)
            {
                EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.InvalidRequest);
                return;
            }

            // did we at least type /pay someone something . . .
            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(FromPlayerIdentity);

            var accountToSpend = AccountManager.FindAccount(FromPlayerIdentity);
            if (accountToSpend == null)
            {
                EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.NoRepientAccount);
                return;
            }

            // need fix negative amounts before checking if the player can afford it.
            if (payingPlayer != null && !payingPlayer.IsAdmin())
                TransactionAmount = Math.Abs(TransactionAmount);

            // It needs to first check the player has enough to cover his payment
            if (TransactionAmount <= accountToSpend.BankBalance || (payingPlayer != null && payingPlayer.IsAdmin()))
            // do we have enough or are we admin so it doesnt matter
            {
                // it needs to check the person being paid has an account record, 
                var account = AccountManager.FindAccount(ToPlayerIdentity);
                if (account == null)
                {
                    EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.NoSenderAccount);
                    return;
                }

                // admins can give or take money, normal players can only give money so convert negative to positive
                // here we add the players bank record again with the updated balance minus what they spent
                accountToSpend.BankBalance -= TransactionAmount;
                accountToSpend.Date = DateTime.Now;

                // here we retrive the target player steam id and balance
                // here we write it back to our bank ledger file
                account.BankBalance += TransactionAmount;
                account.Date = DateTime.Now;

                // This pushes account updates to the clients, so it displays changes on their hud.
                MessageUpdateClient.SendAccountMessage(accountToSpend);
                MessageUpdateClient.SendAccountMessage(account);

                EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.Success);
                EconomyScript.Instance.ServerLogger.WriteVerbose("Pay: '{0}' sent {1} {3} to '{2}'", accountToSpend.NickName, TransactionAmount, account.NickName, EconomyScript.Instance.ServerConfig.CurrencyName);
            }
            else
            {
                EconPayUserResponse.SendMessage(CallbackModChannel, TransactionId, EconPayUserMessage.InsufficientFunds);
            }
        }
    }
}
