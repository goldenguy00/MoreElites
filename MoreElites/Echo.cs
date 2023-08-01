using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
  public class Echo
  {
    public static Color AffixEchoColor = Color.black;
    public static EquipmentDef AffixEchoEquipment;
    public static BuffDef AffixEchoBuff;
    public static EliteDef AffixEchoElite;
    public static ItemDef SummonedEcho = Addressables.LoadAssetAsync<ItemDef>("RoR2/InDev/SummonedEcho.asset").WaitForCompletion();
    public static float healthMult = 4f;
    public static float damageMult = 2f;
    public static float affixDropChance = 0f;
    private static GameObject echoProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/InDev/EchoHunterProjectile.prefab").WaitForCompletion();
    private static GameObject celestineHalo = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion(), "EchoCrown");
    private static Material echoMat = Addressables.LoadAssetAsync<Material>("RoR2/InDev/matEcho.mat").WaitForCompletion();
    private static Texture2D eliteRamp = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampShadowClone.png").WaitForCompletion();
    private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
    // RoR2/Base/Common/ColorRamps/texRampWarbanner.png RoR2/DLC1/Common/ColorRamps/texRampVoidArenaShield.png

    public Echo()
    {
      this.AddLanguageTokens();
      this.SetupBuff();
      this.SetupEquipment();
      this.SetupElite();
      this.AddContent();
      EliteRamp.AddRamp(AffixEchoElite, eliteRamp);
      ContentAddition.AddItemDef(SummonedEcho);
      ContentAddition.AddEliteDef(AffixEchoElite);
      ContentAddition.AddBuffDef(AffixEchoBuff);
      RecalculateStatsAPI.GetStatCoefficients += ReduceSummonHP;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
      On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
      On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
      On.RoR2.CombatDirector.Init += CombatDirector_Init;
    }

    private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
    {
      orig();
      if (EliteAPI.VanillaEliteTiers.Length > 2)
      {
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        elites.Add(AffixEchoElite);
        targetTier.eliteTypes = elites.ToArray();
      }
      if (EliteAPI.VanillaEliteTiers.Length > 1)
      {
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        elites.Add(AffixEchoElite);
        targetTier.eliteTypes = elites.ToArray();
      }
    }

    private void ReduceSummonHP(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
      if (sender && sender.inventory)
      {
        int stack = sender.inventory.GetItemCount(SummonedEcho);
        if (stack > 0)
          args.baseCurseAdd += Mathf.Pow(1 / 0.1f, stack) - 1;
      }
    }

    private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (self.inventory && self.inventory.GetItemCount(SummonedEcho) > 0)
        body.gameObject.AddComponent<CustomSummonedEchoBodyBehavior>();
    }

    private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
    {
      orig(self);
      if (self.body)
      {
        if (self.activeOverlayCount >= CharacterModel.maxOverlays) return;
        if (self.body.inventory.GetItemCount(SummonedEcho) > 0)
        {
          Material[] array = self.currentOverlays;
          int num = self.activeOverlayCount;
          self.activeOverlayCount = num + 1;
          array[num] = echoMat;
        }
      }
    }

    private void CharacterBody_OnBuffFirstStackGained(
      On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
      CharacterBody self,
      BuffDef buffDef
      )
    {
      orig(self, buffDef);
      if (buffDef == AffixEchoBuff)
        self.AddItemBehavior<CustomAffixEchoBehavior>(1);
    }

    private void CharacterBody_OnBuffFinalStackLost(
  On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
  CharacterBody self, BuffDef buffDef)
    {
      orig(self, buffDef);
      if (buffDef == AffixEchoBuff)
        self.AddItemBehavior<CustomAffixEchoBehavior>(0);
    }

    private void AddContent()
    {
      ItemDisplays itemDisplays = new ItemDisplays();
      ContentAddition.AddEliteDef(AffixEchoElite);
      ContentAddition.AddBuffDef(AffixEchoBuff);
      ItemAPI.Add(new CustomEquipment(AffixEchoEquipment, itemDisplays.CreateItemDisplayRules(celestineHalo, echoMat)));
    }

    private void SetupBuff()
    {
      AffixEchoBuff = ScriptableObject.CreateInstance<BuffDef>();
      AffixEchoBuff.name = "AffixEchoBuff";
      AffixEchoBuff.canStack = false;
      AffixEchoBuff.isCooldown = false;
      AffixEchoBuff.isDebuff = false;
      AffixEchoBuff.buffColor = AffixEchoColor;
      AffixEchoBuff.iconSprite = eliteIcon;
    }

    private void SetupEquipment()
    {
      AffixEchoEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
      AffixEchoEquipment.appearsInMultiPlayer = true;
      AffixEchoEquipment.appearsInSinglePlayer = true;
      AffixEchoEquipment.canBeRandomlyTriggered = false;
      AffixEchoEquipment.canDrop = false;
      AffixEchoEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
      AffixEchoEquipment.cooldown = 0.0f;
      AffixEchoEquipment.isLunar = false;
      AffixEchoEquipment.isBoss = false;
      AffixEchoEquipment.passiveBuffDef = AffixEchoBuff;
      AffixEchoEquipment.dropOnDeathChance = affixDropChance * 0.01f;
      AffixEchoEquipment.enigmaCompatible = false;
      AffixEchoEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixEcho", false);
      foreach (Renderer componentsInChild in AffixEchoEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
        componentsInChild.material = echoMat;
      AffixEchoEquipment.nameToken = "EQUIPMENT_AFFIX_Echo_NAME";
      AffixEchoEquipment.name = "AffixEcho";
    }

    private void SetupElite()
    {
      AffixEchoElite = ScriptableObject.CreateInstance<EliteDef>();
      AffixEchoElite.color = AffixEchoColor;
      AffixEchoElite.eliteEquipmentDef = AffixEchoEquipment;
      AffixEchoElite.modifierToken = "ELITE_MODIFIER_Echo";
      AffixEchoElite.name = "EliteEcho";
      AffixEchoElite.healthBoostCoefficient = healthMult;
      AffixEchoElite.damageBoostCoefficient = damageMult;
      AffixEchoBuff.eliteDef = AffixEchoElite;
    }

    private void AddLanguageTokens()
    {
      LanguageAPI.Add("ELITE_MODIFIER_Echo", "Echo {0}");
      LanguageAPI.Add("EQUIPMENT_AFFIX_Echo_NAME", "Echo Aspect");
    }
    public class CustomAffixEchoBehavior : CharacterBody.ItemBehavior
    {
      private DeployableMinionSpawner echoSpawner1;
      private DeployableMinionSpawner echoSpawner2;
      private CharacterSpawnCard spawnCard;
      private List<CharacterMaster> spawnedEchoes = new List<CharacterMaster>();

      private void FixedUpdate() => this.spawnCard.nodeGraphType = this.body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;

      private void Awake() => this.enabled = false;

      private void OnEnable()
      {
        MasterCatalog.MasterIndex masterIndexForBody = MasterCatalog.FindAiMasterIndexForBody(this.body.bodyIndex);
        this.spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        this.spawnCard.prefab = MasterCatalog.GetMasterPrefab(masterIndexForBody);
        this.spawnCard.inventoryToCopy = this.body.inventory;
        this.spawnCard.equipmentToGrant = new EquipmentDef[1];
        this.spawnCard.itemsToGrant = new ItemCountPair[1]
        {
          new ItemCountPair()
          {
            itemDef = SummonedEcho,
            count = 1
          }
        };
        this.CreateSpawners();
      }

      private void OnDisable()
      {
        UnityEngine.Object.Destroy((UnityEngine.Object)this.spawnCard);
        this.spawnCard = (CharacterSpawnCard)null;
        for (int index = this.spawnedEchoes.Count - 1; index >= 0; --index)
        {
          if ((bool)(UnityEngine.Object)this.spawnedEchoes[index])
            this.spawnedEchoes[index].TrueKill();
        }
        this.DestroySpawners();
      }

      private void CreateSpawners()
      {
        Xoroshiro128Plus rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)this.GetInstanceID());
        CreateSpawner(ref this.echoSpawner1, DeployableSlot.RoboBallRedBuddy, (SpawnCard)this.spawnCard);
        CreateSpawner(ref this.echoSpawner2, DeployableSlot.RoboBallGreenBuddy, (SpawnCard)this.spawnCard);

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
          buddySpawner.onMinionSpawnedServer += new Action<SpawnCard.SpawnResult>(this.OnMinionSpawnedServer);
        }
      }

      private void DestroySpawners()
      {
        this.echoSpawner1?.Dispose();
        this.echoSpawner1 = (DeployableMinionSpawner)null;
        this.echoSpawner2?.Dispose();
        this.echoSpawner2 = (DeployableMinionSpawner)null;
      }

      private void OnMinionSpawnedServer(SpawnCard.SpawnResult spawnResult)
      {
        GameObject spawnedInstance = spawnResult.spawnedInstance;
        if (!(bool)(UnityEngine.Object)spawnedInstance)
          return;
        CharacterMaster spawnedMaster = spawnedInstance.GetComponent<CharacterMaster>();
        if (!(bool)(UnityEngine.Object)spawnedMaster)
          return;
        this.spawnedEchoes.Add(spawnedMaster);
        OnDestroyCallback.AddCallback(spawnedMaster.gameObject, (Action<OnDestroyCallback>)(_ => this.spawnedEchoes.Remove(spawnedMaster)));
      }
    }

    public class CustomSummonedEchoBodyBehavior : MonoBehaviour
    {
      private float fireTimer;
      private float fireInterval = 3f;
      private float damageCoefficient = 3f;
      private CharacterBody body;

      private void Awake()
      {
        this.body = this.gameObject.GetComponent<CharacterBody>();
      }

      private void FixedUpdate()
      {
        this.fireTimer -= Time.fixedDeltaTime;
        if ((double)this.fireTimer > 0.0)
          return;
        this.fireTimer = this.fireInterval;
        if (this.body.healthComponent && !this.body.healthComponent.alive)
          return;
        ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
        {
          crit = false,
          damage = this.body.damage * this.damageCoefficient,
          damageColorIndex = DamageColorIndex.Default,
          damageTypeOverride = new DamageType?(DamageType.SlowOnHit),
          owner = this.body.gameObject,
          position = this.body.aimOrigin,
          rotation = Quaternion.LookRotation(Vector3.up),
          procChainMask = new ProcChainMask(),
          projectilePrefab = echoProjectile,
          force = 400f,
          target = (GameObject)null
        });
      }
    }
  }
}
