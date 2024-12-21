using R2API;
using RoR2;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
    public class Frenzied : EliteBase<Frenzied>
    {
        public override string Name => "FrenziedNuxlar";
        public override string EquipmentName => "Frenzied Aspect";
        public override string PickupText => "Aspect of the Frenzied Flame";
        public override string DescriptionText => "Increased move and attack speed";
        public override string LoreText => "LET CHAOS BURN THE WORLD";

        public override EliteTier EliteTierDef => (EliteTier)PluginConfig.eliteTierFrenzied.Value;
        public override Color EliteColor => Color.yellow;
        public override Texture2D EliteRamp => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWarbanner2.png").WaitForCompletion();
        public override Sprite EliteIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerSphereIndicator.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixFrenzied", false);

        public override void Init()
        {
            base.Init();

            RecalculateStatsAPI.GetStatCoefficients += Frenzy;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        public override EquipmentDef SetupEquipment()
        {
            var def = base.SetupEquipment();
            def.cooldown = 10f;

            return def;
        }

        public override void OnBuffGained(CharacterBody self) => self.AddItemBehavior<FrenziedTeleportController>(1);
        public override void OnBuffLost(CharacterBody self) => self.AddItemBehavior<FrenziedTeleportController>(0);

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            var result = orig(self, equipmentDef);
            if (!result && equipmentDef == EliteEquipmentDef)
            {
                if (self.characterBody && self.characterBody.isPlayerControlled && self.characterBody.TryGetComponent<FrenziedTeleportController>(out var teleporter))
                    teleporter.StartCoroutine(teleporter.Teleport());
                return true;
            }
            return result;
        }

        private void Frenzy(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(EliteBuffDef))
            {
                args.baseMoveSpeedAdd += 2f;
                args.attackSpeedMultAdd += 0.5f;
            }
        }

        public class FrenziedTeleportController : CharacterBody.ItemBehavior
        {
            private static float fireInterval = 10f;
            private static GameObject blinkPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Treebot/SonicBoomEffect.prefab").WaitForCompletion();
            private static float shortBlinkDistance = 25f;
            private static float blinkDistance = 50f;

            private float fireTimer;
            private Vector3 blinkDestination = Vector3.zero;
            private Vector3 blinkStart = Vector3.zero;

            private void FixedUpdate()
            {
                if (this.body.isPlayerControlled || !this.body.healthComponent || !this.body.healthComponent.alive)
                    return;

                this.fireTimer += Time.fixedDeltaTime;
                if (this.fireTimer >= fireInterval)
                {
                    this.fireTimer = 0; 

                    StartCoroutine(Teleport());
                }
            }

            public IEnumerator Teleport()
            {
                CalculateBlinkDestination();

                if (this.blinkStart != this.blinkDestination)
                {
                    Util.PlaySound("Play_parent_teleport", this.gameObject);
                    this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));

                    TeleportHelper.TeleportBody(this.body, this.blinkDestination, false);

                    yield return new WaitForSeconds(0.33f);

                    this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
                }
            }

            private void CreateBlinkEffect(Vector3 origin)
            {
                if (blinkPrefab)
                {
                    EffectManager.SpawnEffect(blinkPrefab, new EffectData()
                    {
                        rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart),
                        origin = origin,
                        scale = this.body.radius
                    }, false);
                }
            }

            private void CalculateBlinkDestination()
            {
                var forward = Vector3.zero;
                var aimRay = this.body.inputBank ? this.body.inputBank.GetAimRay() : new Ray(this.transform.position, this.transform.forward);

                if (!this.body.isPlayerControlled)
                {
                    var bullseyeSearch = new BullseyeSearch
                    {
                        searchOrigin = aimRay.origin,
                        searchDirection = aimRay.direction,
                        maxDistanceFilter = blinkDistance,
                        teamMaskFilter = TeamMask.allButNeutral,
                        filterByLoS = false,
                        sortMode = BullseyeSearch.SortMode.Angle
                    };
                    bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(this.gameObject));
                    bullseyeSearch.RefreshCandidates();

                    var hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
                    if (hurtBox)
                    {
                        var center = hurtBox.transform.position - this.transform.position;
                        var radius = 15f;
                        forward = center + (Vector3)(radius * Random.insideUnitCircle);
                    }
                }

                if (forward == Vector3.zero && this.body.inputBank)
                    forward = this.body.inputBank.moveVector * shortBlinkDistance;

                this.blinkDestination = this.transform.position;
                this.blinkStart = this.blinkDestination;

                var nodes = !this.body.isFlying || this.body.name == "MinorConstructBody(Clone)" ? SceneInfo.instance.groundNodes : SceneInfo.instance.airNodes;
                nodes.GetNodePosition(nodes.FindClosestNode(this.transform.position + forward, this.body.hullClassification), out this.blinkDestination);

                this.blinkDestination += this.transform.position - this.body.footPosition;

                if (this.body.characterDirection)
                    this.body.characterDirection.forward = forward;
            }
        }
    }
}
