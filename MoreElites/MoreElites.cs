using BepInEx;
using BepInEx.Bootstrap;
using System.Security.Permissions;
using System.Security;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MoreElites
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MoreElites : BaseUnityPlugin
    {
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "Nuxlar";
        public const string PluginName = "MoreElites";
        public const string PluginVersion = "1.0.4";

        public static MoreElites Instance { get; private set; }

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);
            PluginConfig.Init();

            EliteBase.CreateElites();
        }
    }
}