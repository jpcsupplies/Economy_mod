namespace Economy.scripts.EconConfig
{
    using Sandbox.Definitions;
    using System.Linq;
    using VRage;
    using VRage.ObjectBuilders;
    using System.Collections.Generic;
    using System.IO;
    using Sandbox.ModAPI;

    public static class MarketManagement
    {
        public static string GetContentFilename()
        {
            return string.Format("Itemlist_{0}.txt", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        }

        public static MarketConfig LoadContent()
        {
            string filename = GetContentFilename();

            if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(MarketConfig)))
                return InitContent();

            TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(MarketConfig));

            var xmlText = reader.ReadToEnd();
            reader.Close();

            if (string.IsNullOrWhiteSpace(xmlText))
                return InitContent();

            MarketConfig config = null;
            try
            {
                config = MyAPIGateway.Utilities.SerializeFromXML<MarketConfig>(xmlText);
                EconomyScript.Instance.ServerLogger.Write("Loading existing MarketConfig.");
                SyncMarketItems(config);
            }
            catch
            {
                // content failed to deserialize.
                EconomyScript.Instance.ServerLogger.Write("Failed to deserialize MarketConfig. Creating new MarketConfig.");
                config = InitContent();
            }

            return config;
        }

        private static MarketConfig InitContent()
        {
            EconomyScript.Instance.ServerLogger.Write("Creating new MarketConfig.");
            MarketConfig marketConfig = new MarketConfig();
            marketConfig.MarketItems = new List<MarketStruct>();
            SyncMarketItems(marketConfig);
            return marketConfig;
        }

        public static void SaveContent(MarketConfig config)
        {
            string filename = GetContentFilename();
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(MarketConfig));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<MarketConfig>(config));
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Check that all current Definitions are in the MarketConfig.
        /// </summary>
        /// <param name="config"></param>
        private static void SyncMarketItems(MarketConfig config)
        {
            // Combination of Components.sbc, PhysicalItems.sbc, and AmmoMagazines.sbc files.
            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach (var item in physicalItems)
            {
                if (item.Public)
                {
                    // TypeId and SubtypeName are both Case sensitive. Do not Ignore case.
                    if (!config.MarketItems.Any(e => e.TypeId.Equals(item.Id.TypeId.ToString()) && e.SubtypeName.Equals(item.Id.SubtypeName)))
                    {
                        config.MarketItems.Add(new MarketStruct { TypeId = item.Id.TypeId.ToString(), SubtypeName = item.Id.SubtypeName, BuyPrice = 1, SellPrice = 1 });
                        EconomyScript.Instance.ServerLogger.Write("MarketItem Adding new item: {0} {1}.", item.Id.TypeId.ToString(), item.Id.SubtypeName);
                    }
                }
            }
        }

        /// <summary>
        /// Must be called by the Client for correct localization.
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="subtypeName"></param>
        /// <returns></returns>
        public static string GetDisplayName(string typeId, string subtypeName)
        {
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(typeId, out result))
            {
                var id = new MyDefinitionId(result, subtypeName);
                MyPhysicalItemDefinition definition;
                if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition))
                {
                    return definition.DisplayNameEnum.HasValue ? MyTexts.GetString(definition.DisplayNameEnum.Value) : definition.DisplayNameString;
                }
            }
            return "";
        }

        private static MyDefinitionId? GetDefinitionId(MarketStruct marketItem)
        {
            MyObjectBuilderType result;
            if (MyObjectBuilderType.TryParse(marketItem.TypeId, out result))
            {
                return new MyDefinitionId(result, marketItem.SubtypeName);
            }

            return null;
        }
    }
}
