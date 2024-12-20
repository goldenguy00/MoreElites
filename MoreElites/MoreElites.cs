using BepInEx;
using BepInEx.Configuration;
using System.Reflection;
using System.Collections.Generic;

namespace MoreElites
{
    [BepInPlugin("com.Nuxlar.MoreElites", "MoreElites", "1.0.4")]
    public class MoreElites : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableEcho;
        public static ConfigEntry<bool> enableVolatile;
        public static ConfigEntry<bool> enableEmpowering;
        public static ConfigEntry<bool> enableFrenzied;
        public static ConfigEntry<bool> upgradeVolatile;
        public static ConfigEntry<bool> upgradeEmpowering;
        public static ConfigEntry<bool> upgradeFrenzied;

        public static ConfigEntry<float> t1HealthMult;
        public static ConfigEntry<float> t1DamageMult;

        public static ConfigEntry<float> t1HonorHealthMult;
        public static ConfigEntry<float> t1HonorDamageMult;

        public static ConfigEntry<float> t2HealthMult;
        public static ConfigEntry<float> t2DamageMult;

        private static ConfigFile MEConfig { get; set; }

        public void Awake()
        {
            Log.Init(Logger);

            MEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.MoreElites.cfg", true);

            enableEcho = MEConfig.Bind<bool>("General", "Enable Echo", true, "Should enable the Echo Elite (Shadow Clone Elite)");
            
            enableVolatile = MEConfig.Bind<bool>("General", "Enable Volatile", true, "Should enable the Volatile Elite (RoR1 Missile Elite)");
            upgradeVolatile = MEConfig.Bind<bool>("General", "Upgrade Volatile", true, "Should the Volatile Elite (RoR1 Missile Elite) only appear on stage 3 and later");

            enableEmpowering = MEConfig.Bind<bool>("General", "Enable Empowering", true, "Should enable the Empowering Elite (Warbanner Elite)");
            upgradeEmpowering = MEConfig.Bind<bool>("General", "Upgrade Empowering", false, "Should the Empowering Elite (Warbanner Elite) only appear on stage 3 and later");
            
            enableFrenzied = MEConfig.Bind<bool>("General", "Enable Frenzied", true, "Should enable the Frenzied Elite (RoR1 Elite)");
            upgradeFrenzied = MEConfig.Bind<bool>("General", "Upgrade Frenzied", false, "Should the Frenzied Elite (RoR1 Elite) only appear on stage 3 and later");

            t1HealthMult = MEConfig.Bind<float>("General", "T1 Health Multiplier", 4f, "Vanilla T1 is 4. A good alt is 3. Does not affect vanilla T1s.");
            t1DamageMult = MEConfig.Bind<float>("General", "T1 Damage Multiplier", 2f, "Vanilla T1 is 2. A good alt is 1.5. Does not affect vanilla T1s.");

            t1HonorHealthMult = MEConfig.Bind<float>("General", "T1 Honor Health Multiplier", 2.5f, "Vanilla T1 is 2.5. Does not affect vanilla T1s.");
            t1HonorDamageMult = MEConfig.Bind<float>("General", "T1 Honor Damage Multiplier", 1.5f, "Vanilla T1 is 1.5. Does not affect vanilla T1s.");

            t2HealthMult = MEConfig.Bind<float>("General", "T2 Health Multiplier", 18f, "Vanilla T2 is 18. A good alt is 12. Does not affect vanilla T2s.");
            t2DamageMult = MEConfig.Bind<float>("General", "T2 Damage Multiplier", 6f, "Vanilla T2 is 6. A good alt is 3.5. Does not affect vanilla T2s.");

            WipeConfig(MEConfig);

            EliteBase.CreateElites();
        }

        private void WipeConfig(ConfigFile configFile)
        {
            PropertyInfo orphanedEntriesProp = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile);
            orphanedEntries.Clear();

            configFile.Save();
        }
    }
}