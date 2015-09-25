namespace Economy.scripts.Messages
{
    using System;
    using System.Linq;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using Economy.scripts;
    using EconConfig;

    /// <summary>
    /// this is to do the actual work of moving the goods checks need to occur before this
    /// </summary>
    [ProtoContract]
    public class MessageSell : MessageBase
    {
        /// <summary>
        /// person, NPC, offer or faction to sell to
        /// </summary>
        [ProtoMember(1)]
        public string ToUserName;

        /// <summary>
        /// qty of item
        /// </summary>
        [ProtoMember(2)]
        public decimal ItemQuantity;

        /// <summary>
        /// item name / id we are selling
        /// </summary>
        [ProtoMember(3)]
        public string ItemTypeId;

        [ProtoMember(4)]
        public string ItemSubTypeName;

        /// <summary>
        /// unit price of item
        /// </summary>
        [ProtoMember(4)]
        public decimal ItemPrice;

        /// <summary>
        /// Use the Current Buy price to sell it at. The Player 
        /// will not have access to this information without fetching it first. This saves us the trouble.
        /// </summary>
        [ProtoMember(5)]
        public bool UseBankBuyPrice;

        /// <summary>
        /// We are selling to the Merchant.
        /// </summary>
        [ProtoMember(6)]
        public bool SellToMerchant;

        /// <summary>
        /// The Item is been put onto the market.
        /// </summary>
        [ProtoMember(7)]
        public bool OfferToMarket;

        //[ProtoMember(8)]
        //public string zone; //used to identify market we are selling to ??

        public static void SendMessage(string toUserName, decimal itemQuantity, string itemTypeId, string itemSubTypeName, decimal itemPrice, bool useBankBuyPrice, bool sellToMerchant, bool offerToMarket)
        {
            ConnectionHelper.SendMessageToServer(new MessageSell { ToUserName = toUserName, ItemQuantity = itemQuantity, ItemTypeId = itemTypeId, ItemSubTypeName = itemSubTypeName, ItemPrice = itemPrice, UseBankBuyPrice = useBankBuyPrice, SellToMerchant = sellToMerchant, OfferToMarket = offerToMarket });
        }

        public override void ProcessClient()
        {
            // never processed on client
            //will we need this to remove players inventory items?
        }

        public override void ProcessServer()
        {
            BankAccountStruct account;

            //* Logic:                     
            //* Get player steam ID
            var payingPlayer = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            // Who are we selling to
            if (SellToMerchant)
                account = EconomyScript.Instance.BankConfigData.Accounts.FirstOrDefault(a => a.SteamId == EconomyConsts.NpcMerchantId);
            else
                account = EconomyScript.Instance.BankConfigData.FindAccount(ToUserName);

            if (account == null)
            {
                ReturnInventoryItems();
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, player does not exist or have an account!");
                return;
            }

            var item = EconomyScript.Instance.MarketConfigData.MarketItems.FirstOrDefault(e => e.TypeId == ItemTypeId && e.SubtypeName == ItemSubTypeName);
            if (item == null)
            {
                ReturnInventoryItems();
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the items you are trying to sell doesn't have a market entry!");
                // TODO: in reality, this item needs not just to have an entry created, but a value applied also. It's the value that is more important.
                return;
            }

            if (item.IsBlacklisted)
            {
                ReturnInventoryItems();
                MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "Sorry, the item you tried to sell is blacklisted on this server.");
                return;
            }

            if (UseBankBuyPrice)
                ItemPrice = item.SellPrice * ItemQuantity;

            var accountToSell = EconomyScript.Instance.BankConfigData.FindOrCreateAccount(SenderSteamId, SenderDisplayName, SenderLanguage);
            var transactionAmount = ItemPrice * ItemQuantity;
            
            // need fix negative amounts before checking if the player can afford it.
            if (!payingPlayer.IsAdmin())
                transactionAmount = Math.Abs(transactionAmount);

            if (SellToMerchant)
            {
                // here we look up item price and transfer items and money as appropriate
            }
            else if (OfferToMarket)
            {
                // Here we post offer to appropriate zone market
            }
            else
            {
                // is it a player then?             
            }


            /*
            //  old code to be disemboweled later

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
            } */
        }

        private void ReturnInventoryItems()
        {
            // TODO: return the items to the seller inventory.
        }
    }
}
