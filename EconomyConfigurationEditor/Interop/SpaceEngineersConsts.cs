
namespace EconomyConfigurationEditor.Interop
{
    using System;
    using System.IO;
    using System.Reflection;
    using VRage.Utils;

    public class SpaceEngineersConsts
    {
        /// <summary>
        /// Thumbnail image of last position in save.
        /// </summary>
        public const string ThumbnailImageFilename = "thumb.jpg";

        /// <summary>
        /// Contains summary of save content filename.
        /// </summary>
        public const string SandBoxCheckpointFilename = "Sandbox.sbc";

        /// <summary>
        /// Contains Xml serialized main content filename.
        /// </summary>
        public const string SandBoxSectorFilename = "SANDBOX_0_0_0_.sbs";

        /// <summary>
        /// This is the file extension added to the normal filename for Sanbox files, changing the ".sbs" to ".sbsPB"
        /// </summary>
        public readonly static string ProtobuffersExtension = VRage.ObjectBuilders.MyObjectBuilderSerializer.ProtobufferExtension ?? "PB";

        public const byte EmptyVoxelMaterial = 0xff;

        // Current set max speed m/s for Ships.
        public const float MaxShipVelocity = 104.375f;

        // Current set max speed m/s for Players - as of update 01.023.
        public const float MaxPlayerVelocity = 111.531f;

        // Estimated max speed m/s for Meteors - as of update 01.024.
        public const float MaxMeteorVelocity = 202.812f;

        public const float PlayerMass = 100f;

        /// <summary>
        /// Converts the internal game value (mL) to the nominal metric (L) for display.
        /// </summary>
        public const float VolumeMultiplyer = 1000f;


        public const string BasePathName = "SpaceEngineers";
        public const string SavesFolder = "Saves";
        public const string ModsFolder = "Mods";
        public const string BlueprintsFolder = "Blueprints";
        public const string LocalBlueprintsSubFolder = "local";

        public static Version GetSEVersion()
        {
            try
            {
                return new Version(new MyVersion(GetSEVersionInt()).FormattedText.ToString().Replace("_", "."));
            }
            catch
            {
                return new Version();
            }
        }

        public static int GetSEVersionInt()
        {
            try
            {
                // SE_VERSION is a private constant. Need to use reflection to get it. 
                FieldInfo field = typeof(SpaceEngineers.Game.SpaceEngineersGame).GetField("SE_VERSION", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                return (int)field.GetValue(null);
            }
            catch
            {
                return 0;
            }
        }
    }
}
