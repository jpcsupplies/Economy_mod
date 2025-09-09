namespace Economy.scripts.EconConfig
{
    using Economy.scripts.EconStructures;
    using MissionStructures;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using VRageMath;

    public static class EconDataManager
    {
        #region Load and save CONFIG

        public static EconConfigStruct LoadConfig()
        {
            EconConfigStruct config;
            string xmlText;

            // new file name and location.
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(EconomyConsts.WorldStorageConfigFilename, typeof(EconConfigStruct)))
            {
                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(EconomyConsts.WorldStorageConfigFilename, typeof(EconConfigStruct));
                xmlText = reader.ReadToEnd();
                reader.Close();
            }
            else
            {
                config = InitConfig();
                ValidateAndUpdateConfig(config);
                return config;
            }

            if (string.IsNullOrWhiteSpace(xmlText))
            {
                config = InitConfig();
                ValidateAndUpdateConfig(config);
                return config;
            }

            try
            {
                config = MyAPIGateway.Utilities.SerializeFromXML<EconConfigStruct>(xmlText);
                EconomyScript.Instance.ServerLogger.WriteInfo("Loading existing EconConfigStruct.");
            }
            catch
            {
                // config failed to deserialize.
                EconomyScript.Instance.ServerLogger.WriteError("Failed to deserialize EconConfigStruct. Creating new EconConfigStruct.");
                config = InitConfig();
            }

            if (config == null || config.DefaultPrices == null || config.DefaultPrices.Count == 0)
                config = InitConfig();

            ValidateAndUpdateConfig(config);
            return config;
        }

        private static void ValidateAndUpdateConfig(EconConfigStruct config)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Validating and Updating Config.");

            // Sync in whatever is defined in the game (may contain new cubes, and modded cubes).
            MarketManager.SyncMarketItems(ref config.DefaultPrices);

            if (config.TradeTimeout.TotalSeconds < 1f)
            {
                config.TradeTimeout = new TimeSpan(0, 0, 1); // limit minimum trade timeout to 1 second.
                EconomyScript.Instance.ServerLogger.WriteWarning("TradeTimeout has been reset, as it was below 1 second.");
            }
        }

        private static EconConfigStruct InitConfig()
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating new EconConfigStruct.");
            EconConfigStruct config = new EconConfigStruct();
            config.DefaultPrices = new List<MarketItemStruct>();

            #region Default prices in raw Xml.

            const string xmlText = @"<Market>
<MarketItems>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>ZoneChip</SubtypeName>
      <Quantity>100000</Quantity>
      <SellPrice>1147</SellPrice>
      <BuyPrice>1000</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Datapad</TypeId>
      <SubtypeName>Datapad</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>8</SellPrice>
      <BuyPrice>7</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Package</TypeId>
      <SubtypeName>Package</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>5000</SellPrice>
      <BuyPrice>2000</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>Medkit</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>12</SellPrice>
      <BuyPrice>11</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_FruitBar</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>150</SellPrice>
      <BuyPrice>90</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_GardenSlaw</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>200</SellPrice>
      <BuyPrice>50</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_RedPellets</SubtypeName>
      <Quantity>50</Quantity>
      <SellPrice>80</SellPrice>
      <BuyPrice>20</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Chili</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1000</SellPrice>
      <BuyPrice>100</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Flatbread</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>100</SellPrice>
      <BuyPrice>20</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_FruitPastry</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>130</SellPrice>
      <BuyPrice>30</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_VeggieBurger</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>250</SellPrice>
      <BuyPrice>31</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_GreenPellets</SubtypeName>
      <Quantity>600</Quantity>
      <SellPrice>80</SellPrice>
      <BuyPrice>20</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Curry</SubtypeName>
      <Quantity>40</Quantity>
      <SellPrice>300</SellPrice>
      <BuyPrice>40</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Dumplings</SubtypeName>
      <Quantity>10</Quantity>
      <SellPrice>130</SellPrice>
      <BuyPrice>30</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Spaghetti</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>150</SellPrice>
      <BuyPrice>35</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Lasagna</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>151</SellPrice>
      <BuyPrice>35</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Burrito</SubtypeName>
      <Quantity>20</Quantity>
      <SellPrice>135</SellPrice>
      <BuyPrice>31</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_FrontierStew</SubtypeName>
      <Quantity>50</Quantity>
      <SellPrice>600</SellPrice>
      <BuyPrice>60</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_SearedSabiroid</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1000</SellPrice>
      <BuyPrice>100</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
       <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
       <SubtypeName>MealPack_SteakDinner</SubtypeName>
       <Quantity>0</Quantity>
       <SellPrice>700</SellPrice>
       <BuyPrice>80</BuyPrice>
       <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
       <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
       <SubtypeName>MammalMeatRaw</SubtypeName>
       <Quantity>0</Quantity>
       <SellPrice>50</SellPrice>
       <BuyPrice>10</BuyPrice>
       <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MammalMeatCooked</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>70</SellPrice>
      <BuyPrice>20</BuyPrice>
    <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>InsectMeatRaw</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>80</SellPrice>
      <BuyPrice>30</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>InsectMeatCooked</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>100</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>RadiationKit</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>501</SellPrice>
      <BuyPrice>350</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_Ramen</SubtypeName>
      <Quantity>5000</Quantity>
      <SellPrice>10</SellPrice>
      <BuyPrice>9</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>MealPack_KelpCrisp</SubtypeName>
      <Quantity>80000</Quantity>
      <SellPrice>30</SellPrice>
      <BuyPrice>10</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalObject</TypeId>
      <SubtypeName>Algae</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>3</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_SeedItem</TypeId>
      <SubtypeName>Fruit</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>300</SellPrice>
      <BuyPrice>80</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_SeedItem</TypeId>
      <SubtypeName>Grain</SubtypeName>
      <Quantity>1500</Quantity>
      <SellPrice>150</SellPrice>
      <BuyPrice>50</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_SeedItem</TypeId>
      <SubtypeName>Mushrooms</SubtypeName>
      <Quantity>2000</Quantity>
      <SellPrice>200</SellPrice>
      <BuyPrice>61</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_SeedItem</TypeId>
      <SubtypeName>Vegetables</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>250</SellPrice>
      <BuyPrice>70</BuyPrice>
    <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_ConsumableItem</TypeId>
      <SubtypeName>Powerkit</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>11.4</SellPrice>
      <BuyPrice>10.4</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalObject</TypeId>
      <SubtypeName>SpaceCredit</SubtypeName>
      <Quantity>1000000000</Quantity>
      <SellPrice>0.0107</SellPrice>
      <BuyPrice>0.0107</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>NATO_5p56x45mm</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>2.35</SellPrice>
      <BuyPrice>2.09</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>NATO_25x184mm</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>84.78</SellPrice>
      <BuyPrice>75.36</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>Missile200mm</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>59.10 </SellPrice>
      <BuyPrice>52.54</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Construction</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>2</SellPrice>
      <BuyPrice>1.78</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>MetalGrid</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>58.72</SellPrice>
      <BuyPrice>52.19</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>InteriorPlate</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.70</SellPrice>
      <BuyPrice>0.62</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>SteelPlate</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>4.20</SellPrice>
      <BuyPrice>3.73</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Girder</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>1.40</SellPrice>
      <BuyPrice>1.24</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>SmallTube</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>0.89</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>LargeTube</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>6</SellPrice>
      <BuyPrice>5.34</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Motor</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>37.74</SellPrice>
      <BuyPrice>33.54</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Display</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>21.99</SellPrice>
      <BuyPrice>19.54</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>BulletproofGlass</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>65.36</SellPrice>
      <BuyPrice>58.10</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Computer</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.97</SellPrice>
      <BuyPrice>0.86</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Reactor</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>52.23</SellPrice>
      <BuyPrice>46.42</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Thrust</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>140.62</SellPrice>
      <BuyPrice>125</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>GravityGenerator</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>1920.16</SellPrice>
      <BuyPrice>1706.81</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Medical</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>666.32</SellPrice>
      <BuyPrice>592.29</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>RadioCommunication</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>5.96</SellPrice>
      <BuyPrice>5.30</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Detector</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>102.20</SellPrice>
      <BuyPrice>90.85</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Explosives</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>46.38</SellPrice>
      <BuyPrice>41.23</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>SolarCell</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>102.33</SellPrice>
      <BuyPrice>90.96</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>PowerCell</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>19.85</SellPrice>
      <BuyPrice>17.65</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Stone</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.13</SellPrice>
      <BuyPrice>0.12</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Iron</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.11</SellPrice>
      <BuyPrice>0.10</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Nickel</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>2.16</SellPrice>
      <BuyPrice>1.92</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Cobalt</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>1.81</SellPrice>
      <BuyPrice>1.61</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Magnesium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.07</SellPrice>
      <BuyPrice>0.06</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Silicon</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>2.44</SellPrice>
      <BuyPrice>2.17</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Silver</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.73</SellPrice>
      <BuyPrice>0.65</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Gold</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.08</SellPrice>
      <BuyPrice>0.07</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Platinum</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.15</SellPrice>
      <BuyPrice>0.14</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Uranium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.17</SellPrice>
      <BuyPrice>0.16</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Stone</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.19</SellPrice>
      <BuyPrice>0.17</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Iron</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.20</SellPrice>
      <BuyPrice>0.18</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Nickel</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>6.75</SellPrice>
      <BuyPrice>6</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Cobalt</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>7.53</SellPrice>
      <BuyPrice>6.69</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Magnesium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>12.30</SellPrice>
      <BuyPrice>10.93</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Silicon</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>4.36</SellPrice>
      <BuyPrice>3.87</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Silver</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>19.10</SellPrice>
      <BuyPrice>18.09</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Gold</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>29.87</SellPrice>
      <BuyPrice>28.77</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Platinum</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>52.37</SellPrice>
      <BuyPrice>51</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Uranium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>52.36</SellPrice>
      <BuyPrice>50.99</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>10.73</SellPrice>
      <BuyPrice>0.65</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>EngineerPlushie</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1000</SellPrice>
      <BuyPrice>900</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>EngineerPlushie</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1000</SellPrice>
      <BuyPrice>900</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>SabiroidPlushie</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1000</SellPrice>
      <BuyPrice>900</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FlareClip</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>10</SellPrice>
      <BuyPrice>5</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxBlue</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxGreen</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxRed</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxPink</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxYellow</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
      <SubtypeName>FireworksBoxRainbow</SubtypeName>
      <Quantity>500</Quantity>
      <SellPrice>50</SellPrice>
      <BuyPrice>45</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>FlareGunItem</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>80</SellPrice>
      <BuyPrice>60</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>PreciseAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>120.84</SellPrice>
      <BuyPrice>20.52</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>RapidFireAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>130.43</SellPrice>
      <BuyPrice>30.05</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>UltimateAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>150.94</SellPrice>
      <BuyPrice>50.28</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_OxygenContainerObject</TypeId>
      <SubtypeName>OxygenBottle</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>261.99</SellPrice>
      <BuyPrice>232.88</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>WelderItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>12.68</SellPrice>
      <BuyPrice>1.20</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>Welder2Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>11.36</SellPrice>
      <BuyPrice>1.21</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>Welder3Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>11.84</SellPrice>
      <BuyPrice>1.63</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>Welder4Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>12.16</SellPrice>
      <BuyPrice>1.92</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinderItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>11.92</SellPrice>
      <BuyPrice>1.71</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinder2Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>130.55</SellPrice>
      <BuyPrice>30.15</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinder3Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>140.83</SellPrice>
      <BuyPrice>20.47</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinder4Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>130.16</SellPrice>
      <BuyPrice>32.76</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>HandDrillItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>16.11</SellPrice>
      <BuyPrice>5.43</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>HandDrill2Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>13.73</SellPrice>
      <BuyPrice>3.32</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>HandDrill3Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>14.97</SellPrice>
      <BuyPrice>4.42</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>HandDrill4Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>14.97</SellPrice>
      <BuyPrice>4.42</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Scrap</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>0.13</SellPrice>
      <BuyPrice>0.11</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Scrap</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>0.13</SellPrice>
      <BuyPrice>0.11</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Ice</SubtypeName>
      <Quantity>10000000</Quantity>
      <SellPrice>0.337</SellPrice>
      <BuyPrice>0.299</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Organic</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>0.89</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_GasContainerObject</TypeId>
      <SubtypeName>HydrogenBottle</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>261.99</SellPrice>
      <BuyPrice>232.88</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Superconductor</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>180.84</SellPrice>
      <BuyPrice>160.75</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DesertTree</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DesertTreeDead</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>LeafTree</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>PineTree</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>PineTreeSnow</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>LeafTreeMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DesertTreeMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DesertTreeDeadMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>true</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>PineTreeSnowMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DeadBushMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>DesertBushMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>LeafBushMedium_var1</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>LeafBushMedium_var2</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>PineBushMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_TreeObject</TypeId>
      <SubtypeName>SnowPineBushMedium</SubtypeName>
      <Quantity>0</Quantity>
      <SellPrice>1</SellPrice>
      <BuyPrice>1</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_GasProperties</TypeId>
      <SubtypeName>Oxygen</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>0.035</SellPrice>
      <BuyPrice>0.033</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_GasProperties</TypeId>
      <SubtypeName>Hydrogen</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>0.03</SellPrice>
      <BuyPrice>0.025</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Component</TypeId>
      <SubtypeName>Canvas</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>152.91</SellPrice>
      <BuyPrice>135.92</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
  </MarketItems>
</Market>";

            // anything not in this Xml, will be added in via ValidateAndUpdateConfig() and SyncMarketItems().

            #endregion

            try
            {
                var items = MyAPIGateway.Utilities.SerializeFromXML<MarketStruct>(xmlText);
                config.DefaultPrices = items.MarketItems;
            }
            catch (Exception ex)
            {
                // This catches our stupidity and two left handed typing skills.
                // Check the Server logs to make sure this data loaded.
                EconomyScript.Instance.ServerLogger.WriteException(ex);
            }

            return config;
        }

        public static void SaveConfig(EconConfigStruct config)
        {
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(EconomyConsts.WorldStorageConfigFilename, typeof(EconConfigStruct));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<EconConfigStruct>(config));
            writer.Flush();
            writer.Close();
        }

        #endregion

        #region Load and save DATA

        public static EconDataStruct LoadData(List<MarketItemStruct> defaultPrices)
        {
            EconDataStruct data;
            string xmlText;

            // new file name and location.
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(EconomyConsts.WorldStorageDataFilename, typeof(EconDataStruct)))
            {
                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(EconomyConsts.WorldStorageDataFilename, typeof(EconDataStruct));
                xmlText = reader.ReadToEnd();
                reader.Close();
            }
            else
            {
                data = InitData();
                CheckDefaultMarket(data, defaultPrices);
                ValidateAndUpdateData(data, defaultPrices);
                return data;
            }

            if (string.IsNullOrWhiteSpace(xmlText))
            {
                data = InitData();
                CheckDefaultMarket(data, defaultPrices);
                ValidateAndUpdateData(data, defaultPrices);
                return data;
            }

            try
            {
                data = MyAPIGateway.Utilities.SerializeFromXML<EconDataStruct>(xmlText);
                EconomyScript.Instance.ServerLogger.WriteInfo("Loading existing EconDataStruct.");
            }
            catch
            {
                // data failed to deserialize.
                EconomyScript.Instance.ServerLogger.WriteError("Failed to deserialize EconDataStruct. Creating new EconDataStruct.");
                data = InitData();
            }

            CheckDefaultMarket(data, defaultPrices);
            ValidateAndUpdateData(data, defaultPrices);
            return data;
        }

        private static void CheckDefaultMarket(EconDataStruct data, List<MarketItemStruct> defaultPrices)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Checking Default Market Data.");

            var market = data.Markets.FirstOrDefault(m => m.MarketId == EconomyConsts.NpcMerchantId);
            if (market == null)
            {
                market = new MarketStruct
                {
                    Open = true,
                    MarketId = EconomyConsts.NpcMerchantId,
                    MarketZoneType = MarketZoneType.FixedSphere,
                    DisplayName = EconomyConsts.NpcMarketName,
                    MarketZoneSphere = new BoundingSphereD(Vector3D.Zero, EconomyScript.Instance.ServerConfig.DefaultTradeRange), // Center of the game world.
                    MarketItems = new List<MarketItemStruct>()
                };
                data.Markets.Add(market);
            }

            if (string.IsNullOrEmpty(market.DisplayName))
                market.DisplayName = EconomyConsts.NpcMarketName;

            AddMissingItemsToMarket(market, defaultPrices);
        }

        public static void CreateNpcMarket(string marketName, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating Npc Market.");

            var market = new MarketStruct
            {
                MarketId = EconomyConsts.NpcMerchantId,
                Open = true,
                DisplayName = marketName,
                MarketItems = new List<MarketItemStruct>(),
            };
            SetMarketShape(market, x, y, z, size, shape);

            AddMissingItemsToMarket(market, EconomyScript.Instance.ServerConfig.DefaultPrices);
            
            EconomyScript.Instance.Data.Markets.Add(market);
        }

        public static void CreateNpcMarket(string marketName, long entityId, decimal size)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating Npc Market.");

            var market = new MarketStruct
            {
                MarketId = EconomyConsts.NpcMerchantId,
                Open = true,
                DisplayName = marketName,
                MarketItems = new List<MarketItemStruct>(),
                MarketZoneType = MarketZoneType.EntitySphere,
                MarketZoneSphere = new BoundingSphereD(Vector3D.Zero, decimal.ToDouble(size)),
                EntityId = entityId
            };

            AddMissingItemsToMarket(market, EconomyScript.Instance.ServerConfig.DefaultPrices);

            EconomyScript.Instance.Data.Markets.Add(market);
        }

        public static void CreatePlayerMarket(ulong accountId, long entityId, double size, string blockCustomName)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating Player Market.");

            var market = new MarketStruct
            {
                MarketId = accountId,
                Open = false,
                EntityId = entityId,
                DisplayName = blockCustomName,
                MarketItems = new List<MarketItemStruct>(),
                MarketZoneType = MarketZoneType.EntitySphere,
                MarketZoneSphere = new BoundingSphereD(Vector3D.Zero, size)
            };

            AddMissingItemsToMarket(market, EconomyScript.Instance.ServerConfig.DefaultPrices);
            
            EconomyScript.Instance.Data.Markets.Add(market);
        }

        private static void AddMissingItemsToMarket(MarketStruct market, List<MarketItemStruct> defaultPrices)
        {
            // Add missing items that are covered by Default items, with 0 quantity.
            foreach (var defaultItem in defaultPrices)
            {
                var item = market.MarketItems.FirstOrDefault(e => e.TypeId.Equals(defaultItem.TypeId) && e.SubtypeName.Equals(defaultItem.SubtypeName));
                if (item == null)
                {
                    market.MarketItems.Add(new MarketItemStruct {
                        TypeId = defaultItem.TypeId,
                        SubtypeName = defaultItem.SubtypeName,
                        BuyPrice = defaultItem.BuyPrice,
                        SellPrice = defaultItem.SellPrice,
                        IsBlacklisted = defaultItem.IsBlacklisted,
                        Quantity = market.MarketId == EconomyConsts.NpcMerchantId ? defaultItem.Quantity : 0 });
                    EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding Default item: {0} {1}.", defaultItem.TypeId, defaultItem.SubtypeName);
                }
                else
                {
                    // Disable any blackmarket items.
                    if (defaultItem.IsBlacklisted)
                        item.IsBlacklisted = true;
                }
            }
        }

        public static void SetMarketShape(MarketStruct market, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            market.MarketZoneType = shape;
            switch (shape)
            {
                case MarketZoneType.FixedSphere:
                    market.MarketZoneSphere = new BoundingSphereD(new Vector3D((double)x, (double)y, (double)z), (double)size);
                    break;
                case MarketZoneType.FixedBox:
                    var sz = (double)(size / 2);
                    market.MarketZoneBox = new BoundingBoxD(new Vector3D((double)x - sz, (double)y - sz, (double)z - sz), new Vector3D((double)x + sz, (double)y + sz, (double)z + sz));
                    break;
            }
        }

        private static void ValidateAndUpdateData(EconDataStruct data, List<MarketItemStruct> defaultPrices)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Validating and Updating Data.");

            if (data.Accounts != null)
            {
                if (data.Accounts.Count != 0)
                {
                    if (data.Clients == null)
                        data.Clients = new List<ClientAccountStruct>();
                    foreach (BankAccountStruct account in data.Accounts)
                    {
                        data.Clients.Add(new ClientAccountStruct
                        {
                            SteamId = account.SteamId,
                            BankBalance = account.BankBalance,
                            Date = account.Date,
                            OpenedDate = account.OpenedDate,
                            MissionId = account.MissionId,
                            Language = account.Language,
                            NickName = account.NickName
                        });

                    }
                }
                data.Accounts = null;
            }

            foreach (ClientAccountStruct client in data.Clients)
            {
                if (client.ClientHudSettings == null)
                    client.ClientHudSettings = new ClientHudSettingsStruct();
            }

            foreach (var market in data.Markets)
            {
                AddMissingItemsToMarket(market, defaultPrices);
            }

            if (data.Missions == null)
                data.Missions = new List<MissionBaseStruct>();

            // Buy/Sell - check we have our NPC banker ready
            NpcMerchantManager.VerifyAndCreate(data);

            // Initial check of account on server load.
            AccountManager.CheckAccountExpiry(data);
        }

        private static EconDataStruct InitData()
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating new EconDataStruct.");
            EconDataStruct data = new EconDataStruct();
            data.Clients = new List<ClientAccountStruct>();
            data.Markets = new List<MarketStruct>();
            data.OrderBook = new List<OrderBookStruct>();
            data.ShipSale = new List<ShipSaleStruct>();
            data.Missions = new List<MissionBaseStruct>();
            return data;
        }

        public static void SaveData(EconDataStruct data)
        {
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(EconomyConsts.WorldStorageDataFilename, typeof(EconDataStruct));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<EconDataStruct>(data));
            writer.Flush();
            writer.Close();
        }

        #endregion

        #region Load and Save ReactivePricing

        /// <summary>
        /// Loads the Reactive Pricing data if it exists, or it creates a default table if data is empty (no rows).
        /// </summary>
        /// <returns></returns>
        public static ReactivePricingStruct LoadReactivePricing()
        {
            ReactivePricingStruct pricingData;
            string xmlText;

            // new file name and location.
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(EconomyConsts.WorldStoragePricescaleFilename, typeof(ReactivePricingStruct)))
            {
                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(EconomyConsts.WorldStoragePricescaleFilename, typeof(ReactivePricingStruct));
                xmlText = reader.ReadToEnd();
                reader.Close();
            }
            else
            {
                return InitPricing();
            }

            if (string.IsNullOrWhiteSpace(xmlText))
            {
                return InitPricing();
            }

            try
            {
                pricingData = MyAPIGateway.Utilities.SerializeFromXML<ReactivePricingStruct>(xmlText);
                EconomyScript.Instance.ServerLogger.WriteInfo("Loading existing ReactivePricingStruct.");
            }
            catch
            {
                // config failed to deserialize.
                EconomyScript.Instance.ServerLogger.WriteError("Failed to deserialize ReactivePricingStruct. Creating new ReactivePricingStruct.");
                pricingData = InitPricing();
            }

            if (pricingData == null || pricingData.Prices == null || pricingData.Prices.Count == 0)
                pricingData = InitPricing();

            return pricingData;
        }

        private static ReactivePricingStruct InitPricing()
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating new ReactivePricingStruct.");
            ReactivePricingStruct pricing = new ReactivePricingStruct();

            // This entire section should instead load / create the list at server start so that nothing but simple maths is performed in reactive pricing so there is 
            // negligable performance impact to lcds
            pricing.Prices.Add(new PricingStruct(10, 1.1m, "Critical stock 10 or less 10% increase"));
            pricing.Prices.Add(new PricingStruct(50, 1.1m, "Low Stock 11 to 50 10% increase"));
            pricing.Prices.Add(new PricingStruct(100, 1.05m, "Could be better 51 to 100 5% increase"));
            pricing.Prices.Add(new PricingStruct(1000, 1m, "About right 101 to 5000 no change"));
            pricing.Prices.Add(new PricingStruct(5000, 0.95m, "Bit high  5001- 10000 drop price 5%"));
            pricing.Prices.Add(new PricingStruct(10000, 0.95m, "Getting fuller 10001 to 50000 drop another 5%"));
            pricing.Prices.Add(new PricingStruct(50000, 0.90m, "Way too much now drop another 10%"));
            pricing.Prices.Add(new PricingStruct(100000, 0.50m, "Getting out of hand drop another 50%"));
            pricing.Prices.Add(new PricingStruct(200000, 0.25m, "Ok we need to dump this stock now 75% price drop"));
            return pricing;
        }

        public static void SaveReactivePricing(ReactivePricingStruct pricingData)
        {
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(EconomyConsts.WorldStoragePricescaleFilename, typeof(ReactivePricingStruct));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<ReactivePricingStruct>(pricingData));
            writer.Flush();
            writer.Close();
        }

        public static decimal PriceAdjust(decimal price, decimal onhand, PricingBias bias)
        {
            //
            // Summary:
            //     Takes specified price, and adjusts it based on given stock on hand using reactive price rules to
            //     give us a market buy or sell price that has reacted to overstocked or understocked goods.
            //     Can be called when buying(done), selling(done) or displaying prices on lcds(done) or /pricelist command(done) or /value(done) possibly /worth too (possible issues with performance)
            //     Bias favours price changes on sell or buy (done), need to factor in transaction size or item price somehow to avoid exploiting price points

            /* desired logic:
            0: presumably we run this server side since the player is unlikely to have the price file, if no file found create one
            1: either load the price adjust table from the pricetable file OR use a list/array which already contains the table
            2: compare the on hand value supplied with the values in the table if it matches a criteria adjust price and any other bias/protection we add
            3: repeat this until all table entries have been checked and/or applied as appropriate to slide the price up or down
            4: return the calculated price for output to player price list or for buy/sell transactions
            *** We also apply transport tycoon style subsides in here too.
            */



            //here is the meat and bones of the reactive pricing.  Any logic we use to slide the price up or down goes here  
            //This should prevent the sell (to player) price dropping below any possible reactive price change to the buy (from player) price
            //Ths should be all this logic needs to be production server safe. 
            var x = 0;
            do
            {
                if ((onhand > EconomyScript.Instance.ReactivePricing.Prices[x].PricePoint) && (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange < 1))  //price goes down
                {
                    if (bias == PricingBias.Buy) { price = price * (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange - 0.05m); } // Buy from player price bias too much stock
                     //else { price = price * (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange); } // Sell to player price disabled as this can potentially allow players to cheat
                }
                else
                {
                    if ((onhand <= EconomyScript.Instance.ReactivePricing.Prices[x].PricePoint) && (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange > 1)) //price goes up
                    {
                        if (bias == PricingBias.Sell) { price = price * (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange + 0.05m); } //Sell to player price bias low stock
                        //else { price = price * (EconomyScript.Instance.ReactivePricing.Prices[x].PriceChange); } // Buy from player price disabled as this can potentially allow players to cheat
                    }
                }
                x++;
            } while (x < EconomyScript.Instance.ReactivePricing.Prices.Count); //Avoid out of bounds errors

            return price;
        }

        #endregion
    }
}
