using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreElites
{
  public class ItemDisplays
  {
    public GameObject ItemBodyModelPrefab;

    public static CharacterModel.RendererInfo[] ItemDisplaySetup(
      GameObject obj,
      bool debugmode = false)
    {
      List<Renderer> rendererList = new List<Renderer>();
      MeshRenderer[] componentsInChildren1 = obj.GetComponentsInChildren<MeshRenderer>();
      if ((uint)componentsInChildren1.Length > 0U)
        rendererList.AddRange((IEnumerable<Renderer>)componentsInChildren1);
      SkinnedMeshRenderer[] componentsInChildren2 = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
      if ((uint)componentsInChildren2.Length > 0U)
        rendererList.AddRange((IEnumerable<Renderer>)componentsInChildren2);
      CharacterModel.RendererInfo[] rendererInfoArray = new CharacterModel.RendererInfo[rendererList.Count];
      for (int index = 0; index < rendererList.Count; ++index)
        rendererInfoArray[index] = new CharacterModel.RendererInfo()
        {
          defaultMaterial = rendererList[index] is SkinnedMeshRenderer ? rendererList[index].sharedMaterial : rendererList[index].material,
          renderer = rendererList[index],
          defaultShadowCastingMode = ShadowCastingMode.On,
          ignoreOverlays = false
        };
      return rendererInfoArray;
    }

    public ItemDisplayRuleDict CreateItemDisplayRules(
      GameObject prefab,
      Material material)
    {
      GameObject gameObject = prefab;
      gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = material;
      gameObject.AddComponent<ItemDisplay>();
      gameObject.GetComponent<ItemDisplay>().rendererInfos = ItemDisplays.ItemDisplaySetup(gameObject);
      this.ItemBodyModelPrefab = gameObject;
      ItemDisplayRuleDict itemDisplayRules = new ItemDisplayRuleDict(Array.Empty<ItemDisplayRule>());
      itemDisplayRules.Add("mdlCommandoDualies", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.0017f, 0.45426f, -0.00889f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.095f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlHuntress", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.00237f, 0.34538f, -0.06892f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlToolbot", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.1519f, 2.71484f, 2.21381f),
          localAngles = new Vector3(60f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlEngi", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "HeadCenter",
          localPos = new Vector3(-0.00053f, 0.52744f, 0.08005f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.13f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlMage", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.0125f, 0.23135f, -0.03155f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlMerc", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.01953f, 0.31854f, 0.01433f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlLoader", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.00151f, 0.28744f, -0.00129f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlCroco", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.02466f, 0.46623f, 2.35514f),
          localAngles = new Vector3(70.00002f, 180f, 180f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlCaptain", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.01106f, 0.28928f, -0.0017f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlBandit2", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.01673f, 0.27646f, -0.00129f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlEquipmentDrone", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "HeadCenter",
          localPos = new Vector3(0.0f, 0.0f, 1.09378f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.3f, 0.3f, 0.3f)
        }
      });
      itemDisplayRules.Add("mdlWarframeWisp(Clone)", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.00284f, 0.25323f, -0.07018f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlRailGunner", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.00025f, 0.24998f, -0.01575f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.02781f, 0.27557f, 0.02447f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlHeretic", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.09251f, 0.05643f, -0.01722f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlBeetle", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.02673f, 0.62368f, 0.43346f),
          localAngles = new Vector3(45f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("AcidLarva", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "BodyBase",
          localPos = new Vector3(-0.55948f, 5.77417f, -0.33291f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlBeetleGuard", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.01662f, -0.13404f, 2.86457f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlBeetleQueen", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.06181f, 4.40589f, 0.24246f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlBell", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Chain",
          localPos = new Vector3(0.0f, -1.43253f, 0.0f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.3f, 0.3f, 0.3f)
        }
      });
      itemDisplayRules.Add("mdlBison", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.01535f, 0.14477f, 0.84346f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlBrother", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.05811f, 0.33004f, -0.01252f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.15f, 0.15f, 0.15f)
        }
      });
      itemDisplayRules.Add("mdlClayBoss", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "PotLidTop",
          localPos = new Vector3(0.0f, 1.43864f, 1.25631f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.3f, 0.3f, 0.3f)
        }
      });
      itemDisplayRules.Add("mdlClayBruiser", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.00255f, 0.59611f, 0.08158f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlClayGrenadier", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.02701f, 0.31263f, 0.02993f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlMagmaWorm", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-2E-05f, -2.48183f, -0.51458f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.3f, 0.3f, 0.3f)
        }
      });
      itemDisplayRules.Add("mdlFlyingVermin", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Body",
          localPos = new Vector3(0.0f, 1.77686f, 0.11532f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlGolem", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.0f, 1.39084f, -0.01647f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.3f, 0.3f, 0.3f)
        }
      });
      itemDisplayRules.Add("mdlGrandparent", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.0854f, 7.61769f, -0.17647f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.8f, 0.8f, 0.8f)
        }
      });
      itemDisplayRules.Add("mdlGravekeeper", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.1399f, 2.9322f, 0.53341f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlGreaterWisp", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "MaskBase",
          localPos = new Vector3(0.0f, 1.7257f, 0.0f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlGup", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "MainBody2",
          localPos = new Vector3(0.12951f, 1.5392f, 0.08611f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlHermitCrab", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Base",
          localPos = new Vector3(1E-05f, 2.24121f, -1E-05f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlImp", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Neck",
          localPos = new Vector3(0.0f, 0.42651f, 0.05615f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlImpBoss", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Neck",
          localPos = new Vector3(0.0f, 1.75499f, -0.04981f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.35f, 0.35f, 0.35f)
        }
      });
      itemDisplayRules.Add("mdlJellyfish", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Hull2",
          localPos = new Vector3(0.11887f, 1.8858f, 0.11731f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlLemurian", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.0f, 1.74816f, -2.29201f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlLemurianBruiser", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.0f, 0.07847f, 2.41218f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlMiniMushroom", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-1.0948f, 0.0f, 0.0f),
          localAngles = new Vector3(0.0f, 0.0f, 90f),
          localScale = new Vector3(0.25f, 0.25f, 0.25f)
        }
      });
      itemDisplayRules.Add("mdlMinorConstruct", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "CapTop",
          localPos = new Vector3(0.0f, 1.25985f, -0.02785f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlNullifier", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Center",
          localPos = new Vector3(0.0f, 3.32941f, 0.0f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.6f, 0.6f, 0.6f)
        }
      });
      itemDisplayRules.Add("mdlParent", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-112.8285f, 151.4217f, 2E-05f),
          localAngles = new Vector3(0.0f, 0.0f, 45f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlRoboBallBoss", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "MainEyeMuzzle",
          localPos = new Vector3(0.0f, 0.0f, 0.0f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlRoboBallMini", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Muzzle",
          localPos = new Vector3(0.0f, 0.09907f, 0.02346f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlScav", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(2E-05f, -5.75954f, -13.77031f),
          localAngles = new Vector3(80.00003f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlTitan", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.39177f, 8.86227f, -0.16581f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlVagrant", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Hull",
          localPos = new Vector3(0.0f, 2.11688f, 0.0f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.35f, 0.35f, 0.35f)
        }
      });
      itemDisplayRules.Add("mdlVermin", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.0f, -0.92722f, -0.94452f),
          localAngles = new Vector3(15f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlVoidBarnacle", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-0.36053f, 0.04626f, -1.10482f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.1f, 0.1f, 0.1f)
        }
      });
      itemDisplayRules.Add("mdlVoidJailer", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-1.13059f, 0.0642f, -1.17159f),
          localAngles = new Vector3(90f, 225f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      itemDisplayRules.Add("mdlVoidMegaCrab", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "BodyBase",
          localPos = new Vector3(-0.00216f, 9.20985f, -0.23349f),
          localAngles = new Vector3(0.0f, 0.0f, 0.0f),
          localScale = new Vector3(0.5f, 0.5f, 0.5f)
        }
      });
      itemDisplayRules.Add("mdlVulture", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(-2E-05f, 1.43721f, -4.09087f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.35f, 0.35f, 0.35f)
        }
      });
      itemDisplayRules.Add("mdlWisp1Mouth", new ItemDisplayRule[1]
      {
        new ItemDisplayRule()
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = this.ItemBodyModelPrefab,
          childName = "Head",
          localPos = new Vector3(0.0f, 0.0f, 1.57299f),
          localAngles = new Vector3(90f, 0.0f, 0.0f),
          localScale = new Vector3(0.2f, 0.2f, 0.2f)
        }
      });
      return itemDisplayRules;
    }
  }
}
