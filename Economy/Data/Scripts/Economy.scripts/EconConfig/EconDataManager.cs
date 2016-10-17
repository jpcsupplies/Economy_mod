﻿namespace Economy.scripts.EconConfig
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Economy.scripts.EconStructures;
    using Sandbox.ModAPI;
    using VRageMath;

    public static class ReactivePricing {
        public static decimal PriceAdjust(decimal price, decimal onhand)
        {
            //
            // Summary:
            //     Takes specified price, and adjusts it based on given stock on hand using reactive price rules to
            //     give us a market buy or sell price that has reacted to overstocked or understocked goods.
            //     Can be called when buying, selling or displaying prices on lcds or /pricelist command or /value possibly /worth too

            //just a placeholder at the moment return test value
            return 69;
        }
    }

    public static class EconDataManager
    {
        // probably should load our reactive pricing rules here somewhere
        #region Load and save CONFIG

        public static string GetConfigFilename()
        {
            return string.Format("EconomyConfig_{0}.xml", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        }

        public static EconConfigStruct LoadConfig()
        {
            string filename = GetConfigFilename();
            EconConfigStruct config = null;

            if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(EconConfigStruct)))
            {
                config = InitConfig();
                ValidateAndUpdateConfig(config);
                return config;
            }

            TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(EconConfigStruct));

            var xmlText = reader.ReadToEnd();
            reader.Close();

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
      <SellPrice>52.19</SellPrice>
      <BuyPrice>58.72</BuyPrice>
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
      <SellPrice>0.05</SellPrice>
      <BuyPrice>0.04</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ore</TypeId>
      <SubtypeName>Uranium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>0.07</SellPrice>
      <BuyPrice>0.06</BuyPrice>
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
      <SellPrice>9.10</SellPrice>
      <BuyPrice>8.09</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Gold</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>9.87</SellPrice>
      <BuyPrice>8.77</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Platinum</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>12.37</SellPrice>
      <BuyPrice>11</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_Ingot</TypeId>
      <SubtypeName>Uranium</SubtypeName>
      <Quantity>1000</Quantity>
      <SellPrice>12.36</SellPrice>
      <BuyPrice>10.99</BuyPrice>
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
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>PreciseAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>12.84</SellPrice>
      <BuyPrice>2.52</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>RapidFireAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>13.43</SellPrice>
      <BuyPrice>3.05</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>UltimateAutomaticRifleItem</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>15.94</SellPrice>
      <BuyPrice>5.28</BuyPrice>
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
      <SellPrice>13.55</SellPrice>
      <BuyPrice>3.15</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinder3Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>12.83</SellPrice>
      <BuyPrice>2.47</BuyPrice>
      <IsBlacklisted>false</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_PhysicalGunObject</TypeId>
      <SubtypeName>AngleGrinder4Item</SubtypeName>
      <Quantity>100</Quantity>
      <SellPrice>13.16</SellPrice>
      <BuyPrice>2.76</BuyPrice>
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
      <Quantity>10000</Quantity>
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
      <SellPrice>10.11</SellPrice>
      <BuyPrice>8.97</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
    </MarketItem>
    <MarketItem>
      <TypeId>MyObjectBuilder_GasProperties</TypeId>
      <SubtypeName>Hydrogen</SubtypeName>
      <Quantity>10000</Quantity>
      <SellPrice>10.11</SellPrice>
      <BuyPrice>8.97</BuyPrice>
      <IsBlacklisted>true</IsBlacklisted>
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
            string filename = GetConfigFilename();
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(EconConfigStruct));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<EconConfigStruct>(config));
            writer.Flush();
            writer.Close();
        }

        #endregion

        #region Load and save DATA

        public static string GetDataFilename()
        {
            return string.Format("EconomyData_{0}.xml", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));
        }


 

        public static EconDataStruct LoadData(List<MarketItemStruct> defaultPrices)
        {
            string filename = GetDataFilename();
            EconDataStruct data = null;

            if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(EconDataStruct)))
            {
                data = InitData();
                CheckDefaultMarket(data, defaultPrices);
                ValidateAndUpdateData(data, defaultPrices);
                return data;
            }

            TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(EconDataStruct));

            var xmlText = reader.ReadToEnd();
            reader.Close();

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

            // Add missing items that are covered by Default items.
            foreach (var defaultItem in defaultPrices)
            {
                var item = market.MarketItems.FirstOrDefault(e => e.TypeId.Equals(defaultItem.TypeId) && e.SubtypeName.Equals(defaultItem.SubtypeName));
                if (item == null)
                {
                    market.MarketItems.Add(new MarketItemStruct { TypeId = defaultItem.TypeId, SubtypeName = defaultItem.SubtypeName, BuyPrice = defaultItem.BuyPrice, SellPrice = defaultItem.SellPrice, IsBlacklisted = defaultItem.IsBlacklisted, Quantity = defaultItem.Quantity });
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

            // Add missing items that are covered by Default items.
            foreach (var defaultItem in EconomyScript.Instance.ServerConfig.DefaultPrices)
            {
                var item = market.MarketItems.FirstOrDefault(e => e.TypeId.Equals(defaultItem.TypeId) && e.SubtypeName.Equals(defaultItem.SubtypeName));
                if (item == null)
                {
                    market.MarketItems.Add(new MarketItemStruct { TypeId = defaultItem.TypeId, SubtypeName = defaultItem.SubtypeName, BuyPrice = defaultItem.BuyPrice, SellPrice = defaultItem.SellPrice, IsBlacklisted = defaultItem.IsBlacklisted, Quantity = defaultItem.Quantity });
                    EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding Default item: {0} {1}.", defaultItem.TypeId, defaultItem.SubtypeName);
                }
                else
                {
                    // Disable any blackmarket items.
                    if (defaultItem.IsBlacklisted)
                        item.IsBlacklisted = true;
                }
            }

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

            // Add missing items that are covered by Default items, with 0 quantity.
            foreach (var defaultItem in EconomyScript.Instance.ServerConfig.DefaultPrices)
            {
                var item = market.MarketItems.FirstOrDefault(e => e.TypeId.Equals(defaultItem.TypeId) && e.SubtypeName.Equals(defaultItem.SubtypeName));
                if (item == null)
                {
                    market.MarketItems.Add(new MarketItemStruct { TypeId = defaultItem.TypeId, SubtypeName = defaultItem.SubtypeName, BuyPrice = defaultItem.BuyPrice, SellPrice = defaultItem.SellPrice, IsBlacklisted = defaultItem.IsBlacklisted, Quantity = 0 });
                    EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding Default item: {0} {1}.", defaultItem.TypeId, defaultItem.SubtypeName);
                }
                else
                {
                    // Disable any blackmarket items.
                    if (defaultItem.IsBlacklisted)
                        item.IsBlacklisted = true;
                }
            }

            EconomyScript.Instance.Data.Markets.Add(market);
        }

        public static void SetMarketShape(MarketStruct market, decimal x, decimal y, decimal z, decimal size, MarketZoneType shape)
        {
            market.MarketZoneType = shape;
            switch (shape)
            {
                case MarketZoneType.FixedSphere:
                    market.MarketZoneSphere = new BoundingSphereD(new Vector3D((double) x, (double) y, (double) z), (double) size);
                    break;
                case MarketZoneType.FixedBox:
                    var sz = (double)(size / 2);
                    market.MarketZoneBox = new BoundingBoxD(new Vector3D((double) x - sz, (double) y - sz, (double) z - sz), new Vector3D((double) x + sz, (double) y + sz, (double) z + sz));
                    break;
            }
        }

        private static void ValidateAndUpdateData(EconDataStruct data, List<MarketItemStruct> defaultPrices)
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Validating and Updating Data.");

            // Add missing items that are covered by Default items.
            foreach (var defaultItem in defaultPrices)
            {
                foreach (var market in data.Markets)
                {
                    var item = market.MarketItems.FirstOrDefault(e => e.TypeId.Equals(defaultItem.TypeId) && e.SubtypeName.Equals(defaultItem.SubtypeName));
                    var isNpcMerchant = market.MarketId == EconomyConsts.NpcMerchantId; // make sure no stock is added to player markets.

                    // TODO: remove this later. It's a temporary fix to setup the new Open property.
                    // Added 01.125.
                    if (isNpcMerchant)
                        market.Open = true;

                    if (item == null)
                    {
                        market.MarketItems.Add(new MarketItemStruct { TypeId = defaultItem.TypeId, SubtypeName = defaultItem.SubtypeName, BuyPrice = defaultItem.BuyPrice, SellPrice = defaultItem.SellPrice, IsBlacklisted = defaultItem.IsBlacklisted, Quantity = isNpcMerchant ? defaultItem.Quantity : 0 });
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

            // Buy/Sell - check we have our NPC banker ready
            NpcMerchantManager.VerifyAndCreate(data);

            // Initial check of account on server load.
            AccountManager.CheckAccountExpiry(data);
        }

        private static EconDataStruct InitData()
        {
            EconomyScript.Instance.ServerLogger.WriteInfo("Creating new EconDataStruct.");
            EconDataStruct data = new EconDataStruct();
            data.Accounts = new List<BankAccountStruct>();
            data.Markets = new List<MarketStruct>();
            data.OrderBook = new List<OrderBookStruct>();
            return data;
        }

        public static void SaveData(EconDataStruct data)
        {
            string filename = GetDataFilename();
            TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(EconDataStruct));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML<EconDataStruct>(data));
            writer.Flush();
            writer.Close();
        }

        #endregion
    }
}
