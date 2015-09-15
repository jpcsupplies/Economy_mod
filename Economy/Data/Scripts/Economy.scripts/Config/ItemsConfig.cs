namespace Economy.scripts.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Sandbox.Definitions;
    using VRage;

    public class ItemsConfig
    {

        private void ReadDefinitions()
        {
            // Combination of Components.sbc, PhysicalItems.sbc, and AmmoMagazines.sbc files.
            var physicalItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach (var item in physicalItems)
            {
                if (item.Public)
                {
                    // get blueprint.
                    var blueprint = MyDefinitionManager.Static.GetBlueprintDefinition(item.Id);

                    // the localized name of the item.
                    var displayName = item.DisplayNameEnum.HasValue ? MyTexts.GetString(item.DisplayNameEnum.Value) : item.DisplayNameString;


                    // 2 identifiers.
                    //item.Id.TypeId.ToString()
                    //item.Id.SubtypeName
                }
            }

            //...
        }
    }
}
