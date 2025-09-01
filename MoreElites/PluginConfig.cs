using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using MiscFixes.Modules;
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
                "Should enable the Echo Elite (Shadow Clone Elite)",
                true);

            eliteTierEcho = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Echo",
                ConfigTier.GuildedTier,
                "Sets the Elite Tier for the Echo Elite (Shadow Clone Elite). A good alt is GuildedTier (Stage 3 and later) or T2 (post-loop)");

            // volatile
            enableVolatile = MEConfig.BindOption(
                "General",
                "Enable Volatile",
                "Should enable the Volatile Elite (RoR1 Missile Elite)",
                true);
            eliteTierVolatile = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Volatile",
                ConfigTier.GuildedTier,
                "Sets the Elite Tier for the Volatile Elite (RoR1 Missile Elite). A good alt is GuidedTier (Stage 3 and later)");

            // empowering
            enableEmpowering = MEConfig.BindOption(
                "General",
                "Enable Empowering",
                "Should enable the Empowering Elite (Warbanner Elite)",
                true);
            eliteTierEmpowering = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Empowering",
                ConfigTier.Tier1,
                "Sets the Elite Tier for the Empowering Elite (Warbanner Elite)");

            // frenzied
            enableFrenzied = MEConfig.BindOption(
                "General",
                "Enable Frenzied",
                "Should enable the Frenzied Elite (RoR1 Elite)",
                true);
            eliteTierFrenzied = MEConfig.BindOptionEnum(
                "General",
                "Elite Tier Frenzied",
                ConfigTier.Tier1,
                "Sets the Elite Tier for the Frenzied Elite (RoR1 Elite)");

            // t1
            t1HealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Health Multiplier",
                "Vanilla T1 is 4. A good alt is 3. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                4f,
                1f, 10f);
            t1DamageMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Damage Multiplier",
                "Vanilla T1 is 2. A good alt is 1.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                2f,
                1f, 10f);

            // t1 honor
            t1HonorHealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T1 Honor Health Multiplier",
                "Vanilla T1 is 2.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                2.5f,
                1f, 10f);
            t1HonorDamageMult = MEConfig.BindOptionSlider(
                "Stats", 
                "T1 Honor Damage Multiplier",
                "Vanilla T1 is 1.5. Does not affect vanilla T1s. Ignored if elite reworks is installed.",
                1.5f,
                1f, 10f);

            // t2
            t2HealthMult = MEConfig.BindOptionSlider(
                "Stats",
                "T2 Health Multiplier",
                "Vanilla T2 is 18. A good alt is 12. Does not affect vanilla T2s. Ignored if elite reworks is installed.",
                18f,
                1f, 30f);
            t2DamageMult = MEConfig.BindOptionSlider(
                "Stats",
                "T2 Damage Multiplier",
                "Vanilla T2 is 6. A good alt is 3.5. Does not affect vanilla T2s. Ignored if elite reworks is installed.",
                6f,
                1f, 10f);

            WipeConfig();
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
        public static ConfigEntry<ConfigTier> BindOptionEnum(this ConfigFile myConfig, string section, string name, ConfigTier defaultValue, string description = "", bool restartRequired = true)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<ConfigTier>(ConfigTier.Tier1, ConfigTier.Tier2)));

            if (MoreElites.RooInstalled)
                Extensions.TryRegisterOption(configEntry, restartRequired, MoreElites.PluginGUID, MoreElites.PluginName);

            return configEntry;
        }
        #endregion
    }
}
