namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Economy.scripts;
    using Management;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage;

    /// <summary>
    /// this is to do the actual work of setting new prices and stock levels.
    /// </summary>
    [ProtoContract]
    public class MessageConfig : MessageBase
    {
        #region properties

        /// <summary>
        /// The key config item to set.
        /// </summary>
        [ProtoMember(1)]
        public string ConfigName;

        /// <summary>
        /// The value to set the config item to.
        /// </summary>
        [ProtoMember(2)]
        public string Value;

        #endregion

        public static void SendMessage(string configName, string value)
        {
            ConnectionHelper.SendMessageToServer(new MessageConfig { ConfigName = configName.ToLower(), Value = value });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            // Only Admin can change config.
            if (!player.IsAdmin())
            {
                EconomyScript.Instance.ServerLogger.WriteWarning("A Player without Admin \"{0}\" {1} attempted to access EConfig.", SenderDisplayName, SenderSteamId);
                return;
            }

            MyTexts.LanguageDescription myLanguage;

            // These will match with names defined in the RegEx patterm <EconomyScript.EconfigPattern>
            switch (ConfigName)
            {
                case "language":
                    if (string.IsNullOrEmpty(Value))
                    {
                        myLanguage = MyTexts.Languages[EconomyScript.Instance.ServerConfig.Language];
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "Language: {0} ({1})", myLanguage.Name, myLanguage.FullCultureName);
                    }
                    else
                    {
                        int intTest;
                        if (int.TryParse(Value, out intTest))
                        {
                            if (MyTexts.Languages.ContainsKey(intTest))
                            {
                                EconomyScript.Instance.ServerConfig.Language = intTest;
                                EconomyScript.Instance.SetLanguage();
                                myLanguage = MyTexts.Languages[intTest];
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "Language updated to: {0} ({1})", myLanguage.Name, myLanguage.FullCultureName);
                                return;
                            }
                        }

                        foreach (var lang in MyTexts.Languages)
                        {
                            if (lang.Value.Name.Equals(Value, StringComparison.InvariantCultureIgnoreCase)
                                || lang.Value.CultureName.Equals(Value, StringComparison.InvariantCultureIgnoreCase)
                                || lang.Value.FullCultureName.Equals(Value, StringComparison.InvariantCultureIgnoreCase))
                            {
                                EconomyScript.Instance.ServerConfig.Language = (int)lang.Value.Id;
                                EconomyScript.Instance.SetLanguage();
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "Language updated to: {0} ({1})", lang.Value.Name, lang.Value.FullCultureName);
                                return;
                            }
                        }

                        myLanguage = MyTexts.Languages[EconomyScript.Instance.ServerConfig.Language];
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "Language: {0} ({1})", myLanguage.Name, myLanguage.FullCultureName);
                    }
                    break;

                case "tradenetworkname":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradenetworkName: {0}", EconomyScript.Instance.ServerConfig.TradeNetworkName);
                    else
                    {
                        EconomyScript.Instance.ServerConfig.TradeNetworkName = Value;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradenetworkName updated to: \"{0}\"", EconomyScript.Instance.ServerConfig.TradeNetworkName);

                        // push updates to all clients.
                        var listPlayers = new List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(listPlayers);

                        foreach (var connectedPlayer in listPlayers)
                            MessageUpdateClient.SendTradeNetworkName(connectedPlayer.SteamUserId, EconomyScript.Instance.ServerConfig.TradeNetworkName);
                    }
                    break;


                case "currencyname":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "CurrencyName: {0}", EconomyScript.Instance.ServerConfig.CurrencyName);
                    else
                    {
                        EconomyScript.Instance.ServerConfig.CurrencyName = Value;
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "CurrencyName updated to: \"{0}\"", EconomyScript.Instance.ServerConfig.CurrencyName);

                        // push updates to all clients.
                        var listPlayers = new List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(listPlayers);

                        foreach (var connectedPlayer in listPlayers)
                            MessageUpdateClient.SendCurrencyName(connectedPlayer.SteamUserId, EconomyScript.Instance.ServerConfig.CurrencyName);
                    }
                    break;

                case "limitedrange":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedRange: {0}", EconomyScript.Instance.ServerConfig.LimitedRange ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            EconomyScript.Instance.ServerConfig.LimitedRange = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedRange updated to: {0}", EconomyScript.Instance.ServerConfig.LimitedRange ? "On" : "Off");
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedRange: {0}", EconomyScript.Instance.ServerConfig.LimitedRange ? "On" : "Off");
                    }
                    break;

                case "limitedsupply":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedSupply: {0}", EconomyScript.Instance.ServerConfig.LimitedSupply ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            EconomyScript.Instance.ServerConfig.LimitedSupply = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedSupply updated to: {0}", EconomyScript.Instance.ServerConfig.LimitedSupply ? "On" : "Off");
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LimitedSupply: {0}", EconomyScript.Instance.ServerConfig.LimitedSupply ? "On" : "Off");
                    }
                    break;

                case "enablelcds":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableLcds: {0}", EconomyScript.Instance.ServerConfig.EnableLcds ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            var clearRefresh = EconomyScript.Instance.ServerConfig.EnableLcds && !boolTest.Value;
                            EconomyScript.Instance.ServerConfig.EnableLcds = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableLcds updated to: {0}", EconomyScript.Instance.ServerConfig.EnableLcds ? "On" : "Off");

                            if (clearRefresh)
                                LcdManager.BlankLcds();
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableLcds: {0}", EconomyScript.Instance.ServerConfig.EnableLcds ? "On" : "Off");
                    }
                    break;

                case "tradetimeout":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradeTimeout: {0}", EconomyScript.Instance.ServerConfig.TradeTimeout);
                    else
                    {
                        TimeSpan timeTest;
                        if (TimeSpan.TryParse(Value, out timeTest))
                        {
                            EconomyScript.Instance.ServerConfig.TradeTimeout = timeTest;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradeTimeout updated to: {0} ", EconomyScript.Instance.ServerConfig.TradeTimeout);
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradeTimeout: {0}", EconomyScript.Instance.ServerConfig.TradeTimeout);
                    }
                    break;

                case "accountexpiry":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "AccountExpiry: {0}", EconomyScript.Instance.ServerConfig.AccountExpiry);
                    else
                    {
                        TimeSpan timeTest;
                        if (TimeSpan.TryParse(Value, out timeTest))
                        {
                            EconomyScript.Instance.ServerConfig.AccountExpiry = timeTest;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "AccountExpiry updated to: {0} ", EconomyScript.Instance.ServerConfig.AccountExpiry);
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "AccountExpiry: {0}", EconomyScript.Instance.ServerConfig.AccountExpiry);
                    }
                    break;

                case "startingbalance":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "StartingBalance: {0}", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                    else
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, out decimalTest))
                        {
                            if (decimalTest >= 0)
                            {
                                EconomyScript.Instance.ServerConfig.DefaultStartingBalance = decimalTest;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "StartingBalance updated to: {0} ", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                                return;
                            }
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "StartingBalance: {0}", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                    }
                    break;

                default:
                    var msg = new StringBuilder();

                    myLanguage = MyTexts.Languages[EconomyScript.Instance.ServerConfig.Language];
                    msg.AppendFormat("Language: {0} ({1})\r\n", myLanguage.Name, myLanguage.FullCultureName);
                    msg.AppendFormat("TradenetworkName: \"{0}\"\r\n", EconomyScript.Instance.ServerConfig.TradeNetworkName);
                    msg.AppendFormat("LimitedRange: {0}\r\n", EconomyScript.Instance.ServerConfig.LimitedRange ? "On" : "Off");
                    msg.AppendFormat("LimitedSupply: {0}\r\n", EconomyScript.Instance.ServerConfig.LimitedSupply ? "On" : "Off");
                    msg.AppendFormat("TradeTimeout: {0}  (days.hours:mins:secs)\r\n", EconomyScript.Instance.ServerConfig.TradeTimeout);
                    msg.AppendFormat("StartingBalance: {0}\r\n", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                    msg.AppendFormat("CurrencyName: \"{0}\"\r\n", EconomyScript.Instance.ServerConfig.CurrencyName);
                    msg.AppendFormat("AccountExpiry: {0}  (days.hours:mins:secs)\r\n", EconomyScript.Instance.ServerConfig.AccountExpiry);
                    msg.AppendFormat("EnableLcds: {0}\r\n", EconomyScript.Instance.ServerConfig.EnableLcds ? "On" : "Off");
                    MessageClientDialogMessage.SendMessage(SenderSteamId, "ECONFIG", " ", msg.ToString());
                    break;
            }
        }

        private bool? GetBool(string value)
        {
            bool boolTest;
            if (bool.TryParse(value, out boolTest))
                return boolTest;

            if (value.Equals("off", StringComparison.InvariantCultureIgnoreCase) || value == "0")
                return false;

            if (value.Equals("on", StringComparison.InvariantCultureIgnoreCase) || value == "1")
                return true;
            return null;
        }
    }
}
