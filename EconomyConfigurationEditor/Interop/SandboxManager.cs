namespace EconomyConfigurationEditor
{
    using EconomyConfigurationEditor.Interop;
    using EconomyConfigurationEditor.Support;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using SpaceEngineers.Game;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;
    using VRageRender;

    public class SandboxManager
    {
        public bool LoadGame(string seBinPath, string userDataPath, string savePath)
        {
            if (!Directory.Exists(seBinPath))
                return false;

            if (!Directory.Exists(savePath))
                return false;

            if (!savePath.StartsWith(userDataPath, StringComparison.OrdinalIgnoreCase))
                return false;

            string contentPath = Path.GetFullPath(Path.Combine(seBinPath, @"..\Content"));

            MyFileSystem.Reset();
            MyFileSystem.Init(contentPath, userDataPath);

            MyLog.Default = MySandboxGame.Log;
            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            MyFileSystem.InitUserSpecific(null);

            SpaceEngineersGame.SetupPerGameSettings();

            VRageRender.MyRenderProxy.Initialize(new MyNullRender());
            // We create a whole instance of MySandboxGame!
            // If this is causing an exception, then there is a missing dependency.
            MySandboxGame gameTemp = new MySandboxGame(null);

            #region Game Localization

            // creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);

            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var languageTag = culture.IetfLanguageTag;
            var localizationPath = Path.Combine(contentPath, @"Data\Localization");
            var codes = languageTag.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var maincode = codes.Length > 0 ? codes[0] : null;
            var subcode = codes.Length > 1 ? codes[1] : null;
            MyTexts.Clear();
            MyTexts.LoadTexts(localizationPath, maincode, subcode);

            #endregion

            MyStorageBase.UseStorageCache = false;

            #region MySession creation

            // Replace the private constructor on MySession, so we can create it without getting involed with Havok and other depdancies.
            var keenStart = typeof(Sandbox.Game.World.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
            var ourStart = typeof(EconomyConfigurationEditor.Interop.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
            ReflectionUtil.ReplaceMethod(ourStart, keenStart);

            // Create an empty instance of MySession for use by low level code.
            Sandbox.Game.World.MySession mySession = ReflectionUtil.ConstructPrivateClass<Sandbox.Game.World.MySession>(new Type[0], new object[0]);
            ReflectionUtil.ConstructField(mySession, "m_sessionComponents"); // Required as the above code doesn't populate it during ctor of MySession.
            mySession.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };

            // Assign the instance back to the static.
            Sandbox.Game.World.MySession.Static = mySession;

            Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static = new MyHeightMapLoadingSystem();
            Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static.LoadData();

            #endregion

            #region Load Sandbox

            var filename = Path.Combine(savePath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            MyObjectBuilder_Checkpoint checkpoint;
            string errorInformation;
            bool compressedCheckpointFormat;
            bool snapshot = false;
            bool retVal = SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Checkpoint>(filename, out checkpoint, out compressedCheckpointFormat, out errorInformation, snapshot);

            if (!retVal)
                return false;

            #endregion

            MyDefinitionManager.Static.PrepareBaseDefinitions();
            MyDefinitionManager.Static.LoadData(checkpoint.Mods);
            var MaterialIndex = new Dictionary<string, byte>();

            return true;
        }
    }
}
