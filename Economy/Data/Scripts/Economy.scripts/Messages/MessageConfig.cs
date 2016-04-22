namespace Economy.scripts.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Economy.scripts;
    using Management;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.Game.ModAPI;

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
                #region language

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

                #endregion

                #region tradenetworkname

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

                #endregion

                #region currencyname

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

                #endregion

                #region limitedrange

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

                #endregion

                #region limitedsupply

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


                #endregion

                #region enablelcds

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

                #endregion

                #region EnableNpcTradezones

                case "enablenpctradezones":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableNpcTradezones: {0}", EconomyScript.Instance.ServerConfig.EnableNpcTradezones ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            var clearRefresh = EconomyScript.Instance.ServerConfig.EnableNpcTradezones && !boolTest.Value;
                            EconomyScript.Instance.ServerConfig.EnableNpcTradezones = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableNpcTradezones updated to: {0}", EconomyScript.Instance.ServerConfig.EnableNpcTradezones ? "On" : "Off");

                            if (clearRefresh)
                                LcdManager.BlankLcds();
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnableNpcTradezones: {0}", EconomyScript.Instance.ServerConfig.EnableNpcTradezones ? "On" : "Off");
                    }
                    break;

                #endregion

                #region EnablePlayerTradezones

                case "enableplayertradezones":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerTradezones: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerTradezones ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            var clearRefresh = EconomyScript.Instance.ServerConfig.EnablePlayerTradezones && !boolTest.Value;
                            EconomyScript.Instance.ServerConfig.EnablePlayerTradezones = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerTradezones updated to: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerTradezones ? "On" : "Off");

                            if (clearRefresh)
                                LcdManager.BlankLcds();
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerTradezones: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerTradezones ? "On" : "Off");
                    }
                    break;

                #endregion

                #region EnablePlayerPayments

                case "enableplayerpayments":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerPayments: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerPayments ? "On" : "Off");
                    else
                    {
                        bool? boolTest = GetBool(Value);
                        if (boolTest.HasValue)
                        {
                            var clearRefresh = EconomyScript.Instance.ServerConfig.EnablePlayerPayments && !boolTest.Value;
                            EconomyScript.Instance.ServerConfig.EnablePlayerPayments = boolTest.Value;
                            MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerPayments updated to: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerPayments ? "On" : "Off");

                            if (clearRefresh)
                                LcdManager.BlankLcds();
                            return;
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "EnablePlayerPayments: {0}", EconomyScript.Instance.ServerConfig.EnablePlayerPayments ? "On" : "Off");
                    }
                    break;

                #endregion

                #region tradetimeout

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

                #endregion

                #region accountexpiry

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

                #endregion

                #region startingbalance

                case "startingbalance":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "StartingBalance: {0}", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                    else
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, out decimalTest))
                        {
                            // TODO: perhaps we should truncate the value.

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

                #endregion

                #region LicenceMin

                case "licencemin":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMin: {0}", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin);
                    else
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, out decimalTest))
                        {
                            // TODO: perhaps we should truncate the value.

                            if (decimalTest >= 0)
                            {
                                if (EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax < decimalTest)
                                {
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMin cannot be more than LicenceMax.");
                                    return;
                                }

                                EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin = decimalTest;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMin updated to: {0} ", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin);
                                return;
                            }
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMin: {0}", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin);
                    }
                    break;

                #endregion

                #region LicenceMax

                case "licencemax":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMax: {0}", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax);
                    else
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, out decimalTest))
                        {
                            // TODO: perhaps we should truncate the value.

                            if (decimalTest >= 0)
                            {
                                if (decimalTest < EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin)
                                {
                                    MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMax cannot be less than LicenceMin.");
                                    return;
                                }

                                EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax = decimalTest;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMax updated to: {0} ", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax);
                                return;
                            }
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "LicenceMax: {0}", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax);
                    }
                    break;

                #endregion

                #region RelinkRatio

                case "relinkratio":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "RelinkRatio: {0}", EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
                    else
                    {
                        var numFormat = CultureInfo.CurrentCulture.NumberFormat;

                        NumberFormatInfo nfi = new NumberFormatInfo()
                        {
                            CurrencyDecimalDigits = numFormat.PercentDecimalDigits,
                            CurrencyDecimalSeparator = numFormat.PercentDecimalSeparator,
                            CurrencyGroupSeparator = numFormat.PercentGroupSeparator,
                            CurrencyGroupSizes = numFormat.PercentGroupSizes,
                            CurrencyNegativePattern = numFormat.PercentNegativePattern,
                            CurrencyPositivePattern = numFormat.PercentPositivePattern,
                            CurrencySymbol = numFormat.PercentSymbol
                        };

                        decimal decimalTest;
                        if (decimal.TryParse(Value, out decimalTest))
                        {
                            // TODO: perhaps we should truncate the value.

                            if (decimalTest >= 0)
                            {
                                EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio = decimalTest;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "RelinkRatio updated to: {0:P} ", EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
                                return;
                            }
                        }
                        else if (decimal.TryParse(Value, NumberStyles.Currency, nfi, out decimalTest))
                        {
                            // TODO: perhaps we should truncate the value.

                            if (decimalTest >= 0)
                            {
                                EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio = decimalTest / 100;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "RelinkRatio updated to: {0:P} ", EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
                                return;
                            }
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "RelinkRatio: {0:P}", EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
                    }
                    break;

                #endregion

                #region MaximumPlayerZones

                case "maximumplayerzones":
                    if (string.IsNullOrEmpty(Value))
                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "MaximumPlayerZones: {0}", EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                    else
                    {
                        int intTest;
                        if (int.TryParse(Value, out intTest))
                        {
                            if (intTest >= 0)
                            {
                                EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones = intTest;
                                MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "MaximumPlayerZones updated to: {0} ", EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                                return;
                            }
                        }

                        MessageClientTextMessage.SendMessage(SenderSteamId, "ECONFIG", "TradeZoneLicence: {0}", EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);
                    }
                    break;

                #endregion

                #region default

                default:
                    var msg = new StringBuilder();

                    myLanguage = MyTexts.Languages[EconomyScript.Instance.ServerConfig.Language];
                    msg.AppendFormat("Language: {0} ({1})\r\n", myLanguage.Name, myLanguage.FullCultureName);
                    msg.AppendFormat("TradenetworkName: \"{0}\"\r\n", EconomyScript.Instance.ServerConfig.TradeNetworkName);
                    msg.AppendFormat("LimitedRange: {0}\r\n", EconomyScript.Instance.ServerConfig.LimitedRange ? "On" : "Off");
                    msg.AppendFormat("LimitedSupply: {0}\r\n", EconomyScript.Instance.ServerConfig.LimitedSupply ? "On" : "Off");
                    msg.AppendFormat("TradeTimeout: {0}  (days.hours:mins:secs)\r\n", EconomyScript.Instance.ServerConfig.TradeTimeout);
                    msg.AppendFormat("StartingBalance: {0:#,#.######}\r\n", EconomyScript.Instance.ServerConfig.DefaultStartingBalance);
                    msg.AppendFormat("CurrencyName: \"{0}\"\r\n", EconomyScript.Instance.ServerConfig.CurrencyName);
                    msg.AppendFormat("AccountExpiry: {0}  (days.hours:mins:secs)\r\n", EconomyScript.Instance.ServerConfig.AccountExpiry);
                    msg.AppendFormat("EnableLcds: {0}\r\n", EconomyScript.Instance.ServerConfig.EnableLcds ? "On" : "Off");
                    msg.AppendFormat("EnableNpcTradezones: {0}\r\n", EconomyScript.Instance.ServerConfig.EnableNpcTradezones ? "On" : "Off");
                    msg.AppendLine();
                    msg.AppendLine("--- Player Tradezones ---");
                    msg.AppendFormat("EnablePlayerTradezones: {0}\r\n", EconomyScript.Instance.ServerConfig.EnablePlayerTradezones ? "On" : "Off");
                    msg.AppendFormat("EnablePlayerPayments: {0}\r\n", EconomyScript.Instance.ServerConfig.EnablePlayerPayments ? "On" : "Off");
                    msg.AppendFormat("LicenceMin: {0:#,#.######} (at {1:#,#.######}m)\r\n", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMin, EconomyScript.Instance.ServerConfig.TradeZoneMinRadius);
                    msg.AppendFormat("LicenceMax: {0:#,#.######} (at {1:#,#.######}m)\r\n", EconomyScript.Instance.ServerConfig.TradeZoneLicenceCostMax, EconomyScript.Instance.ServerConfig.TradeZoneMaxRadius);
                    msg.AppendFormat("RelinkRatio: {0:P}\r\n", EconomyScript.Instance.ServerConfig.TradeZoneRelinkRatio);
                    msg.AppendFormat("MaximumPlayerZones: {0}\r\n", EconomyScript.Instance.ServerConfig.MaximumPlayerTradeZones);

                    MessageClientDialogMessage.SendMessage(SenderSteamId, "ECONFIG", " ", msg.ToString());
                    break;

                    #endregion
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
