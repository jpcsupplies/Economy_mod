namespace Economy.scripts.Messages
{
    using System;
    using Config;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using System.Linq;
    using Economy.scripts;

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
            bool hasAccount = false;

            //string alias; //represents players current in game nickname
            //string timestamp; //will be used for seen command later maybe
            //int records; //number of record lines in bank file replaced by "BankConfigData.Accounts.Count.ToString()"
            //int count; //counter for iterating over records no longer used


            // did we at least type /pay someone something . . .
            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            var accountToSpend = EconomyScript.Instance.BankConfigData.FindOrCreateAccount(SenderSteamId, SenderDisplayName);

            //* It needs to first check the player has enough to cover his payment
            var bankBalance = Convert.ToDecimal(accountToSpend.BankBalance);
            if (TransactionAmount <= bankBalance || payingPlayer.IsAdmin())
            // do we have enough or are we admin so it doesnt matter
            //*      if true, 
            {
                //*          it needs to check the person being paid has an account record, 
                var account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(
                    a => a.NickName.Equals(ToUserName, StringComparison.InvariantCultureIgnoreCase));

                //*               if false, 
                //*                  it needs to check if the other player is even online
                //*                     if true
                //*                         create one with default balance.
                //*                         flag hasaccount bool true

                //*                     if false
                //*                         display an error message player not found
                //*                         flag hasaccount bool false
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

                //is there a modify property to save the need to remove then re-add? 
                //here we remove this players bank record
                EconomyScript.Instance.BankConfigData.Accounts.Remove(accountToSpend);
                if (!payingPlayer.IsAdmin())
                {
                    TransactionAmount = Math.Abs(TransactionAmount);
                }
                //admins can give or take money, normal players can only give money so convert negative to positive
                //here we add the players bank record again with the updated balance minus what they spent
                accountToSpend = new BankAccountStruct()
                {
                    BankBalance = (bankBalance - TransactionAmount),
                    Date = DateTime.Now,
                    NickName = SenderDisplayName,
                    SteamId = SenderSteamId
                };
                EconomyScript.Instance.BankConfigData.Accounts.Add(accountToSpend);

                //here we retrive the target player steam id and balance
                ulong theirID = account.SteamId;
                decimal theirbank = (account.BankBalance += TransactionAmount);
                string theirnick = account.NickName;
                //here we clean out the old data
                EconomyScript.Instance.BankConfigData.Accounts.Remove(account);
                //here we build a new record with the correct data
                account = new BankAccountStruct()
                {
                    BankBalance = theirbank,
                    Date = DateTime.Now,
                    NickName = theirnick,
                    SteamId = theirID
                };
                //here we write it back to our bank ledger file
                EconomyScript.Instance.BankConfigData.Accounts.Add(account);

                //if this works this is a very sexy way to work with our file
                //testing: it does indeed work, if i was a teenager id probably need to change my underwear at this point

                // now need to work out how to notify receiving player that they were paid and/or any message the sending player wrote
                // which needs to not send if the player isnt online - pity ive no idea how to write to the faction chat system
                // be a good place to send the player a faction message as it would work even if they were offline..

                MessageClientTextMessage.SendMessage(account.SteamId, "PAY",
                    string.Format("{0}, {1} just paid you {2} for {3}", theirnick, SenderDisplayName, TransactionAmount, Reason));

                MessageClientTextMessage.SendMessage(SenderSteamId, "PAY",
                    string.Format("You just paid {0}, {2} for {3}", theirnick, SenderDisplayName, TransactionAmount, Reason));

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
