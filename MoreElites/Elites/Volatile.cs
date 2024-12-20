using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites.Elites
{
    public class Volatile : EliteBase<Volatile>
    {
        private GameObject VolatileProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion().InstantiateClone("AffixVolatileNuxProjectile");
        private GameObject VolatileProjectileGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileGhost.prefab").WaitForCompletion().InstantiateClone("AffixVolatileNuxProjectileGhost");
        private GameObject behemoExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab").WaitForCompletion();

        public override string Name => "Volatile";
        public override string EquipmentName => "Volatile Aspect";
        public override string PickupText => "Aspect of Volatility";
        public override string DescriptionText => "All attacks explode and periodically fire missiles.";
        public override string LoreText => "Hope you like dodging";

        public override EliteTier EliteTierDef => (EliteTier)PluginConfig.eliteTierVolatile.Value;
        public override Color EliteColor => Color.black;
        public override Texture2D EliteRamp => Addressables.LoadAssetAsync<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampStrongerBurn.png").WaitForCompletion();
        public override Sprite EliteIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        public override Sprite AspectIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();

        public override Material EliteMaterial { get; set; } = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matMagmaWormFireball.mat").WaitForCompletion();
        public override GameObject PickupModelPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion().InstantiateClone("PickupAffixVolatile", false);

        public override void Init()
        {
            base.Init();

            VolatileProjectileGhost.transform.GetChild(0).GetComponent<TrailRenderer>().sharedMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matMagmaWormFireballTrail.mat").WaitForCompletion();
            VolatileProjectileGhost.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().sharedMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Junk/Bandit/matThermiteFlare.mat").WaitForCompletion();
            VolatileProjectileGhost.transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial = this.EliteMaterial;
            VolatileProjectileGhost.transform.GetChild(3).GetComponent<Light>().color = this.EliteColor;

            VolatileProjectile.GetComponent<ProjectileController>().ghostPrefab = this.VolatileProjectileGhost;
            var missileController = VolatileProjectile.GetComponent<MissileController>();
            missileController.acceleration = 2;
            missileController.deathTimer = 10;
            missileController.maxVelocity = 20;
            missileController.turbulence = 6f;
            ContentAddition.AddProjectile(VolatileProjectile);

            On.RoR2.GlobalEventManager.OnHitAll += AddBehemoExplosion;
        }

        public override void OnBuffGained(CharacterBody self) => self.AddItemBehavior<VolatileMissileController>(1);
        public override void OnBuffLost(CharacterBody self) => self.AddItemBehavior<VolatileMissileController>(0);

        private void AddBehemoExplosion(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);

            if (damageInfo.procCoefficient > 0 && !damageInfo.rejected && damageInfo.attacker && damageInfo.attacker.TryGetComponent<CharacterBody>(out var body) && body.HasBuff(EliteBuffDef))
            {
                var radius = 4 * damageInfo.procCoefficient;

                EffectManager.SpawnEffect(behemoExplosion, new EffectData()
                {
                    origin = damageInfo.position,
                    scale = radius,
                    rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                }, true);

                new BlastAttack()
                {
                    position = damageInfo.position,
                    baseDamage = Util.OnHitProcDamage(damageInfo.damage, body.damage, 0.25f),
                    baseForce = 0f,
                    radius = radius,
                    attacker = damageInfo.attacker,
                    inflictor = damageInfo.inflictor,
                    teamIndex = TeamComponent.GetObjectTeam(damageInfo.attacker),
                    crit = damageInfo.crit,
                    procChainMask = damageInfo.procChainMask,
                    procCoefficient = 0f,
                    damageColorIndex = DamageColorIndex.Item,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    damageType = damageInfo.damageType
                }.Fire();
            }
        }

        public class VolatileMissileController : CharacterBody.ItemBehavior
        {
            private static float normalBaseDamage = 12f;
            private static float normalLevelDamage = 2.4f;
            private static float championBaseDamage = 18f;
            private static float championLevelDamage = 3.6f;

            private float fireTimer;
            private float fireInterval = 3f;

            private void FixedUpdate()
            {
                if (!this.body || !this.body.healthComponent || !this.body.healthComponent.alive)
                    return;

                this.fireTimer += Time.fixedDeltaTime;
                if (this.fireTimer >= fireInterval)
                {
                    this.fireTimer = 0;

                    fireInterval = UnityEngine.Random.Range(1, 6);

                    var damage = this.body.isChampion
                        ? championBaseDamage + championLevelDamage * this.body.level
                        : normalBaseDamage + normalLevelDamage * this.body.level;

                    MissileUtils.FireMissile(this.body.corePosition, this.body, default, null, damage, Util.CheckRoll(this.body.crit, this.body.master), Instance.VolatileProjectile, DamageColorIndex.Item, false);
                }
            }
        }
    }
}
