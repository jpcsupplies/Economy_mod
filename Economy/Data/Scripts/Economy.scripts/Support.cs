namespace Economy.scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public static class Support
    {
        #region fields

        private static string[] _oreNames;
        private static List<string> _ingotNames;
        private static MyPhysicalItemDefinition[] _physicalItems;
        private static string[] _physicalItemNames;
        private static bool _hasBuiltComponentList;

        #endregion

        /// <summary>
        /// Builds a list of the in game components and their localized names for searching so 
        /// that subsequent calls to FindPhysicalParts(...) don't have to build a lists every time.
        /// </summary>
        private static void BuildComponentLists()
        {
            if (_hasBuiltComponentList)
                return;

            EconomyScript.Instance.ClientLogger.Write("BuildComponentLists");

            MyDefinitionManager.Static.GetOreTypeNames(out _oreNames);
            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            _physicalItems = physicalItems.Where(item => item.Public).ToArray();  // Limit to public items.  This will remove the CubePlacer. :)
            _ingotNames = new List<string>();

            foreach (var physicalItem in _physicalItems)
            {
                if (physicalItem.Id.TypeId == typeof(MyObjectBuilder_Ingot))
                {
                    _ingotNames.Add(physicalItem.Id.SubtypeName);
                }
            }

            // Make sure all Public Physical item names are unique, so they can be properly searched for.
            var names = new List<string>();
            foreach (var item in _physicalItems)
            {
                var baseName = item.DisplayNameEnum.HasValue ? MyTexts.GetString(item.DisplayNameEnum.Value) : item.DisplayNameString;
                var uniqueName = baseName;
                var index = 1;
                while (names.Contains(uniqueName, StringComparer.InvariantCultureIgnoreCase))
                {
                    index++;
                    uniqueName = string.Format("{0}{1}", baseName, index);
                }
                names.Add(uniqueName);
            }
            _physicalItemNames = names.ToArray();

            _hasBuiltComponentList = true;
        }

        /// <summary>
        /// Find the physical object of the specified name or partial name.
        /// </summary>
        /// <param name="itemName">The name of the physical object to find.</param>
        /// <param name="objectBuilder">The object builder of the physical object, ready for use.</param>
        /// <param name="options">Returns a list of potential matches if there was more than one of the same or partial name.</param>
        /// <returns>Returns true if a single exact match was found.</returns>
        public static bool FindPhysicalParts(string itemName, out MyObjectBuilder_Base objectBuilder, out string[] options)
        {
            BuildComponentLists();

            itemName = itemName.Trim();
            var itemNames = itemName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // prefix the search term with 'ore' to find this ore name.
            if (itemNames.Length > 1 && itemNames[0].Equals("ore", StringComparison.InvariantCultureIgnoreCase))
            {
                var findName = itemName.Substring(4).Trim();

                var exactMatchOres = _oreNames.Where(ore => ore.Equals(findName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                if (exactMatchOres.Length == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ore() { SubtypeName = exactMatchOres[0] };
                    options = new string[0];
                    return true;
                }
                else if (exactMatchOres.Length > 1)
                {
                    objectBuilder = null;
                    options = exactMatchOres;
                    return false;
                }

                var partialMatchOres = _oreNames.Where(ore => ore.IndexOf(findName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();
                if (partialMatchOres.Length == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ore() { SubtypeName = partialMatchOres[0] };
                    options = new string[0];
                    return true;
                }
                else if (partialMatchOres.Length > 1)
                {
                    objectBuilder = null;
                    options = partialMatchOres;
                    return false;
                }

                objectBuilder = null;
                options = new string[0];
                return false;
            }

            // prefix the search term with 'ingot' to find this ingot name.
            if (itemNames.Length > 0 && itemNames[0].Equals("ingot", StringComparison.InvariantCultureIgnoreCase))
            {
                var findName = itemName.Substring(6).Trim();

                var exactMatchIngots = _ingotNames.Where(ingot => ingot.Equals(findName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                if (exactMatchIngots.Length == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ingot() { SubtypeName = exactMatchIngots[0] };
                    options = new string[0];
                    return true;
                }
                else if (exactMatchIngots.Length > 1)
                {
                    objectBuilder = null;
                    options = exactMatchIngots;
                    return false;
                }

                var partialMatchIngots = _ingotNames.Where(ingot => ingot.IndexOf(findName, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();
                if (partialMatchIngots.Length == 1)
                {
                    objectBuilder = new MyObjectBuilder_Ingot() { SubtypeName = partialMatchIngots[0] };
                    options = new string[0];
                    return true;
                }
                else if (partialMatchIngots.Length > 1)
                {
                    objectBuilder = null;
                    options = partialMatchIngots;
                    return false;
                }

                objectBuilder = null;
                options = new string[0];
                return false;
            }

            // full name match.
            var res = _physicalItemNames.FirstOrDefault(s => s != null && s.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

            // need a good method for finding partial name matches.
            if (res == null)
            {
                var matches = _physicalItemNames.Where(s => s != null && s.StartsWith(itemName, StringComparison.InvariantCultureIgnoreCase)).Distinct().ToArray();

                if (matches.Length == 1)
                {
                    res = matches.FirstOrDefault();
                }
                else
                {
                    matches = _physicalItemNames.Where(s => s != null && s.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) >= 0).Distinct().ToArray();
                    if (matches.Length == 1)
                    {
                        res = matches.FirstOrDefault();
                    }
                    else if (matches.Length > 1)
                    {
                        objectBuilder = null;
                        options = matches;
                        return false;
                    }
                }
            }

            if (res != null)
            {
                var item = _physicalItems[Array.IndexOf(_physicalItemNames, res)];
                if (item != null)
                {
                    objectBuilder = MyObjectBuilderSerializer.CreateNewObject(item.Id.TypeId, item.Id.SubtypeName);
                    options = new string[0];
                    return true;
                }
            }

            objectBuilder = null;
            options = new string[0];
            return false;
        }

        /// <summary>
        /// check the seller is in range of a valid trade region or player
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static bool RangeCheck(IMyPlayer player1, IMyPlayer player2)
        {
            //if (limited range setting is false or My location works out To be within 2500 of a valid trade area)
            //lookup the location of target name and compare with location of seller
            //there has to be an easy way to do this, the GPSs use it..
            // TODO: implement. https://github.com/jpcsupplies/Economy_mod/issues/49

            var character1 = player1.GetCharacter();
            var character2 = player2.GetCharacter();

            if (character1 == null || character2 == null)
                // one player or the other doesn't exist in game.
                return false;

            Vector3D position1 = ((IMyEntity)character1).GetPosition();
            Vector3D position2 = ((IMyEntity)character2).GetPosition();

            // TODO: check broadcast antenna. There doesn't appear to be any accessible API to help at this stage.
            // MyAntennaSystem.GetPlayerRelayedBroadcasters(MyCharacter playerCharacter, MyEntity interactedEntityRepresentative, HashSet<MyDataBroadcaster> output)
            // MyAntennaSystem.CheckConnection(MyIdentity sender, MyIdentity receiver)
            // MySyncCharacter.CheckPlayerConnection(ulong senderSteamId, ulong receiverSteamId)

            var distance = Vector3D.Distance(position1, position2);

            // so did it come within our default range
            return distance <= 2500;
        }
    }
}
