using R2API;
using RoR2;
using RoR2.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
  public class Frenzied
  {
    public static Color AffixFrenziedColor = Color.yellow;
    public static Color AffixFrenziedLightColor = Color.yellow;
    public static EquipmentDef AffixFrenziedEquipment;
    public static BuffDef AffixFrenziedBuff;
    public static EliteDef AffixFrenziedElite;
    public static float healthMult = 4f;
    public static float damageMult = 2f;
    public static float affixDropChance = 0.00025f;
    private static GameObject FrenziedWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "FrenziedWard");
    private static GameObject celestineHalo = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElitePoison/DisplayEliteUrchinCrown.prefab").WaitForCompletion(), "FrenziedCrown");
    private static Material warbannerMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerSphereIndicator.mat").WaitForCompletion();
    private static GameObject blinkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Treebot/SonicBoomEffect.prefab").WaitForCompletion();
    private static GameObject blinkEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorRetreatToShellEffect.prefab").WaitForCompletion();
    private static Material FrenziedMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffRing.mat").WaitForCompletion();
    private static Texture2D eliteRamp = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWarbanner2.png").WaitForCompletion();
    private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
    private static Sprite aspectIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();
    // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 

    public Frenzied()
    {
      celestineHalo.AddComponent<NetworkIdentity>();
      FrenziedWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = FrenziedMat;
      this.AddLanguageTokens();
      this.SetupBuff();
      this.SetupEquipment();
      this.SetupElite();
      this.AddContent();
      EliteRamp.AddRamp(AffixFrenziedElite, eliteRamp);
      RecalculateStatsAPI.GetStatCoefficients += Frenzy;
      On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
      On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
      On.RoR2.CombatDirector.Init += CombatDirector_Init;
      On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
    }

    private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
    {
      bool result = orig(self, equipmentDef);
      if (!result)
      {
        if (equipmentDef == AffixFrenziedEquipment)
        {
          CharacterBody cb = self.characterBody;
          if (cb && cb.isPlayerControlled && Run.instance)
          {
            Xoroshiro128Plus rng = Run.instance.spawnRng;
            FrenziedTeleportController abc = cb.GetComponent<FrenziedTeleportController>();
            if (abc)
              self.StartCoroutine(abc.Teleport());
          }
          result = true;
        }
      }
      return result;
    }

    private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
    {
      orig();
      if (EliteAPI.VanillaEliteTiers.Length > 2)
      {
        // HONOR
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        AffixFrenziedElite.healthBoostCoefficient = 2.5f;
        AffixFrenziedElite.damageBoostCoefficient = 1.5f;
        elites.Add(AffixFrenziedElite);
        targetTier.eliteTypes = elites.ToArray();
      }
      if (EliteAPI.VanillaEliteTiers.Length > 1)
      {
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        AffixFrenziedElite.healthBoostCoefficient = 4f;
        AffixFrenziedElite.damageBoostCoefficient = 2f;
        elites.Add(AffixFrenziedElite);
        targetTier.eliteTypes = elites.ToArray();
      }
    }

    private void Frenzy(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
      if (sender && sender.inventory)
      {
        if (sender.HasBuff(AffixFrenziedBuff))
        {
          args.baseMoveSpeedAdd += 2f;
          args.attackSpeedMultAdd += 0.5f;
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
      if (buffDef == AffixFrenziedBuff)
        self.gameObject.AddComponent<FrenziedTeleportController>();
    }

    private void CharacterBody_OnBuffFinalStackLost(
  On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
  CharacterBody self, BuffDef buffDef)
    {
      orig(self, buffDef);
      if (buffDef == AffixFrenziedBuff)
        GameObject.Destroy(self.gameObject.GetComponent<FrenziedTeleportController>());
    }

    private void AddContent()
    {
      ItemDisplays itemDisplays = new ItemDisplays();
      ContentAddition.AddEliteDef(AffixFrenziedElite);
      ContentAddition.AddBuffDef(AffixFrenziedBuff);
      ItemAPI.Add(new CustomEquipment(AffixFrenziedEquipment, itemDisplays.CreateItemDisplayRules(celestineHalo, warbannerMat)));
    }

    private void SetupBuff()
    {
      AffixFrenziedBuff = ScriptableObject.CreateInstance<BuffDef>();
      AffixFrenziedBuff.name = "EliteFrenziedBuff";
      AffixFrenziedBuff.canStack = false;
      AffixFrenziedBuff.isCooldown = false;
      AffixFrenziedBuff.isDebuff = false;
      AffixFrenziedBuff.buffColor = AffixFrenziedLightColor;
      AffixFrenziedBuff.name = AffixFrenziedBuff.name;
      AffixFrenziedBuff.iconSprite = eliteIcon;
      (AffixFrenziedBuff as UnityEngine.Object).name = AffixFrenziedBuff.name;

    }

    private void SetupEquipment()
    {
      AffixFrenziedEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
      AffixFrenziedEquipment.appearsInMultiPlayer = true;
      AffixFrenziedEquipment.appearsInSinglePlayer = true;
      AffixFrenziedEquipment.canBeRandomlyTriggered = false;
      AffixFrenziedEquipment.canDrop = false;
      AffixFrenziedEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
      AffixFrenziedEquipment.cooldown = 10f;
      AffixFrenziedEquipment.isLunar = false;
      AffixFrenziedEquipment.isBoss = false;
      AffixFrenziedEquipment.passiveBuffDef = AffixFrenziedBuff;
      AffixFrenziedEquipment.dropOnDeathChance = affixDropChance;
      AffixFrenziedEquipment.enigmaCompatible = false;
      AffixFrenziedEquipment.pickupIconSprite = aspectIcon;
      AffixFrenziedEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixFrenzied", false);
      foreach (Renderer componentsInChild in AffixFrenziedEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
        componentsInChild.material = warbannerMat;
      AffixFrenziedEquipment.nameToken = "EQUIPMENT_AFFIX_Frenzied_NAME";
      AffixFrenziedEquipment.name = "AffixFrenziedNuxlar";

      AffixFrenziedEquipment.pickupToken = "Aspect of Frenzy";
      AffixFrenziedEquipment.descriptionToken = "Buffed move/atk speed";
      AffixFrenziedEquipment.loreToken = "";
    }

    private void SetupElite()
    {
      AffixFrenziedElite = ScriptableObject.CreateInstance<EliteDef>();
      AffixFrenziedElite.color = AffixFrenziedLightColor;
      AffixFrenziedElite.eliteEquipmentDef = AffixFrenziedEquipment;
      AffixFrenziedElite.modifierToken = "ELITE_MODIFIER_Frenzied";
      AffixFrenziedElite.name = "EliteFrenzied";
      (AffixFrenziedElite as ScriptableObject).name = "EliteFrenzied";
      AffixFrenziedElite.healthBoostCoefficient = healthMult;
      AffixFrenziedElite.damageBoostCoefficient = damageMult;
      AffixFrenziedBuff.eliteDef = AffixFrenziedElite;
    }

    private void AddLanguageTokens()
    {
      LanguageAPI.Add("ELITE_MODIFIER_Frenzied", "Frenzied {0}");
      LanguageAPI.Add("EQUIPMENT_AFFIX_Frenzied_NAME", "Frenzied Aspect");
    }

    public class FrenziedTeleportController : MonoBehaviour
    {
      private float fireTimer;
      private float fireInterval = 10f;
      private GameObject blinkPrefab = blinkEffect2;
      private CharacterBody body;
      private Vector3 blinkDestination = Vector3.zero;
      private Vector3 blinkStart = Vector3.zero;
      private float shortBlinkDistance = 25f;
      private float blinkDistance = 50f;

      private void Awake()
      {
        this.body = this.gameObject.GetComponent<CharacterBody>();
      }

      private void FixedUpdate()
      {
        if (this.body.isPlayerControlled)
          return;
        this.fireTimer += Time.fixedDeltaTime;
        if ((double)this.fireTimer < this.fireInterval)
          return;
        if (this.body.healthComponent && !this.body.healthComponent.alive)
          return;
        this.fireTimer %= 1;
        CalculateBlinkDestination();
        if (this.blinkStart == this.blinkDestination)
          return;
        StartCoroutine(Teleport());
      }

      public IEnumerator Teleport()
      {
        // Play_voidRaid_fall_return
        Util.PlaySound("Play_parent_teleport", this.gameObject);
        this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
        TeleportHelper.TeleportBody(this.body, this.blinkDestination);
        yield return new WaitForSeconds(0.33f);
        this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
      }

      private void CreateBlinkEffect(Vector3 origin)
      {
        if (!(bool)(UnityEngine.Object)this.blinkPrefab)
          return;
        EffectManager.SpawnEffect(this.blinkPrefab, new EffectData()
        {
          rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart),
          origin = origin,
          scale = this.body.radius
        }, false);
      }

      private void CalculateBlinkDestination()
      {
        Vector3 vector3 = Vector3.zero;
        Ray aimRay = (bool)(UnityEngine.Object)this.body.inputBank ? new Ray(this.body.inputBank.aimOrigin, this.body.inputBank.aimDirection) : new Ray(this.transform.position, this.transform.forward);
        BullseyeSearch bullseyeSearch = new BullseyeSearch();
        bullseyeSearch.searchOrigin = aimRay.origin;
        bullseyeSearch.searchDirection = aimRay.direction;
        bullseyeSearch.maxDistanceFilter = this.blinkDistance;
        bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
        bullseyeSearch.filterByLoS = false;
        bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(this.gameObject));
        bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
        bullseyeSearch.RefreshCandidates();
        HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
        if ((bool)(UnityEngine.Object)hurtBox && !this.body.isPlayerControlled)
        {
          Vector3 center = hurtBox.transform.position - this.transform.position;
          float radius = 15f;
          vector3 = center + (Vector3)(radius * UnityEngine.Random.insideUnitCircle);
        }
        else if (this.body.inputBank)
          vector3 = this.body.inputBank.moveVector * this.shortBlinkDistance;
        this.blinkDestination = this.transform.position;
        this.blinkStart = this.transform.position;
        NodeGraph nodes = this.body.isFlying ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes;
        if (this.body.name == "MinorConstructBody(Clone)")
          nodes = SceneInfo.instance.groundNodes;
        nodes.GetNodePosition(nodes.FindClosestNode(this.transform.position + vector3, this.body.hullClassification), out this.blinkDestination);
        this.blinkDestination += this.transform.position - this.body.footPosition;
        if (this.body.characterDirection)
          this.body.characterDirection.forward = vector3;
      }
    }
  }
}
