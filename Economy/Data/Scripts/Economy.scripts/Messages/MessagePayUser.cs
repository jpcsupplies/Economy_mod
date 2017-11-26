namespace Economy.scripts.Messages
{
    using System;
    using EconConfig;
    using Economy.scripts;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessagePayUser : MessageBase
    {
        [ProtoMember(201)]
        public string ToUserName;

        [ProtoMember(202)]
        public decimal TransactionAmount;

        [ProtoMember(203)]
        public string Reason;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            if (!EconomyScript.Instance.ServerConfig.EnablePlayerPayments)
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Player payments is disabled.");
                return;
            }

            // did we at least type /pay someone something . . .
            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            var accountToSpend = AccountManager.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);

            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                TransactionAmount = Math.Abs(TransactionAmount);

            // It needs to first check the player has enough to cover his payment
            if (TransactionAmount <= accountToSpend.BankBalance || payingPlayer.IsAdmin())
            // do we have enough or are we admin so it doesnt matter
            //*      if true, 
            {
                // it needs to check the person being paid has an account record, 
                var account = AccountManager.FindAccount(ToUserName);

                //*               if true - it will always be true if real as it would have created it on join anyway

                //*               if false -  then they were never on this server anyway or seen would have added them already
                //*                         display an error message player not found
                if (account == null)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry, player does not exist or have an account!");
                    return;
                }
                //*               if true, { flag hasaccount bool true }

                if (account.SteamId == payingPlayer.SteamUserId)
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry, you cannot pay yourself!");
                    return;
                }

                //*          if hasaccount bool true   
                // admins can give or take money, normal players can only give money so convert negative to positive
                // here we add the players bank record again with the updated balance minus what they spent
                accountToSpend.BankBalance -= TransactionAmount;
                accountToSpend.Date = DateTime.Now;

                // here we retrive the target player steam id and balance
                // here we write it back to our bank ledger file
                account.BankBalance += TransactionAmount;
                account.Date = DateTime.Now;

                MessageUpdateClient.SendAccountMessage(accountToSpend);
                MessageUpdateClient.SendAccountMessage(account);

                // if this works this is a very sexy way to work with our file
                // testing: it does indeed work, if i was a teenager id probably need to change my underwear at this point

                // This notifies receiving player that they were paid and/or any message the sending player wrote
                // which needs to not send if the player isnt online - pity ive no idea how to write to the faction chat system
                // be a good place to send the player a faction message as it would work even if they were offline..
                MessageClientTextMessage.SendMessage(account.SteamId, "PAY",
                    string.Format("{0}, {1} just paid you {2} {4} for {3}", account.NickName, SenderDisplayName, TransactionAmount, Reason, EconomyScript.Instance.ServerConfig.CurrencyName));

                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY",
                    string.Format("You just paid {0} {1} {3} for {2}", account.NickName, TransactionAmount, Reason, EconomyScript.Instance.ServerConfig.CurrencyName));

                EconomyScript.Instance.ServerLogger.WriteVerbose("Pay: '{0}' sent {1} {3} to '{2}'", accountToSpend.NickName, TransactionAmount, ToUserName, EconomyScript.Instance.ServerConfig.CurrencyName);


                //*      if false/otherwise throw error you dont have enough money
            }
            else
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry you don't have enough {0}", EconomyScript.Instance.ServerConfig.CurrencyName);
            }
        }

        public static void SendMessage(string toUserName, decimal transactionAmount, string reason)
        {
            ConnectionHelper.SendMessageToServer(new MessagePayUser { ToUserName = toUserName, TransactionAmount = transactionAmount, Reason = reason });
        }
    }
}
