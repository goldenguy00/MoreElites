using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;

namespace MoreElites
{
  [BepInPlugin("com.Nuxlar.MoreElites", "MoreElites", "0.8.0")]
  [BepInDependency(ItemAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(EliteAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]

  public class MoreElites : BaseUnityPlugin
  {
    public static ConfigEntry<bool> enableEcho;
    public static ConfigEntry<bool> enableEmpowering;
    public static ConfigEntry<bool> enableFrenzied;
    private static ConfigFile MEConfig { get; set; }

    public void Awake()
    {
      MEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.MoreElites.cfg", true);
      enableEcho = MEConfig.Bind<bool>("General", "Enable Echo", true, "Should enable the Echo Elite (Shadow Clone Elite)");
      enableEmpowering = MEConfig.Bind<bool>("General", "Enable Empowering", true, "Should enable the Empowering Elite (Warbanner Elite)");
      enableFrenzied = MEConfig.Bind<bool>("General", "Enable Frenzied", true, "Should enable the Frenzied Elite (RoR1 Elite)");

      if (enableEcho.Value)
        new Echo();
      if (enableEmpowering.Value)
        new Empowering();
      if (enableFrenzied.Value)
        new Frenzied();
    }

  }
}