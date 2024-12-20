using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites.Elites
{
    public class Empowering : EliteBase<Empowering>
    {
        private GameObject EmpoweringWard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion().InstantiateClone("EmpoweringWard");

        public override string Name => "Empowering";
        public override string EquipmentName => "Empowering Aspect";
        public override string DescriptionText => "Aspect of Power";
        public override string PickupText => "Buffs the move and attack speed of all allies.";
        public override string LoreText => "Do the impossible, see the invisible\r\nRow! Row! Fight the power!\r\nTouch the untouchable, break the unbreakable\r\nRow! Row! Fight the power!";

        public override EliteTier EliteTierDefs => EliteTier.T1;
        public override Color EliteColor => new Color(1f, 0.5f, 0.0f);
        public override Texture2D EliteRamp => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMagmaWorm.png").WaitForCompletion();
        public override Sprite EliteIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffRing.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixEmpowering", false);

        public override void Init()
        {
            base.Init();

            EmpoweringWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = EliteMaterial;
            EmpoweringWard.GetComponent<BuffWard>().buffDef = RoR2Content.Buffs.Warbanner;

            Object.Destroy(EmpoweringWard.GetComponent<AkEvent>());
            Object.Destroy(EmpoweringWard.GetComponent<AkEvent>());
            Object.Destroy(EmpoweringWard.GetComponent<AkGameObj>());
        }

        public override void OnBuffGained(CharacterBody self)
        {
            var gameObject = Object.Instantiate(EmpoweringWard);
            gameObject.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
            gameObject.GetComponent<BuffWard>().Networkradius = 25f + self.radius;
            gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
        }

        public override void OnBuffLost(CharacterBody self)
        {
            var buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
            Object.Destroy(buffWard);
        }
    }
}
