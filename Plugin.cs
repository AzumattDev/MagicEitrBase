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
        internal const string ModVersion = "1.1.8";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource MagicEitrBaseLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public enum Toggle
        {
            Off,
            On
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Skill_Divider = config("2 - Math Variables", "Skill Divider", 100.0f, "Divider applied to your magic skill level in the bonus calculation. A higher value will reduce the bonus gained per level.");

            Power_Amount = config("2 - Math Variables", "Power Amount", 2.0f, "Exponent used in the bonus calculation. Increasing this value makes the growth of the bonus non-linear.");

            Skill_Scalar = config("2 - Math Variables", "Skill Scalar", 100, "Multiplier applied to the result of the exponential calculation. Adjust this to scale the bonus amount.");

            Final_Multiplier = config("2 - Math Variables", "Final Multiplier", 1.0f, "Final multiplier applied to the calculated bonus. For example, a value of 2 doubles the bonus added per level.");

            LinearRegeneration = config("3 - Linear regeneration change", "Enabled", Toggle.On, "Toggle to enable linear adjustment of the Eitr regeneration rate. This adjusts the rate based on your current Eitr percentage, while keeping the overall regeneration time roughly constant.");
            LinearRegenerationMultiplier = config("3 - Linear regeneration change", "Multiplier", 3f, "Multiplier applied at 0% Eitr. Values above 1 make regeneration faster at low Eitr (and slower at high Eitr), while values below 1 have the opposite effect.");
            LinearRegenerationThreshold = config("3 - Linear regeneration change", "Regeneration threshold", 0.5f, "The Eitr percentage at which regeneration is considered 'normal.' Below this threshold, regeneration speeds up; above it, the rate slows down.");

            ChangeBaseEitrRegen = config("4 - Base Eitr Regen", "Enabled", Toggle.On, "Enable custom base Eitr regeneration. When enabled, the mod will replace the default regeneration rate and delay with your specified values.");
            ScaleBaseEitrRegenBasedOnSkill = config("4 - Base Eitr Regen", "Scale Based on Skill", Toggle.On, "When enabled, a bonus is added to the custom base regeneration rate based on your higher magic skill (Elemental or Blood Magic). This bonus is calculated using the math variables from Section 2.");
            BaseEitrRegen = config("4 - Base Eitr Regen", "Base Regeneration rate", 2f, "Custom base Eitr regeneration rate (units per second) that is used when custom regeneration is enabled.");
            BaseEitrRegenDelay = config("4 - Base Eitr Regen", "Base Regeneration delay", 2f, "Custom delay (in seconds) before Eitr starts regenerating when using the custom settings.");
            BaseEitrRegenBonusMultiplier = config("4 - Base Eitr Regen", "Regen Bonus Multiplier", 0.15f, "Multiplier for the skill-based bonus when applied to Eitr regeneration. Lower values reduce the bonus to avoid near-instant regeneration at high skill levels.");


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
        internal static ConfigEntry<float> Power_Amount = null!;
        internal static ConfigEntry<int> Skill_Scalar = null!;
        internal static ConfigEntry<float> Final_Multiplier = null!;
        internal static ConfigEntry<Toggle> LinearRegeneration = null!;
        internal static ConfigEntry<float> LinearRegenerationMultiplier = null!;
        internal static ConfigEntry<float> LinearRegenerationThreshold = null!;
        public static ConfigEntry<Toggle> ChangeBaseEitrRegen = null!;
        public static ConfigEntry<Toggle> ScaleBaseEitrRegenBasedOnSkill = null!;
        public static ConfigEntry<float> BaseEitrRegen = null!;
        public static ConfigEntry<float> BaseEitrRegenDelay = null!;
        public static ConfigEntry<float> BaseEitrRegenBonusMultiplier = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        #endregion
    }


    public static class ToggleExtensions
    {
        public static bool IsEnabled(this MagicEitrBasePlugin.Toggle toggle)
        {
            return toggle == MagicEitrBasePlugin.Toggle.On;
        }
    }
}