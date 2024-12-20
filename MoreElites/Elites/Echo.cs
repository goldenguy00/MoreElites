using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System;

namespace MoreElites.Elites
{
    public class Echo : EliteBase<Echo>
    {
        public ItemDef summonedEchoItem = Addressables.LoadAssetAsync<ItemDef>("RoR2/InDev/SummonedEcho.asset").WaitForCompletion();
        public Material overlayMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/voidoutro/matVoidRaidCrabEyeOverlay1BLUE.mat").WaitForCompletion();

        public override string Name => "Echo";
        public override string EquipmentName => "Echo Aspect";
        public override string PickupText => "Aspect of Echo";
        public override string DescriptionText => "Summon 2 copies of yourself";
        public override string LoreText => "Shadow clone jutsu";

        public override EliteTier EliteTierDef => PluginConfig.eliteTierEcho.Value;
        public override Color EliteColor => Color.black;
        public override Texture2D EliteRamp => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampShadowClone.png").WaitForCompletion();
        public override Sprite EliteIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/InDev/matEcho.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixEcho", false);

        public override void Init()
        {
            base.Init();

            ContentAddition.AddItemDef(summonedEchoItem);

            var celestineHalo = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion().InstantiateClone("EchoCrown");
            celestineHalo.AddComponent<NetworkIdentity>();

            ItemAPI.Add(new CustomEquipment(CustomEliteDef.EliteDef.eliteEquipmentDef, ItemDisplays.CreateItemDisplayRules(celestineHalo, EliteMaterial)));

            RecalculateStatsAPI.GetStatCoefficients += ReduceSummonHP;
            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        }

        public override void OnBuffGained(CharacterBody self) => self.AddItemBehavior<CustomAffixEchoBehavior>(1);
        public override void OnBuffLost(CharacterBody self) => self.AddItemBehavior<CustomAffixEchoBehavior>(0);

        private void ReduceSummonHP(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                var stack = sender.inventory.GetItemCount(summonedEchoItem);
                if (stack > 0)
                    args.baseCurseAdd += Mathf.Pow(1 / 0.1f, stack) - 1;
            }
        }

        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            if (self.inventory && self.inventory.GetItemCount(summonedEchoItem) > 0)
                body.gameObject.AddComponent<CustomSummonedEchoBodyBehavior>();
        }

        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            if (self.body && self.body.inventory)
            {
                AddOverlay(overlayMat, self.body.HasBuff(EliteBuffDef));
                AddOverlay(EliteMaterial, self.body.inventory.GetItemCount(summonedEchoItem) > 0);
            }

            void AddOverlay(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount < CharacterModel.maxOverlays && condition)
                {
                    self.currentOverlays[self.activeOverlayCount++] = overlayMaterial;
                }
            }
        }

        public class CustomAffixEchoBehavior : CharacterBody.ItemBehavior
        {
            private DeployableMinionSpawner echoSpawner1;
            private DeployableMinionSpawner echoSpawner2;
            private CharacterSpawnCard spawnCard;
            private readonly List<CharacterMaster> spawnedEchoes = [];

            private void FixedUpdate() => this.spawnCard.nodeGraphType = this.body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;

            private void Awake()
            {
                this.enabled = false;
                // Play_voidRaid_breakLeg Play_voidRaid_fall_pt2  
                // Play_affix_void_spawn
                Util.PlaySound("Play_voidRaid_fog_explode", this.gameObject);
            }

            private void OnEnable()
            {
                var masterIndexForBody = MasterCatalog.FindAiMasterIndexForBody(this.body.bodyIndex);

                this.spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                this.spawnCard.prefab = MasterCatalog.GetMasterPrefab(masterIndexForBody);
                this.spawnCard.inventoryToCopy = this.body.inventory;
                this.spawnCard.itemsToGrant = [new ItemCountPair()
                {
                    itemDef = Instance.summonedEchoItem,
                    count = 1
                }];

                this.CreateSpawners();
            }

            private void OnDisable()
            {
                Destroy(this.spawnCard);
                this.spawnCard = null;

                for (var index = this.spawnedEchoes.Count - 1; index >= 0; --index)
                {
                    if (this.spawnedEchoes[index])
                        this.spawnedEchoes[index].TrueKill();
                }

                this.DestroySpawners();
            }

            private void CreateSpawners()
            {
                var rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)this.GetInstanceID());
                CreateSpawner(ref this.echoSpawner1, DeployableSlot.RoboBallRedBuddy, this.spawnCard);
                CreateSpawner(ref this.echoSpawner2, DeployableSlot.RoboBallGreenBuddy, this.spawnCard);

                void CreateSpawner(
                  ref DeployableMinionSpawner buddySpawner,
                  DeployableSlot deployableSlot,
                  SpawnCard spawnCard)
                {
                    buddySpawner = new DeployableMinionSpawner(this.body.master, deployableSlot, rng)
                    {
                        respawnInterval = 30f,
                        spawnCard = spawnCard
                    };
                    buddySpawner.onMinionSpawnedServer += this.OnMinionSpawnedServer;
                }
            }

            private void DestroySpawners()
            {
                this.echoSpawner1?.Dispose();
                this.echoSpawner1 = null;
                this.echoSpawner2?.Dispose();
                this.echoSpawner2 = null;
            }

            private void OnMinionSpawnedServer(SpawnCard.SpawnResult spawnResult)
            {
                if (spawnResult.spawnedInstance && spawnResult.spawnedInstance.TryGetComponent<CharacterMaster>(out var spawnedMaster))
                {
                    this.spawnedEchoes.Add(spawnedMaster);
                    OnDestroyCallback.AddCallback(spawnedMaster.gameObject, OnDestroyHandler);
                }
            }

            private void OnDestroyHandler(OnDestroyCallback callback)
            {
                this.spawnedEchoes.Remove(callback.GetComponent<CharacterMaster>());
            }
        }

        public class CustomSummonedEchoBodyBehavior : MonoBehaviour
        {
            private static float fireInterval = 3f;
            private static float normalBaseDamage = 24f;
            private static float normalLevelDamage = 4.8f;
            private static float championBaseDamage = 32f;
            private static float championLevelDamage = 7.2f;
            private static GameObject echoProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/InDev/EchoHunterProjectile.prefab").WaitForCompletion();

            private float fireTimer;
            private CharacterBody body;

            private void OnEnable()
            {
                this.body = this.GetComponent<CharacterBody>();
            }

            private void FixedUpdate()
            {
                if (!(this.body && this.body.healthComponent && this.body.healthComponent.alive))
                    return;

                this.fireTimer += Time.fixedDeltaTime;
                if (this.fireTimer >= fireInterval)
                {
                    this.fireTimer = 0;

                    var damage = this.body.isChampion
                        ? championBaseDamage + (championLevelDamage * this.body.level)
                        : normalBaseDamage + (normalLevelDamage * this.body.level);

                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
                    {
                        crit = false,
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageTypeOverride = DamageType.SlowOnHit,
                        owner = this.gameObject,
                        position = this.body.aimOrigin,
                        rotation = Quaternion.LookRotation(Vector3.up),
                        procChainMask = default,
                        projectilePrefab = echoProjectile,
                        force = 400f,
                        target = null
                    });
                }
            }
        }
    }
}
