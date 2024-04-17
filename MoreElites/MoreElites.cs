using BepInEx;
using BepInEx.Configuration;
using System.Reflection;
using System.Collections.Generic;
using R2API;

namespace MoreElites
{
  [BepInPlugin("com.Nuxlar.MoreElites", "MoreElites", "1.0.1")]
  [BepInDependency(ItemAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(EliteAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]

  public class MoreElites : BaseUnityPlugin
  {
    public static ConfigEntry<bool> enableEcho;
    public static ConfigEntry<bool> enableVolatile;
    public static ConfigEntry<bool> enableEmpowering;
    public static ConfigEntry<bool> enableFrenzied;
    public static ConfigEntry<float> t2HealthMult;
    public static ConfigEntry<float> t2DamageMult;
    private static ConfigFile MEConfig { get; set; }

    public void Awake()
    {
      MEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.MoreElites.cfg", true);
      enableEcho = MEConfig.Bind<bool>("General", "Enable Echo", true, "Should enable the Echo Elite (Shadow Clone Elite)");
      enableVolatile = MEConfig.Bind<bool>("General", "Enable Volatile", true, "Should enable the Volatile Elite (RoR1 Missile Elite)");
      enableEmpowering = MEConfig.Bind<bool>("General", "Enable Empowering", true, "Should enable the Empowering Elite (Warbanner Elite)");
      enableFrenzied = MEConfig.Bind<bool>("General", "Enable Frenzied", true, "Should enable the Frenzied Elite (RoR1 Elite)");
      t2HealthMult = MEConfig.Bind<float>("General", "T2 Health Multiplier", 18f, "Vanilla T2 is 18. A good alt is 12. Does not affect vanilla T2s.");
      t2DamageMult = MEConfig.Bind<float>("General", "T2 Damage Multiplier", 6f, "Vanilla T2 is 6. A good alt is 3.5. Does not affect vanilla T2s.");
      WipeConfig(MEConfig);

      if (enableEcho.Value)
        new Echo();
      if (enableEmpowering.Value)
        new Empowering();
      if (enableFrenzied.Value)
        new Frenzied();
      if (enableVolatile.Value)
        new Volatile();
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