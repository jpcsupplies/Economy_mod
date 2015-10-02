namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ObjectBuilders;

    public static class MarketManager
    {
        [Obsolete("To be removed")]
        public static string GetContentFilename()
        {
            return string.Format("Itemlist_{0}.txt", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        }

        [Obsolete("To be removed")]
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
            }
            catch
            {
                // content failed to deserialize.
                EconomyScript.Instance.ServerLogger.Write("Failed to deserialize MarketConfig. Creating new MarketConfig.");
                config = InitContent();
            }

            return config;
        }

        [Obsolete("To be removed")]
        private static MarketConfig InitContent()
        {
            EconomyScript.Instance.ServerLogger.Write("Creating new MarketConfig.");
            MarketConfig marketConfig = new MarketConfig();
            marketConfig.MarketItems = new List<MarketStruct>();
            return marketConfig;
        }

        [Obsolete("To be removed")]
        public static void SaveContent(MarketConfig config)
        {
            string filename = GetContentFilename();
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(MarketConfig));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<MarketConfig>(config));
            writer.Flush();
            writer.Close();
        }

        #region Market helpers

        /// <summary>
        /// Check that all current Definitions are in the EconContentStruct.
        /// </summary>
        /// <param name="marketItems"></param>
        public static void SyncMarketItems(ref List<MarketStruct> marketItems)
        {
            // Combination of Components.sbc, PhysicalItems.sbc, and AmmoMagazines.sbc files.
            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach (var item in physicalItems)
            {
                if (item.Public)
                {
                    // TypeId and SubtypeName are both Case sensitive. Do not Ignore case.
                    if (!marketItems.Any(e => e.TypeId.Equals(item.Id.TypeId.ToString()) && e.SubtypeName.Equals(item.Id.SubtypeName)))
                    {
                        marketItems.Add(new MarketStruct { TypeId = item.Id.TypeId.ToString(), SubtypeName = item.Id.SubtypeName, BuyPrice = 1, SellPrice = 1, IsBlacklisted = false });
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

        #endregion
    }
}
