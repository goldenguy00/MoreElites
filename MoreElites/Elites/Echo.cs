using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Linq;

namespace MoreElites
{
    public class Echo : EliteBase<Echo>
    {
        public ItemDef summonedEchoItem;
        public Material echoMatBlack = Addressables.LoadAssetAsync<Material>("RoR2/InDev/matEcho.mat").WaitForCompletion();
        public GameObject echoProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/InDev/EchoHunterProjectile.prefab").WaitForCompletion().InstantiateClone("EchoHunterProjectile");

        public override string Name => "Echo";
        public override string EquipmentName => "Echo Aspect";
        public override string PickupText => "Aspect of Echo";
        public override string DescriptionText => "Summon 2 copies of yourself";
        public override string LoreText => "Shadow clone jutsu";

        public override EliteTier EliteTierDef => (EliteTier)PluginConfig.eliteTierEcho.Value;
        public override Color EliteColor => Color.black;
        public override Texture2D EliteRamp => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampShadowClone.png").WaitForCompletion();
        public override Sprite EliteIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/voidoutro/matVoidRaidCrabEyeOverlay1BLUE.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixEcho", false);

        public override void Init()
        {
            base.Init();

            var orig = Addressables.LoadAssetAsync<ItemDef>("RoR2/InDev/SummonedEcho.asset").WaitForCompletion();

            var customSummonItem = new CustomItem("EchoSummonItem",
                this.CustomEquipmentDef.EquipmentDef.nameToken,
                this.CustomEquipmentDef.EquipmentDef.descriptionToken,
                this.CustomEquipmentDef.EquipmentDef.loreToken,
                this.CustomEquipmentDef.EquipmentDef.pickupToken,
                orig.pickupIconSprite,
                orig.pickupModelPrefab,
                orig.tier,
                orig.tags,
                orig.canRemove,
                orig.hidden, null, null);

            this.summonedEchoItem = customSummonItem.ItemDef;
            ItemAPI.Add(customSummonItem);

            var celestineHalo = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion().InstantiateClone("EchoCrown");
            celestineHalo.AddComponent<NetworkIdentity>();

            this.CustomEquipmentDef.ItemDisplayRules = ItemDisplays.CreateItemDisplayRules(celestineHalo, EliteMaterial);

            RecalculateStatsAPI.GetStatCoefficients += ReduceSummonHP;
            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        }

        public override void OnBuffGained(CharacterBody self)
        {
            if (NetworkServer.active)
                self.AddItemBehavior<CustomAffixEchoBehavior>(1);
        }

        public override void OnBuffLost(CharacterBody self)
        {
            if (NetworkServer.active)
                self.AddItemBehavior<CustomAffixEchoBehavior>(0);
        }

        private void ReduceSummonHP(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var stack = sender.inventory ? sender.inventory.GetItemCount(summonedEchoItem) : 0;

            if (stack > 0)
                args.baseCurseAdd += Mathf.Pow(1 / 0.1f, stack) - 1;
        }

        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            var stack = self.inventory ? self.inventory.GetItemCount(summonedEchoItem) : 0;

            if (stack > 0)
                body.gameObject.AddComponent<CustomSummonedEchoBodyBehavior>();
        }

        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            if (self.body && self.body.inventory)
            {
                AddOverlay(EliteMaterial, self.body.HasBuff(EliteBuffDef));
                AddOverlay(echoMatBlack, self.body.inventory.GetItemCount(summonedEchoItem) > 0);
            }

            void AddOverlay(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount < CharacterModel.maxOverlays && condition)
                    self.currentOverlays[self.activeOverlayCount++] = overlayMaterial;
            }
        }

        public class CustomAffixEchoBehavior : CharacterBody.ItemBehavior
        {
            public DeployableMinionSpawner echoSpawner1;
            public DeployableMinionSpawner echoSpawner2;

            public CharacterSpawnCard spawnCard;

            public List<CharacterMaster> spawnedEchoes = new List<CharacterMaster>();

            public bool hasEverSpawned;

            public void FixedUpdate()
            {
                spawnCard.nodeGraphType = body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;

                if (!hasEverSpawned)
                {
                    echoSpawner1.respawnStopwatch++;
                    echoSpawner2.respawnStopwatch++;
                }
            }

            public void Awake()
            {
                enabled = false;
                Util.PlaySound("Play_voidRaid_fog_explode", this.gameObject);
            }

            public void OnEnable()
            {
                var masterIndex = MasterCatalog.FindAiMasterIndexForBody(body.bodyIndex);
                spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                spawnCard.prefab = MasterCatalog.GetMasterPrefab(masterIndex);
                spawnCard.inventoryToCopy = body.inventory;
                spawnCard.equipmentToGrant = new EquipmentDef[1];
                spawnCard.itemsToGrant =
                [
                    new ItemCountPair
                    {
                        itemDef = Instance.summonedEchoItem,
                        count = 1
                    }
                ];
                CreateSpawners();
            }

            public void OnDisable()
            {
                Destroy(spawnCard);
                spawnCard = null;
                for (var num = spawnedEchoes.Count - 1; num >= 0; num--)
                {
                    if (spawnedEchoes[num])
                        spawnedEchoes[num].TrueKill();
                }

                DestroySpawners();
            }

            public void CreateSpawners()
            {
                var rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)GetInstanceID());
                CreateSpawner(ref echoSpawner1, DeployableSlot.RoboBallRedBuddy, spawnCard);
                CreateSpawner(ref echoSpawner2, DeployableSlot.RoboBallGreenBuddy, spawnCard);
                void CreateSpawner(ref DeployableMinionSpawner buddySpawner, DeployableSlot deployableSlot, SpawnCard spawnCard)
                {
                    buddySpawner = new DeployableMinionSpawner(body.master, deployableSlot, rng)
                    {
                        maxSpawnDistance = 20f,
                        respawnInterval = 30f,
                        spawnCard = spawnCard
                    };
                    buddySpawner.onMinionSpawnedServer += OnMinionSpawnedServer;
                }
            }

            public void DestroySpawners()
            {
                echoSpawner1?.Dispose();
                echoSpawner1 = null;
                echoSpawner2?.Dispose();
                echoSpawner2 = null;
            }

            public void OnMinionSpawnedServer(SpawnCard.SpawnResult spawnResult)
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
