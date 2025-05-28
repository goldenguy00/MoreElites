using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Items;

namespace MoreElites
{
    public class Echo : EliteBase<Echo>
    {
        public ItemDef summonedEchoItem;
        public Material echoMatBlack = Addressables.LoadAssetAsync<Material>("RoR2/InDev/matEcho.mat").WaitForCompletion();

        public override string Name => "Echo";
        public override string EquipmentName => "Echo Aspect";
        public override string PickupText => "Aspect of Echo";
        public override string DescriptionText => "Summon 2 copies of yourself";
        public override string LoreText => "Shadow clone jutsu";

        public override EliteTier EliteTierDef => (EliteTier)PluginConfig.eliteTierEcho.Value;
        public override Color EliteColor => Color.black;

        public override Texture2D EliteRamp { get; set; } = EliteRampGenerator.CreateGradientTexture([
            new Color32(23, 22, 20, 255),
            new Color32(117, 64, 67, 255),
            new Color32(154, 136, 115, 255),
            new Color32(55, 66, 61, 255),
            new Color32(58, 38, 24, 255),
        ], 256, 8);

        public override Sprite EliteIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon { get; set; } = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();
        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/voidoutro/matVoidRaidCrabEyeOverlay1BLUE.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixEcho", false);

        public override void Init()
        {
            base.Init();

            summonedEchoItem = Addressables.LoadAssetAsync<ItemDef>("RoR2/InDev/SummonedEcho.asset").WaitForCompletion();
            ContentAddition.AddItemDef(summonedEchoItem);

            var celestineHalo = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion().InstantiateClone("EchoCrown");
            celestineHalo.AddComponent<NetworkIdentity>();

            this.CustomEquipmentDef.ItemDisplayRules = ItemDisplays.CreateItemDisplayRules(celestineHalo, EliteMaterial);

            RecalculateStatsAPI.GetStatCoefficients += ReduceSummonHP;
            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
            On.RoR2.CharacterBody.AffixEchoBehavior.OnEnable += AffixEchoBehavior_OnEnable;
            On.RoR2.CharacterBody.AffixEchoBehavior.OnDisable += AffixEchoBehavior_OnDisable;
        }

        private void AffixEchoBehavior_OnDisable(On.RoR2.CharacterBody.AffixEchoBehavior.orig_OnDisable orig, CharacterBody.AffixEchoBehavior self)
        {
            if (self.spawnCard)
                UnityEngine.Object.Destroy(self.spawnCard);
            self.spawnCard = null;

            for (int num = self.spawnedEchoes.Count - 1; num >= 0; num--)
            {
                if (self.spawnedEchoes[num])
                {
                    self.spawnedEchoes[num].TrueKill();
                }
            }

            self.DestroySpawners();
        }

        private void AffixEchoBehavior_OnEnable(On.RoR2.CharacterBody.AffixEchoBehavior.orig_OnEnable orig, CharacterBody.AffixEchoBehavior self)
        {
            var itemCount = self.body.inventory ? self.body.inventory.GetItemCount(summonedEchoItem) : 0;
            if (itemCount > 0)
            {
                self.enabled = false;
                MonoBehaviour.Destroy(self);
            }
            else
            {
                orig(self);
                Util.PlaySound("Play_voidRaid_fog_explode", self.gameObject);
            }
        }

        private void ReduceSummonHP(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(summonedEchoItem) > 0)
                args.baseCurseAdd += 1f / 0.15f;
        }

        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            var itemCount = self.inventory ? self.inventory.GetItemCount(summonedEchoItem) : 0;
            if (itemCount > 0 && !body.GetComponent<CustomSummonedEchoBodyBehavior>())
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

        public override void OnBuffGained(CharacterBody self)
        {
            if (NetworkServer.active)
                self.AddItemBehavior<CharacterBody.AffixEchoBehavior>(1);
        }

        public override void OnBuffLost(CharacterBody self)
        {
            if (NetworkServer.active)
                self.AddItemBehavior<CharacterBody.AffixEchoBehavior>(0);
        }

        public class CustomSummonedEchoBodyBehavior : SummonedEchoBodyBehavior
        {
            private static float normalBaseDamage = 24f;
            private static float normalLevelDamage = 4.8f;
            private static float championBaseDamage = 32f;
            private static float championLevelDamage = 7.2f;

            private new void FixedUpdate()
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
                        projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/EchoHunterProjectile"),
                        force = 400f,
                        target = null
                    });
                }
            }
        }
    }
}
