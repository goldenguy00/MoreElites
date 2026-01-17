using R2API;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MoreElites
{
    public class Empowering : EliteBase<Empowering>
    {
        private GameObject EmpoweringWard = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_EliteHaunted.AffixHauntedWard_prefab).WaitForCompletion().InstantiateClone("EmpoweringWard");

        public override string Name => "Empowering";
        public override string EquipmentName => "Empowering Aspect";
        public override string DescriptionText => "Aspect of Empowering";
        public override string PickupText => "Buffs the move and attack speed of all allies.";
        public override string LoreText => "Do the impossible, see the invisible\r\nRow! Row! Fight the power!\r\nTouch the untouchable, break the unbreakable\r\nRow! Row! Fight the power!";

        public override VanillaEliteTier EliteTierEnum => (VanillaEliteTier)PluginConfig.eliteTierEmpowering.Value;
        public override Color EliteColor => new Color(1f, 0.5f, 0.0f);
        public override Texture2D EliteRamp { get; set; } = Addressables.LoadAssetAsync<Texture2D>(RoR2_Base_Common_ColorRamps.texRampMagmaWorm_png).WaitForCompletion();
        public override Sprite EliteIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_EliteIce.texBuffAffixWhite_tif).WaitForCompletion();
        public override Sprite AspectIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>(RoR2_DLC1_EliteEarth.texAffixEarthIcon_png).WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>(RoR2_Base_WardOnLevel.matWarbannerBuffRing_mat).WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_EliteFire.PickupEliteFire_prefab).WaitForCompletion().InstantiateClone("PickupAffixEmpowering", false);

        public override void Init()
        {
            base.Init();

            EmpoweringWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = EliteMaterial;
            EmpoweringWard.GetComponent<BuffWard>().buffDef = Addressables.LoadAssetAsync<BuffDef>(RoR2_Base_WardOnLevel.bdWarbanner_asset).WaitForCompletion();

            Object.Destroy(EmpoweringWard.GetComponent<AkEvent>());
            Object.Destroy(EmpoweringWard.GetComponent<AkEvent>());
            Object.Destroy(EmpoweringWard.GetComponent<AkGameObj>());
        }

        public override void OnBuffGained(CharacterBody self) => self.AddItemBehavior<AffixEmpoweringBehavior>(1);
        public override void OnBuffLost(CharacterBody self) => self.AddItemBehavior<AffixEmpoweringBehavior>(0);


        public class AffixEmpoweringBehavior : CharacterBody.ItemBehavior
        {
            public GameObject affixEmpoweringWardInstance;

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                var hasItem = stack > 0;
                if (affixEmpoweringWardInstance != hasItem)
                {
                    if (hasItem)
                    {
                        affixEmpoweringWardInstance = Instantiate(Instance.EmpoweringWard);
                        affixEmpoweringWardInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                        affixEmpoweringWardInstance.GetComponent<BuffWard>().Networkradius = 25f + body.radius;
                        affixEmpoweringWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                    }
                    else
                    {
                        Destroy(affixEmpoweringWardInstance);
                        affixEmpoweringWardInstance = null;
                    }
                }
            }

            public void OnDisable()
            {
                if (affixEmpoweringWardInstance)
                    Destroy(affixEmpoweringWardInstance);
            }
        }
    }
}
