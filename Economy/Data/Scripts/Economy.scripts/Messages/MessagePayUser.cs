namespace Economy.scripts.Messages
{
    using System;
    using Economy.scripts;
    using ProtoBuf;
    using Sandbox.ModAPI;

    [ProtoContract]
    public class MessagePayUser : MessageBase
    {
        [ProtoMember(1)]
        public string ToUserName;

        [ProtoMember(2)]
        public decimal TransactionAmount;

        [ProtoMember(3)]
        public string Reason;

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            // flag used when transferring funds eg /pay bob 10 - indicates if bob even has an account record yet
            //bool hasAccount = false;

            //string alias; //represents players current in game nickname
            //string timestamp; //will be used for seen command later maybe
            //int records; //number of record lines in bank file replaced by "BankConfigData.Accounts.Count.ToString()"
            //int count; //counter for iterating over records no longer used


            // did we at least type /pay someone something . . .
            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            var accountToSpend = EconomyScript.Instance.BankConfigData.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);

            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                TransactionAmount = Math.Abs(TransactionAmount);

            // It needs to first check the player has enough to cover his payment
            if (TransactionAmount <= accountToSpend.BankBalance || payingPlayer.IsAdmin())
            // do we have enough or are we admin so it doesnt matter
            //*      if true, 
            {
                // it needs to check the person being paid has an account record, 
                var account = EconomyScript.Instance.BankConfigData.FindAccount(ToUserName);

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

                // is there a modify property to save the need to remove then re-add? 
                // admins can give or take money, normal players can only give money so convert negative to positive
                // here we add the players bank record again with the updated balance minus what they spent
                accountToSpend.BankBalance -= TransactionAmount;
                accountToSpend.Date = DateTime.Now;

                // here we retrive the target player steam id and balance
                // here we write it back to our bank ledger file
                account.BankBalance += TransactionAmount;
                account.Date = DateTime.Now;

                // if this works this is a very sexy way to work with our file
                // testing: it does indeed work, if i was a teenager id probably need to change my underwear at this point

                // This notifies receiving player that they were paid and/or any message the sending player wrote
                // which needs to not send if the player isnt online - pity ive no idea how to write to the faction chat system
                // be a good place to send the player a faction message as it would work even if they were offline..
                MessageClientTextMessage.SendMessage(account.SteamId, "PAY",
                    string.Format("{0}, {1} just paid you {2} for {3}", account.NickName, SenderDisplayName, TransactionAmount, Reason));

                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY",
                    string.Format("You just paid {0}, {1} for {2}", account.NickName, TransactionAmount, Reason));

                EconomyScript.Instance.ServerLogger.Write("Pay: '{0}' sent {1} to '{2}'", accountToSpend.NickName, TransactionAmount, ToUserName);


                //*      if false/otherwise throw error you dont have enough money
            }
            else
            {
                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY", "Sorry you can't afford that much!");
            }
        }

        public static void SendMessage(string toUserName, decimal transactionAmount, string reason)
        {
            ConnectionHelper.SendMessageToServer(new MessagePayUser { ToUserName = toUserName, TransactionAmount = transactionAmount, Reason = reason });
        }
    }
}
