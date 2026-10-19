using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Items;
using System.Collections.Generic;
using RoR2.Navigation;
using RoR2BepInExPack.GameAssetPathsBetter;
using HG;

namespace MoreElites
{
    public class Echo : EliteBase<Echo>
    {
        public ItemDef summonedEchoItem = Addressables.LoadAssetAsync<ItemDef>(RoR2_Junk.SummonedEcho_asset).WaitForCompletion();
        public Material echoMatBlack = Addressables.LoadAssetAsync<Material>(RoR2_Junk.matEcho_mat).WaitForCompletion();
        public DeployableSlot deployableSlot = DeployableAPI.RegisterDeployableSlot((_, _) => 2);
        public GameObject echoProjectile = Addressables.LoadAssetAsync<GameObject>(RoR2_Junk.EchoHunterProjectile_prefab).WaitForCompletion().InstantiateClone("EchoHunterProjectile");

        public override string Name => "Echo";
        public override string EquipmentName => "Echo Aspect";
        public override string PickupText => "Aspect of Echo";
        public override string DescriptionText => "Summon 2 copies of yourself";
        public override string LoreText => "Shadow clone jutsu";

        public override VanillaEliteTier EliteTierEnum => (VanillaEliteTier)PluginConfig.eliteTierEcho.Value;
        public override Color EliteColor => Color.black;

        public override Texture2D EliteRamp { get; set; } = EliteRampGenerator.CreateGradientTexture([
            new Color32(23, 22, 20, 255),
            new Color32(117, 64, 67, 255),
            new Color32(154, 136, 115, 255),
            new Color32(55, 66, 61, 255),
            new Color32(58, 38, 24, 255),
        ], 256, 8);

        public override Sprite EliteIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_EliteIce.texBuffAffixWhite_tif).WaitForCompletion();
        public override Sprite AspectIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>(RoR2_DLC1_EliteEarth.texAffixEarthIcon_png).WaitForCompletion();
        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>(RoR2_DLC1_voidoutro.matVoidRaidCrabEyeOverlay1BLUE_mat).WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_EliteFire.PickupEliteFire_prefab).WaitForCompletion().InstantiateClone("PickupAffixEcho", false);

        public override void Init()
        {
            base.Init();

            echoProjectile.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 120;
            ContentAddition.AddProjectile(echoProjectile);
            ContentAddition.AddItemDef(summonedEchoItem);

            var celestineHalo = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_EliteHaunted.DisplayEliteStealthCrown_prefab).WaitForCompletion().InstantiateClone("EchoCrown");
            celestineHalo.AddComponent<NetworkIdentity>();

            this.CustomEquipmentDef.ItemDisplayRules = ItemDisplays.CreateItemDisplayRules(celestineHalo, EliteMaterial);

            RecalculateStatsAPI.GetStatCoefficients += ReduceSummonHP;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        }

        public override void OnBuffGained(CharacterBody self)
        {
            if (NetworkServer.active && self.inventory && self.inventory.GetItemCountPermanent(summonedEchoItem) == 0)
#pragma warning disable CS0618 // Type or member is obsolete
                self.AddItemBehavior<CustomAffixEchoBehavior>(1);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public override void OnBuffLost(CharacterBody self)
        {
            if (NetworkServer.active)
#pragma warning disable CS0618 // Type or member is obsolete
                self.AddItemBehavior<CustomAffixEchoBehavior>(0);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void ReduceSummonHP(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCountPermanent(summonedEchoItem) > 0)
                args.baseCurseAdd += 1f / 0.15f;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (NetworkServer.active && body.inventory && body.inventory.GetItemCountPermanent(summonedEchoItem) > 0 && !body.GetComponent<CustomSummonedEchoBodyBehavior>())
                body.EnsureComponent<CustomSummonedEchoBodyBehavior>();
        }
        
        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            if (self.body && self.body.inventory)
            {
                AddOverlay(EliteMaterial, self.body.HasBuff(EliteBuffDef));
                AddOverlay(echoMatBlack, self.body.inventory.GetItemCountPermanent(summonedEchoItem) > 0);
            }

            void AddOverlay(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount < CharacterModel.maxOverlays && condition)
                    self.currentOverlays[self.activeOverlayCount++] = overlayMaterial;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public class CustomAffixEchoBehavior : CharacterBody.ItemBehavior
#pragma warning restore CS0618 // Type or member is obsolete
        {
            public DeployableMinionSpawner echoSpawner1;
            public DeployableMinionSpawner echoSpawner2;

            public CharacterSpawnCard spawnCard;

            public List<CharacterMaster> spawnedEchoes = new List<CharacterMaster>();

            public bool hasEverSpawned;

            public void FixedUpdate()
            {
                if (!hasEverSpawned)
                {
                    echoSpawner1.respawnStopwatch++;
                    echoSpawner2.respawnStopwatch++;
                }
            }

            public void Awake()
            {
                enabled = false;
            }

            public void OnEnable()
            {
                Util.PlaySound("Play_voidRaid_fog_explode", this.gameObject);

                spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                spawnCard.prefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(body.bodyIndex));
                spawnCard.inventoryToCopy = body.inventory;
                spawnCard.nodeGraphType = body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;
                spawnCard.equipmentToGrant = new EquipmentDef[1];
                spawnCard.itemsToGrant =
                [
                    new ItemCountPair
                    {
                        itemDef = Instance.summonedEchoItem,
                        count = 1
                    }
                ];

                var rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)GetInstanceID());
                echoSpawner1 = CreateSpawner(rng);
                echoSpawner2 = CreateSpawner(rng);
            }

            public void OnDisable()
            {
                if (spawnCard)
                    Destroy(spawnCard);
                spawnCard = null;

                for (var num = spawnedEchoes.Count - 1; num >= 0; num--)
                {
                    if (spawnedEchoes[num])
                        spawnedEchoes[num].TrueKill();
                }
                spawnedEchoes.Clear();

                echoSpawner1?.Dispose();
                echoSpawner1 = null;
                echoSpawner2?.Dispose();
                echoSpawner2 = null;
            }

            private DeployableMinionSpawner CreateSpawner(Xoroshiro128Plus rng)
            {
                var buddySpawner = new DeployableMinionSpawner(body.master, Instance.deployableSlot, rng)
                {
                    maxSpawnDistance = 20f,
                    respawnInterval = 30f,
                    respawnStopwatch = 0f,
                    spawnCard = spawnCard
                };

                buddySpawner.onMinionSpawnedServer += delegate (SpawnCard.SpawnResult spawnResult)
                {
                    var spawnedInstance = spawnResult.spawnedInstance;
                    if (!spawnedInstance)
                        return;

                    var spawnedMaster = spawnedInstance.GetComponent<CharacterMaster>();
                    if (spawnedMaster)
                    {
                        hasEverSpawned = true;
                        spawnedEchoes.Add(spawnedMaster);
                        OnDestroyCallback.AddCallback(spawnedMaster.gameObject, delegate
                        {
                            spawnedEchoes.Remove(spawnedMaster);
                        });
                    }
                };

                return buddySpawner;
            }
        }

        public class CustomSummonedEchoBodyBehavior : MonoBehaviour
        {
            private static float fireInterval = 3f;
            private static float normalBaseDamage = 24f;
            private static float normalLevelDamage = 4.8f;
            private static float championBaseDamage = 32f;
            private static float championLevelDamage = 7.2f;

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
                        ? championBaseDamage + championLevelDamage * this.body.level
                        : normalBaseDamage + normalLevelDamage * this.body.level;

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
                        projectilePrefab = Instance.echoProjectile,
                        force = 400f,
                        target = null
                    });
                }
            }
        }
    }
}
