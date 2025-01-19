using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using static MoreElites.EliteBase;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MoreElites
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.BlightedElites", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.EliteReworks", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MoreElites : BaseUnityPlugin
    {
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "Nuxlar";
        public const string PluginName = "MoreElites";
        public const string PluginVersion = "1.1.5";

        public static MoreElites Instance { get; private set; }

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        public static bool BlightedInstalled => Chainloader.PluginInfos.ContainsKey("com.Moffein.BlightedElites");
        public static bool ReworksInstalled => Chainloader.PluginInfos.ContainsKey("com.Moffein.EliteReworks");

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);
            PluginConfig.Init();

            EliteBase.CreateElites();

            if (BlightedInstalled)
                AddBlightedCompat();
                
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddBlightedCompat()
        {
            foreach (var elite in EliteBase.EliteInstances)
            {
                if (elite?.CustomEliteDef?.EliteDef)
                {
                    switch (elite.EliteTierDef)
                    {
                        default:
                            break;
                        case EliteBase.EliteTier.T1:
                        case EliteBase.EliteTier.T1Guilded:
                            BlightedElites.Components.AffixBlightedComponent.tier1Affixes.Add(elite.CustomEliteDef.EliteDef);
                            break;
                        case EliteBase.EliteTier.T2:
                            BlightedElites.Components.AffixBlightedComponent.tier2Affixes.Add(elite.CustomEliteDef.EliteDef);
                            break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static float GetHealthBoostCoefficient(EliteBase.EliteTier tier, bool honor = false)
        {
            if (honor)
                return EliteReworks.Tweaks.ModifyEliteTiers.t1HonorHealth;

            return tier < EliteTier.T2 ? EliteReworks.Tweaks.ModifyEliteTiers.t1Health : EliteReworks.Tweaks.ModifyEliteTiers.t2Health;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static float GetDamageBoostCoefficient(EliteBase.EliteTier tier, bool honor = false)
        {
            if (honor)
                return EliteReworks.Tweaks.ModifyEliteTiers.t1HonorDamage;

            return tier < EliteTier.T2 ? EliteReworks.Tweaks.ModifyEliteTiers.t1Damage : EliteReworks.Tweaks.ModifyEliteTiers.t2Damage;
        }
    }
}