using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using RoR2;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MoreElites
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MoreElites : BaseUnityPlugin
    {
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "Nuxlar";
        public const string PluginName = "MoreElites";
        public const string PluginVersion = "1.2.1";

        public static MoreElites Instance { get; private set; }

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        //public static bool BlightedInstalled => Chainloader.PluginInfos.ContainsKey("com.Moffein.BlightedElites");

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);
            PluginConfig.Init();

            if (PluginConfig.enableEcho.Value)
                new Echo();
            if (PluginConfig.enableEmpowering.Value)
                new Empowering();
            if (PluginConfig.enableFrenzied.Value)
                new Frenzied();
            if (PluginConfig.enableVolatile.Value)
                new Volatile();

            EliteBase.CreateElites();
        }
        /*
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddBlightedCompat()
        {
            if (BlightedInstalled)
                AddEliteTypes();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddEliteTypes()
        {
            foreach (var elite in EliteBase.EliteInstances)
            {
                if (elite?.CustomEliteDef?.EliteDef && elite.CustomEliteDef != Echo.Instance.CustomEliteDef)
                {
                    switch (elite.EliteTierDef)
                    {
                        default:
                            break;
                        case EliteBase.EliteTier.T1:
                        case EliteBase.EliteTier.T1Guilded:
                            AddT1(elite.CustomEliteDef.EliteDef);
                            break;
                        case EliteBase.EliteTier.T2:
                            AddT2(elite.CustomEliteDef.EliteDef);
                            break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddT1(EliteDef eliteDef)
        {
            BlightedElites.Components.AffixBlightedComponent.tier1Affixes.Add(eliteDef);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddT2(EliteDef eliteDef)
        {
            BlightedElites.Components.AffixBlightedComponent.tier2Affixes.Add(eliteDef);
        }*/
    }
}