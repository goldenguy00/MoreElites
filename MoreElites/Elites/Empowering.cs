using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MoreElites
{
    public class Empowering : EliteBase<Empowering>
    {
        private GameObject EmpoweringWard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion().InstantiateClone("EmpoweringWard");

        public override string Name => "Empowering";
        public override string EquipmentName => "Empowering Aspect";
        public override string DescriptionText => "Aspect of Empowering";
        public override string PickupText => "Buffs the move and attack speed of all allies.";
        public override string LoreText => "Do the impossible, see the invisible\r\nRow! Row! Fight the power!\r\nTouch the untouchable, break the unbreakable\r\nRow! Row! Fight the power!";

        public override EliteTier EliteTierDef => (EliteTier)PluginConfig.eliteTierEmpowering.Value;
        public override Color EliteColor => new Color(1f, 0.5f, 0.0f);
        public override Texture2D EliteRamp { get; set; } = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMagmaWorm.png").WaitForCompletion();
        public override Sprite EliteIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffRing.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixEmpowering", false);

        public override void Init()
        {
            base.Init();

            EmpoweringWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = EliteMaterial;
            EmpoweringWard.GetComponent<BuffWard>().buffDef = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/WardOnLevel/bdWarbanner.asset").WaitForCompletion();

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
