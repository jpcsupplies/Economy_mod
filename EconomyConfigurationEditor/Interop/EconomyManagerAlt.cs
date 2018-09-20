namespace Economy.scripts
{
    using Economy.scripts;
    using Economy.scripts.EconConfig;
    using Economy.scripts.EconStructures;
    using Economy.scripts.MissionStructures;
    using ProtoBuf;
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using VRageMath;

    public class EconomyManagerAlt
    {
        private const string ModStoragePath = "515710178.sbm_Economy.scripts";

        public bool LoadEconomy(string savePath)
        {
            var storagePath = Path.Combine(savePath, savePath);
            EconomyScript.Instance.ServerConfig = LoadConfig(storagePath);
            EconomyScript.Instance.ReactivePricing = LoadReactivePricing(storagePath);
            EconomyScript.Instance.Data = LoadData(storagePath, EconomyScript.Instance.ServerConfig.DefaultPrices);
            return true;
        }

        public bool SaveEconomy(string savePath)
        {
            //if (Data != null)
            //{
            //    EconDataManager.SaveData(Data);
            //}

            //if (ServerConfig != null)
            //{
            //    EconDataManager.SaveConfig(ServerConfig);
            //}

            //if (ReactivePricing != null)
            //{
            //    EconDataManager.SaveReactivePricing(ReactivePricing);
            //}



            // TODO: ....



            return false;
        }

        private static EconConfigStruct LoadConfig(string storagePath)
        {
            EconConfigStruct config;
            string xmlText;

            // new file name and location.
            string filename = Path.Combine(storagePath, "Storage", ModStoragePath, EconomyConsts.WorldStorageConfigFilename);
            if (File.Exists(filename))
            {
                xmlText = File.ReadAllText(filename);
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
                config = SerializeFromXML<EconConfigStruct>(xmlText);
            }
            catch
            {
                // config failed to deserialize.
                config = InitConfig();
            }

            if (config == null || config.DefaultPrices == null || config.DefaultPrices.Count == 0)
                config = InitConfig();

            ValidateAndUpdateConfig(config);
            return config;
        }

        private static EconConfigStruct InitConfig()
        {
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
                var items = SerializeFromXML<MarketStruct>(xmlText);
                config.DefaultPrices = items.MarketItems;
            }
            catch (Exception ex)
            {
                // This catches our stupidity and two left handed typing skills.
                // Check the Server logs to make sure this data loaded.
                //EconomyScript.Instance.ServerLogger.WriteException(ex);
            }

            return config;
        }

        private static void ValidateAndUpdateConfig(EconConfigStruct config)
        {
            // Sync in whatever is defined in the game (may contain new cubes, and modded cubes).
            SyncMarketItems(ref config.DefaultPrices);

            if (config.TradeTimeout.TotalSeconds < 1f)
            {
                config.TradeTimeout = new TimeSpan(0, 0, 1); // limit minimum trade timeout to 1 second.
            }
        }

        private static void SyncMarketItems(ref List<MarketItemStruct> marketItems)
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
                        // Need to add new items as Blacklisted.
                        marketItems.Add(new MarketItemStruct { TypeId = item.Id.TypeId.ToString(), SubtypeName = item.Id.SubtypeName, BuyPrice = 1, SellPrice = 1, IsBlacklisted = true });
                    }
                }
            }

            // get Gas Property Items.  MyObjectBuilder_GasProperties
            var gasItems = MyDefinitionManager.Static.GetGasDefinitions();

            foreach (var item in gasItems)
            {
                if (item.Public)
                {
                    // TypeId and SubtypeName are both Case sensitive. Do not Ignore case.
                    if (!marketItems.Any(e => e.TypeId.Equals(item.Id.TypeId.ToString()) && e.SubtypeName.Equals(item.Id.SubtypeName)))
                    {
                        // Need to add new items as Blacklisted.
                        marketItems.Add(new MarketItemStruct { TypeId = item.Id.TypeId.ToString(), SubtypeName = item.Id.SubtypeName, BuyPrice = 1, SellPrice = 1, IsBlacklisted = true });
                    }
                }
            }

            // TODO: make sure buy and sell work with correct value of Gas.
            // Bottles...
            // MyDefinitionId = MyOxygenContainerDefinition.StoredGasId;
            // maxVolume = MyOxygenContainerDefinition.Capacity;

            // Tanks... To be done later. it should work the same as bottles though.
            // MyDefinitionId = MyGasTankDefinition.StoredGasId;
        }

        private static ReactivePricingStruct LoadReactivePricing(string storagePath)
        {
            ReactivePricingStruct pricingData;
            string xmlText;

            // new file name and location.
            string filename = Path.Combine(storagePath, "Storage", ModStoragePath, EconomyConsts.WorldStoragePricescaleFilename);
            if (File.Exists(filename))
            {
                xmlText = File.ReadAllText(filename);
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
                pricingData = SerializeFromXML<ReactivePricingStruct>(xmlText);
            }
            catch
            {
                // config failed to deserialize.
                pricingData = InitPricing();
            }

            if (pricingData == null || pricingData.Prices == null || pricingData.Prices.Count == 0)
                pricingData = InitPricing();

            return pricingData;
        }

        private static ReactivePricingStruct InitPricing()
        {
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

        private static EconDataStruct LoadData(string storagePath, List<MarketItemStruct> defaultPrices)
        {
            EconDataStruct data;
            string xmlText;

            // new file name and location.
            string filename = Path.Combine(storagePath, "Storage", ModStoragePath, EconomyConsts.WorldStorageDataFilename);
            if (File.Exists(filename))
            {
                xmlText = File.ReadAllText(filename);
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
                data = SerializeFromXML<EconDataStruct>(xmlText);
                //EconomyScript.Instance.ServerLogger.WriteInfo("Loading existing EconDataStruct.");
            }
            catch
            {
                // data failed to deserialize.
                //EconomyScript.Instance.ServerLogger.WriteError("Failed to deserialize EconDataStruct. Creating new EconDataStruct.");
                data = InitData();
            }

            CheckDefaultMarket(data, defaultPrices);
            ValidateAndUpdateData(data, defaultPrices);
            return data;
        }

        private static EconDataStruct InitData()
        {
            //EconomyScript.Instance.ServerLogger.WriteInfo("Creating new EconDataStruct.");
            EconDataStruct data = new EconDataStruct();
            data.Clients = new List<ClientAccountStruct>();
            data.Markets = new List<MarketStruct>();
            data.OrderBook = new List<OrderBookStruct>();
            data.ShipSale = new List<ShipSaleStruct>();
            data.Missions = new List<MissionBaseStruct>();
            return data;
        }

        private static void CheckDefaultMarket(EconDataStruct data, List<MarketItemStruct> defaultPrices)
        {
            //EconomyScript.Instance.ServerLogger.WriteInfo("Checking Default Market Data.");

            var market = data.Markets.FirstOrDefault(m => m.MarketId == EconomyConsts.NpcMerchantId);
            if (market == null)
            {
                market = new MarketStruct
                {
                    MarketId = EconomyConsts.NpcMerchantId,
                    MarketZoneType = MarketZoneType.FixedSphere,
                    DisplayName = EconomyConsts.NpcMarketName,
                    MarketZoneSphere = new BoundingSphereD(Vector3D.Zero, EconomyConsts.DefaultTradeRange), // Center of the game world.
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
                    //EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding Default item: {0} {1}.", defaultItem.TypeId, defaultItem.SubtypeName);
                }
                else
                {
                    // Disable any blackmarket items.
                    if (defaultItem.IsBlacklisted)
                        item.IsBlacklisted = true;
                }
            }
        }

        private static void ValidateAndUpdateData(EconDataStruct data, List<MarketItemStruct> defaultPrices)
        {
            //EconomyScript.Instance.ServerLogger.WriteInfo("Validating and Updating Data.");

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
                        //EconomyScript.Instance.ServerLogger.WriteVerbose("MarketItem Adding Default item: {0} {1}.", defaultItem.TypeId, defaultItem.SubtypeName);
                    }
                    else
                    {
                        // Disable any blackmarket items.
                        if (defaultItem.IsBlacklisted)
                            item.IsBlacklisted = true;
                    }
                }
            }

            if (data.Missions == null)
                data.Missions = new List<MissionBaseStruct>();

            // Buy/Sell - check we have our NPC banker ready
            NpcMerchantManager.VerifyAndCreate(data);

            // Initial check of account on server load.
            AccountManager.CheckAccountExpiry(data);
        }

        #region Helpers

        // Sandbox.ModAPI.MyAPIUtilities
        private static byte[] SerializeToBinary<T>(T obj)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Serializer.Serialize<T>(memoryStream, obj);
                result = memoryStream.ToArray();
            }
            return result;
        }

        // Sandbox.ModAPI.MyAPIUtilities
        private static T SerializeFromBinary<T>(byte[] data)
        {
            T result;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                result = Serializer.Deserialize<T>(memoryStream);
            }
            return result;
        }

        // string IMyUtilities.SerializeToXML<T>(T objToSerialize)
        private static string SerializeToXML<T>(T objToSerialize)
        {
            try
            {
                var type = objToSerialize.GetType();
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(type);
                StringWriter textWriter = new StringWriter();
                x.Serialize(textWriter, objToSerialize);
                return textWriter.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static T SerializeFromXML<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            //serializer.UnknownElement += Serializer_UnknownElement;
            //serializer.UnknownAttribute += Serializer_UnknownAttribute;
            //serializer.UnknownNode += Serializer_UnknownNode;
            //serializer.UnreferencedObject += Serializer_UnreferencedObject;

            try
            {
                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader))
                    {
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
