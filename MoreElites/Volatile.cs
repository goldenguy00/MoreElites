using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
    public class Volatile
    {
        public static Color AffixVolatileColor = Color.red;
        public static EquipmentDef AffixVolatileEquipment;
        public static BuffDef AffixVolatileBuff;
        public static EliteDef AffixVolatileElite;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        public static float affixDropChance = 0.00025f;
        private static GameObject VolatileProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion(), "AffixVolatileNuxProjectile");
        private static GameObject VolatileProjectileGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileGhost.prefab").WaitForCompletion(), "AffixVolatileNuxProjectileGhost");
        private static Material VolatileMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matMagmaWormFireball.mat").WaitForCompletion();
        private static Material projectileFlare = Addressables.LoadAssetAsync<Material>("RoR2/Junk/Bandit/matThermiteFlare.mat").WaitForCompletion();
        private static Material projectileTrail = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matMagmaWormFireballTrail.mat").WaitForCompletion();
        private static Texture2D eliteRamp = Addressables.LoadAssetAsync<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampStrongerBurn.png").WaitForCompletion();
        private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
        private static Sprite aspectIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();
        private static GameObject behemoExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab").WaitForCompletion();

        public Volatile()
        {
            VolatileProjectileGhost.transform.GetChild(0).GetComponent<TrailRenderer>().sharedMaterial = projectileTrail;
            VolatileProjectileGhost.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().sharedMaterial = projectileFlare;
            VolatileProjectileGhost.transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial = VolatileMat;
            VolatileProjectileGhost.transform.GetChild(3).GetComponent<Light>().color = AffixVolatileColor;

            VolatileProjectile.GetComponent<ProjectileController>().ghostPrefab = VolatileProjectileGhost;
            MissileController missileController = VolatileProjectile.GetComponent<MissileController>();
            missileController.acceleration = 2;
            missileController.deathTimer = 10;
            missileController.maxVelocity = 20;
            missileController.turbulence = 6f;
            ContentAddition.AddProjectile(VolatileProjectile);

            this.AddLanguageTokens();
            this.SetupBuff();
            this.SetupEquipment();
            this.SetupElite();
            this.AddContent();
            EliteRamp.AddRamp(AffixVolatileElite, eliteRamp);

            On.RoR2.GlobalEventManager.OnHitAll += AddBehemoExplosion;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
        }

        private void AddBehemoExplosion(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            if (damageInfo.procCoefficient == 0.0 || damageInfo.rejected)
                return;
            if (!(bool)damageInfo.attacker)
                return;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!(bool)body)
                return;
            if (body.HasBuff(AffixVolatileBuff))
            {
                float num2 = 4 * damageInfo.procCoefficient;
                float damageCoefficient = 0.25f;
                float num3 = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient);

                EffectManager.SpawnEffect(behemoExplosion, new EffectData()
                {
                    origin = damageInfo.position,
                    scale = num2,
                    rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                }, true);

                BlastAttack blastAttack = new BlastAttack()
                {
                    position = damageInfo.position,
                    baseDamage = num3,
                    baseForce = 0.0f,
                    radius = num2,
                    attacker = damageInfo.attacker,
                    inflictor = null
                };

                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.crit = damageInfo.crit;
                blastAttack.procChainMask = damageInfo.procChainMask;
                blastAttack.procCoefficient = 0.0f;
                blastAttack.damageColorIndex = DamageColorIndex.Item;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = damageInfo.damageType;
                blastAttack.Fire();
            }
        }

        private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            if (EliteAPI.VanillaEliteTiers.Length > 2)
            {
                // HONOR
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixVolatileElite.healthBoostCoefficient = 2.5f;
                AffixVolatileElite.damageBoostCoefficient = 1.5f;
                elites.Add(AffixVolatileElite);
                targetTier.eliteTypes = elites.ToArray();
            }
            if (EliteAPI.VanillaEliteTiers.Length > 1)
            {
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixVolatileElite.healthBoostCoefficient = 4f;
                AffixVolatileElite.damageBoostCoefficient = 2f;
                elites.Add(AffixVolatileElite);
                targetTier.eliteTypes = elites.ToArray();
            }
        }


        private void CharacterBody_OnBuffFirstStackGained(
          On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
          CharacterBody self,
          BuffDef buffDef
          )
        {
            orig(self, buffDef);
            if (buffDef == AffixVolatileBuff)
                self.gameObject.AddComponent<VolatileMissileController>();
        }

        private void CharacterBody_OnBuffFinalStackLost(
      On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
      CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == AffixVolatileBuff)
                GameObject.Destroy(self.gameObject.GetComponent<VolatileMissileController>());
        }

        private void AddContent()
        {
            ContentAddition.AddEliteDef(AffixVolatileElite);
            ContentAddition.AddBuffDef(AffixVolatileBuff);
            ContentAddition.AddEquipmentDef(AffixVolatileEquipment);
        }

        private void SetupBuff()
        {
            AffixVolatileBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixVolatileBuff.name = "AffixVolatileBuff";
            AffixVolatileBuff.canStack = false;
            AffixVolatileBuff.isCooldown = false;
            AffixVolatileBuff.isDebuff = false;
            AffixVolatileBuff.buffColor = AffixVolatileColor;
            AffixVolatileBuff.iconSprite = eliteIcon;
            (AffixVolatileBuff as UnityEngine.Object).name = AffixVolatileBuff.name;
        }

        private void SetupEquipment()
        {
            AffixVolatileEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixVolatileEquipment.appearsInMultiPlayer = true;
            AffixVolatileEquipment.appearsInSinglePlayer = true;
            AffixVolatileEquipment.canBeRandomlyTriggered = false;
            AffixVolatileEquipment.canDrop = false;
            AffixVolatileEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixVolatileEquipment.cooldown = 0.0f;
            AffixVolatileEquipment.isLunar = false;
            AffixVolatileEquipment.isBoss = false;
            AffixVolatileEquipment.passiveBuffDef = AffixVolatileBuff;
            AffixVolatileEquipment.dropOnDeathChance = affixDropChance * 0.01f;
            AffixVolatileEquipment.enigmaCompatible = false;
            AffixVolatileEquipment.pickupIconSprite = aspectIcon;
            AffixVolatileEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixVolatile", false);
            foreach (Renderer componentsInChild in AffixVolatileEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = VolatileMat;
            AffixVolatileEquipment.nameToken = "EQUIPMENT_AFFIX_Volatile_NAME";
            AffixVolatileEquipment.name = "AffixVolatile";

            AffixVolatileEquipment.pickupToken = "Aspect of Volatile";
            AffixVolatileEquipment.descriptionToken = "All attacks explode and periodically fire missiles.";
            AffixVolatileEquipment.loreToken = "";
        }

        private void SetupElite()
        {
            AffixVolatileElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixVolatileElite.color = AffixVolatileColor;
            AffixVolatileElite.eliteEquipmentDef = AffixVolatileEquipment;
            AffixVolatileElite.modifierToken = "ELITE_MODIFIER_Volatile";
            AffixVolatileElite.name = "EliteVolatile";
            AffixVolatileElite.healthBoostCoefficient = healthMult;
            AffixVolatileElite.damageBoostCoefficient = damageMult;
            AffixVolatileBuff.eliteDef = AffixVolatileElite;
            (AffixVolatileElite as ScriptableObject).name = "EliteVolatile";
        }

        private void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_Volatile", "Volatile {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_Volatile_NAME", "Volatile Aspect");
        }

        public class VolatileMissileController : MonoBehaviour
        {
            private float fireTimer;
            private float fireInterval = 3f;
            private float damageCoefficient = 0.5f;
            private CharacterBody body;

            private void Awake()
            {
                this.body = this.gameObject.GetComponent<CharacterBody>();
            }

            private void FixedUpdate()
            {
                this.fireTimer += Time.fixedDeltaTime;
                if ((double)this.fireTimer < this.fireInterval)
                    return;
                this.fireTimer %= 1;
                RecalculateInterval();
                if (this.body.healthComponent && !this.body.healthComponent.alive)
                    return;
                MissileUtils.FireMissile(this.body.corePosition, this.body, new ProcChainMask(), (GameObject)null, this.body.damage * damageCoefficient, Util.CheckRoll(this.body.crit, this.body.master), VolatileProjectile, DamageColorIndex.Item, false);
            }

            private void RecalculateInterval()
            {
                fireInterval = UnityEngine.Random.Range(1, 6);
            }
        }
    }
}
