using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace MagicEitrBase
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInIncompatibility("Azumatt.AllTheBases")]
    public class MagicEitrBasePlugin : BaseUnityPlugin
    {
        internal const string ModName = "MagicEitrBase";
        internal const string ModVersion = "1.1.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource MagicEitrBaseLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public enum Toggle
        {
            Off,
            On
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Skill_Divider = config("2 - Math Variables", "Skill Divider", 100.0f,
                "The skill divider used in the calculation of the base eitr.");

            Power_Amount = config("2 - Math Variables", "Power Amount", 2,
                "The power amount used in the calculation of the base eitr.");


            Skill_Scalar = config("2 - Math Variables", "Skill Scalar", 100,
                "The skill scalar used in the calculation of the base eitr.");


            Final_Multiplier = config("2 - Math Variables", "Final Multiplier", 1.0f,
                "The final multiplier used in the calculation of the base eitr. This will multiply the result from the math used in the eitr calculation. Increasing this, will increase the result. Ex. A value of 2 will double the result.");


            SetupWatcher();
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                MagicEitrBaseLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                MagicEitrBaseLogger.LogError($"There was an issue loading your {ConfigFileName}");
                MagicEitrBaseLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        internal static ConfigEntry<float> Skill_Divider = null!;
        internal static ConfigEntry<int> Power_Amount = null!;
        internal static ConfigEntry<int> Skill_Scalar = null!;
        internal static ConfigEntry<float> Final_Multiplier = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        class AcceptableShortcuts : AcceptableValueBase // Used for KeyboardShortcut Configs 
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion
    }
}