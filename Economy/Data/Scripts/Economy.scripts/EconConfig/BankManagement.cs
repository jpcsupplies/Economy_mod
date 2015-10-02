namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Sandbox.ModAPI;

    public static class BankManagement
    {
        [Obsolete("To be removed")]
        public static string GetContentFilename()
        {
            return string.Format("Bank_{0}.txt", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        }

        [Obsolete("To be removed")]
        public static BankConfig LoadContent()
        {
            string filename = GetContentFilename();

            if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(BankConfig)))
                return InitContent();

            TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(BankConfig));

            var xmlText = reader.ReadToEnd();
            reader.Close();

            if (string.IsNullOrWhiteSpace(xmlText))
                return InitContent();

            BankConfig config = null;
            try
            {
                config = MyAPIGateway.Utilities.SerializeFromXML<BankConfig>(xmlText);
            }
            catch
            {
                // content failed to deserialize.
                EconomyScript.Instance.ServerLogger.Write("Failed to deserialize BankConfig. Creating new BankConfig.");
                config = InitContent();
            }

            return config;
        }

        [Obsolete("To be removed")]
        private static BankConfig InitContent()
        {
            BankConfig bankConfig = new BankConfig();
            bankConfig.Accounts = new List<BankAccountStruct>();
            return bankConfig;
        }

        [Obsolete("To be removed")]
        public static void SaveContent(BankConfig config)
        {
            string filename = GetContentFilename();
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(BankConfig));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<BankConfig>(config));
            writer.Flush();
            writer.Close();
        }
    }
}
