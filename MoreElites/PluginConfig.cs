using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace MoreElites
{
    public static class PluginConfig
    {
        public enum ConfigTier
        {
            Tier1 = 1,
            GuildedTier = 4,
            Tier2 = 5
        }

        internal static ConfigFile MEConfig;

        public static ConfigEntry<bool> enableEcho;
        public static ConfigEntry<ConfigTier> eliteTierEcho;

        public static ConfigEntry<bool> enableVolatile;
        public static ConfigEntry<ConfigTier> eliteTierVolatile;

        public static ConfigEntry<bool> enableEmpowering;
        public static ConfigEntry<ConfigTier> eliteTierEmpowering;

        public static ConfigEntry<bool> enableFrenzied;
        public static ConfigEntry<ConfigTier> eliteTierFrenzied;

        public static ConfigEntry<float> t1HealthMult;
        public static ConfigEntry<float> t1DamageMult;

        public static ConfigEntry<float> t1HonorHealthMult;
        public static ConfigEntry<float> t1HonorDamageMult;

        public static ConfigEntry<float> t2HealthMult;
        public static ConfigEntry<float> t2DamageMult;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            if (MoreElites.RooInstalled)
                InitRoO();

            MEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.MoreElites.cfg", true);

            // echo
            enableEcho = MEConfig.BindOption(
                "General",
                "Enable Echo",
                true,
                "Should enable the Echo Elite (Shadow Clone Elite)");
            eliteTierEcho = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Echo",
                ConfigTier.GuildedTier,
                "Sets the Elite Tier for the Echo Elite (Shadow Clone Elite). A good alt is GuildedTier (Stage 3 and later) or T2 (post-loop)");

            // volatile
            enableVolatile = MEConfig.BindOption(
                "General",
                "Enable Volatile",
                true,
                "Should enable the Volatile Elite (RoR1 Missile Elite)");
            eliteTierVolatile = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Volatile",
                ConfigTier.GuildedTier,
                "Sets the Elite Tier for the Volatile Elite (RoR1 Missile Elite). A good alt is GuidedTier (Stage 3 and later)");

            // empowering
            enableEmpowering = MEConfig.BindOption(
                "General",
                "Enable Empowering",
                true,
                "Should enable the Empowering Elite (Warbanner Elite)");
            eliteTierEmpowering = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Empowering",
                ConfigTier.Tier1,
                "Sets the Elite Tier for the Empowering Elite (Warbanner Elite)");

            // frenzied
            enableFrenzied = MEConfig.BindOption(
                "General",
                "Enable Frenzied",
                true,
                "Should enable the Frenzied Elite (RoR1 Elite)");
            eliteTierFrenzied = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Frenzied",
                ConfigTier.Tier1,
                "Sets the Elite Tier for the Frenzied Elite (RoR1 Elite)");

            // t1
            t1HealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Health Multiplier",
                4f,
                "Vanilla T1 is 4. A good alt is 3. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                1f, 10f);
            t1DamageMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Damage Multiplier",
                2f,
                "Vanilla T1 is 2. A good alt is 1.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                1f, 10f);

            // t1 honor
            t1HonorHealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Honor Health Multiplier",
                2.5f,
                "Vanilla T1 is 2.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                1f, 10f);
            t1HonorDamageMult = MEConfig.BindOptionSlider(
                "Stats", 
                "T1 Honor Damage Multiplier",
                1.5f,
                "Vanilla T1 is 1.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                1f, 10f);

            // t2
            t2HealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T2 Health Multiplier",
                18f,
                "Vanilla T2 is 18. A good alt is 12. Does not affect vanilla T2s. Ignored if elite reworks is installed.",
                1f, 30f);
            t2DamageMult = MEConfig.BindOptionSlider(
                "Stats",
                "T2 Damage Multiplier",
                6f,
                "Vanilla T2 is 6. A good alt is 3.5. Does not affect vanilla T2s. Ignored if elite reworks is installed.",
                1f, 10f);

            WipeConfig();
        }

        private static void WipeConfig()
        {
            PropertyInfo orphanedEntriesProp = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(MEConfig);
            orphanedEntries.Clear();

            MEConfig.Save();
        }

        #region Config Binding
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitRoO()
        {
            try
            {
                RiskOfOptions.ModSettingsManager.SetModDescription("More Elites", MoreElites.PluginGUID, MoreElites.PluginName);

                var iconStream = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(MoreElites.Instance.Info.Location), "icon.png"));
                var tex = new Texture2D(256, 256);
                tex.LoadImage(iconStream);
                var icon = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));

                RiskOfOptions.ModSettingsManager.SetModIcon(icon, MoreElites.PluginGUID, MoreElites.PluginName);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOption<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", bool restartRequired = true)
        {
            if (defaultValue is int or float)
            {
                return myConfig.BindOptionSlider(section, name, defaultValue, description, 0, 20, restartRequired);
            }
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, null));

            if (MoreElites.RooInstalled)
                TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<ConfigTier> BindOptionEnum(this ConfigFile myConfig, string section, string name, ConfigTier defaultValue, string description = "", bool restartRequired = true)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<ConfigTier>(ConfigTier.Tier1, ConfigTier.Tier2)));

            if (MoreElites.RooInstalled)
                TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOptionSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", float min = 0, float max = 20, bool restartRequired = true)
        {
            if (defaultValue is not int and not float)
            {
                return myConfig.BindOption(section, name, defaultValue, description, restartRequired);
            }

            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            AcceptableValueBase range = typeof(T) == typeof(int)
                ? new AcceptableValueRange<int>((int)min, (int)max)
                : new AcceptableValueRange<float>(min, max);

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, range));

            if (MoreElites.RooInstalled)
                TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOptionSteppedSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, float increment = 1f, string description = "", float min = 0, float max = 20, bool restartRequired = true)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<float>(min, max)));

            if (MoreElites.RooInstalled)
                TryRegisterOptionSteppedSlider(configEntry, increment, min, max, restartRequired);

            return configEntry;
        }
        #endregion

        #region RoO
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOption<T>(ConfigEntry<T> entry, bool restartRequired)
        {
            if (entry is ConfigEntry<string> stringEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(stringEntry, new RiskOfOptions.OptionConfigs.InputFieldConfig()
                {
                    submitOn = RiskOfOptions.OptionConfigs.InputFieldConfig.SubmitEnum.OnExitOrSubmit,
                    restartRequired = restartRequired
                }), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else if (entry is ConfigEntry<bool> boolEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(boolEntry, restartRequired), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else if (entry is ConfigEntry<KeyboardShortcut> shortCutEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(shortCutEntry, restartRequired), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else if (typeof(T).IsEnum)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(entry, restartRequired), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOption)}.");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOptionSlider<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, new RiskOfOptions.OptionConfigs.IntSliderConfig()
                {
                    min = (int)min,
                    max = (int)max,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = min,
                    max = max,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOptionSlider)}.");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOptionSteppedSlider<T>(ConfigEntry<T> entry, float increment, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(floatEntry, new RiskOfOptions.OptionConfigs.StepSliderConfig()
                {
                    increment = increment,
                    min = min,
                    max = max,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), MoreElites.PluginGUID, MoreElites.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOptionSteppedSlider)}.");
#endif
            }
        }
        #endregion
    }
}
