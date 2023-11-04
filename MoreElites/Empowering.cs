using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreElites
{
  public class Empowering
  {
    public static Color AffixEmpoweringColor = new Color(1f, 0.5f, 0.0f);
    public static EquipmentDef AffixEmpoweringEquipment;
    public static BuffDef AffixEmpoweringBuff;
    public static EliteDef AffixEmpoweringElite;
    public static float healthMult = 4f;
    public static float damageMult = 2f;
    public static float affixDropChance = 0.00025f;
    private static GameObject EmpoweringWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "EmpoweringWard");
    private static Material empoweringMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffRing.mat").WaitForCompletion();
    private static Texture2D eliteRamp = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMagmaWorm.png").WaitForCompletion();
    private static Sprite eliteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
    private static Sprite aspectIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/EliteEarth/texAffixEarthIcon.png").WaitForCompletion();
    // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 

    public Empowering()
    {
      EmpoweringWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = empoweringMat;
      this.AddLanguageTokens();
      this.SetupBuff();
      this.SetupEquipment();
      this.SetupElite();
      this.AddContent();
      EliteRamp.AddRamp(AffixEmpoweringElite, eliteRamp);
      ContentAddition.AddEquipmentDef(AffixEmpoweringEquipment);
      On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
      On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
      On.RoR2.CombatDirector.Init += CombatDirector_Init;
    }

    private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
    {
      orig();
      if (EliteAPI.VanillaEliteTiers.Length > 2)
      {
        // HONOR
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        AffixEmpoweringElite.healthBoostCoefficient = 2.5f;
        AffixEmpoweringElite.damageBoostCoefficient = 1.5f;
        elites.Add(AffixEmpoweringElite);
        targetTier.eliteTypes = elites.ToArray();
      }
      if (EliteAPI.VanillaEliteTiers.Length > 1)
      {
        CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
        List<EliteDef> elites = targetTier.eliteTypes.ToList();
        AffixEmpoweringElite.healthBoostCoefficient = 4f;
        AffixEmpoweringElite.damageBoostCoefficient = 2f;
        elites.Add(AffixEmpoweringElite);
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
      if (buffDef == AffixEmpoweringBuff)
      {
        GameObject gameObject = Object.Instantiate<GameObject>(EmpoweringWard);
        BuffWard component = gameObject.GetComponent<BuffWard>();
        gameObject.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
        component.buffDef = RoR2Content.Buffs.Warbanner;
        component.Networkradius = 25f + self.radius;
        gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
      }
    }

    private void CharacterBody_OnBuffFinalStackLost(
  On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
  CharacterBody self, BuffDef buffDef)
    {
      orig(self, buffDef);
      if (buffDef == AffixEmpoweringBuff)
      {
        BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
        Object.Destroy(buffWard);
      }
    }

    private void AddContent()
    {
      ItemDisplays itemDisplays = new ItemDisplays();
      ContentAddition.AddEliteDef(AffixEmpoweringElite);
      ContentAddition.AddBuffDef(AffixEmpoweringBuff);
    }

    private void SetupBuff()
    {
      AffixEmpoweringBuff = ScriptableObject.CreateInstance<BuffDef>();
      AffixEmpoweringBuff.name = "EliteEmpoweringBuff";
      AffixEmpoweringBuff.canStack = false;
      AffixEmpoweringBuff.isCooldown = false;
      AffixEmpoweringBuff.isDebuff = false;
      AffixEmpoweringBuff.buffColor = AffixEmpoweringColor;
      AffixEmpoweringBuff.iconSprite = eliteIcon;
      (AffixEmpoweringBuff as UnityEngine.Object).name = AffixEmpoweringBuff.name;
    }

    private void SetupEquipment()
    {
      AffixEmpoweringEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
      AffixEmpoweringEquipment.appearsInMultiPlayer = true;
      AffixEmpoweringEquipment.appearsInSinglePlayer = true;
      AffixEmpoweringEquipment.canBeRandomlyTriggered = false;
      AffixEmpoweringEquipment.canDrop = false;
      AffixEmpoweringEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
      AffixEmpoweringEquipment.cooldown = 0.0f;
      AffixEmpoweringEquipment.isLunar = false;
      AffixEmpoweringEquipment.isBoss = false;
      AffixEmpoweringEquipment.passiveBuffDef = AffixEmpoweringBuff;
      AffixEmpoweringEquipment.dropOnDeathChance = affixDropChance;
      AffixEmpoweringEquipment.enigmaCompatible = false;
      AffixEmpoweringEquipment.pickupIconSprite = aspectIcon;
      AffixEmpoweringEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixEmpowering", false);
      foreach (Renderer componentsInChild in AffixEmpoweringEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
        componentsInChild.material = empoweringMat;
      AffixEmpoweringEquipment.nameToken = "EQUIPMENT_AFFIX_EMPOWERING_NAME";
      AffixEmpoweringEquipment.name = "AffixEmpowering";

      AffixEmpoweringEquipment.pickupToken = "Aspect of Empowering";
      AffixEmpoweringEquipment.descriptionToken = "Buffed move/atk speed of allies";
      AffixEmpoweringEquipment.loreToken = "";
    }

    private void SetupElite()
    {
      AffixEmpoweringElite = ScriptableObject.CreateInstance<EliteDef>();
      AffixEmpoweringElite.color = AffixEmpoweringColor;
      AffixEmpoweringElite.eliteEquipmentDef = AffixEmpoweringEquipment;
      AffixEmpoweringElite.modifierToken = "ELITE_MODIFIER_EMPOWERING";
      AffixEmpoweringElite.name = "EliteEmpowering";
      AffixEmpoweringElite.healthBoostCoefficient = healthMult;
      AffixEmpoweringElite.damageBoostCoefficient = damageMult;
      AffixEmpoweringBuff.eliteDef = AffixEmpoweringElite;
      (AffixEmpoweringElite as ScriptableObject).name = "EliteEmpowering";
    }

    private void AddLanguageTokens()
    {
      LanguageAPI.Add("ELITE_MODIFIER_EMPOWERING", "Empowering {0}");
      LanguageAPI.Add("EQUIPMENT_AFFIX_EMPOWERING_NAME", "Empowering Aspect");
    }
  }
}
