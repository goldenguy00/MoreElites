using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MoreElites.Elites;

namespace MoreElites
{
    public abstract class EliteBase<T> : EliteBase where T : EliteBase<T>
    {
        public static T Instance { get; private set; }

        public EliteBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;

            Instance.Init();
            EliteInstances.Add(Instance);
        }
    }

    public abstract class EliteBase
    {
        public enum EliteTier
        {
            T1,
            T1Upgrade,
            T2,
        }

        public static float affixDropChance = 0.00025f;
        public static List<EliteBase> EliteInstances = [];

        public abstract string Name { get; }
        public abstract string EquipmentName { get; }
        public abstract string DescriptionText { get; }
        public abstract string PickupText { get; }
        public abstract string LoreText { get; }
        
        public abstract EliteTier EliteTierDefs { get; }
        public abstract Color EliteColor { get; }
        public abstract Texture2D EliteRamp { get; }
        public abstract Sprite EliteIcon { get; }
        public abstract Sprite AspectIcon { get; }

        public abstract Material EliteMaterial { get; set; }
        public abstract GameObject PickupModelPrefab { get; set; }

        public virtual BuffDef EliteBuffDef { get; set; }
        public virtual EquipmentDef EliteEquipmentDef { get; set; }
        public virtual CustomElite CustomEliteDef { get; set; }
        public virtual CustomElite CustomEliteDefHonor { get; set; }

        public abstract void OnBuffGained(CharacterBody body);
        public abstract void OnBuffLost(CharacterBody body);

        public static void CreateElites()
        {
            if (MoreElites.enableEcho.Value)
                new Echo();
            if (MoreElites.enableEmpowering.Value)
                new Empowering();
            if (MoreElites.enableFrenzied.Value)
                new Frenzied();
            if (MoreElites.enableVolatile.Value)
                new Volatile();

            if (EliteInstances.Any())
            {
                On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
                On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            }
        }

        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (buffDef)
            {
                foreach (var elite in EliteInstances)
                {
                    if (elite?.EliteBuffDef == buffDef)
                        elite.OnBuffGained(self);
                }
            }
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (buffDef)
            {
                foreach (var elite in EliteInstances)
                {
                    if (elite?.EliteBuffDef == buffDef)
                        elite.OnBuffLost(self);
                }
            }
        }

        public virtual void Init()
        {
            Log.Warning($"Initializing elite {this.Name}");

            this.AddLanguageTokens();

            this.EliteBuffDef = this.SetupBuff();
            this.EliteEquipmentDef = this.SetupEquipment();
            this.CustomEliteDef = this.SetupElite();
            this.CustomEliteDefHonor = this.SetupElite(true);

            this.EliteBuffDef.eliteDef = this.CustomEliteDef.EliteDef;

        }

        public virtual void AddLanguageTokens()
        {
            var name = this.Name;

            LanguageAPI.Add($"ELITE_MODIFIER_{name}", name + " {0}");

            LanguageAPI.Add($"EQUIPMENT_AFFIX_{name}_NAME", this.EquipmentName ?? $"{name} Aspect");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{name}_PICKUP_DESC", this.PickupText ?? $"Aspect of {name}");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{name}_DESC", this.DescriptionText ?? "");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{name}_LORE", this.LoreText ?? "");
        }

        public virtual BuffDef SetupBuff()
        {
            var eliteBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            eliteBuffDef.name = $"Affix{this.Name}Buff";
            eliteBuffDef.canStack = false;
            eliteBuffDef.isCooldown = false;
            eliteBuffDef.isDebuff = false;
            eliteBuffDef.buffColor = EliteColor;
            eliteBuffDef.iconSprite = EliteIcon;

            ContentAddition.AddBuffDef(eliteBuffDef);

            return eliteBuffDef;
        }

        public virtual EquipmentDef SetupEquipment()
        {
            var eliteEquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            eliteEquipmentDef.appearsInMultiPlayer = true;
            eliteEquipmentDef.appearsInSinglePlayer = true;
            eliteEquipmentDef.canBeRandomlyTriggered = false;
            eliteEquipmentDef.canDrop = false;
            eliteEquipmentDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
            eliteEquipmentDef.cooldown = 0.0f;
            eliteEquipmentDef.isLunar = false;
            eliteEquipmentDef.isBoss = false;
            eliteEquipmentDef.dropOnDeathChance = EliteBase.affixDropChance;
            eliteEquipmentDef.enigmaCompatible = false;

            eliteEquipmentDef.passiveBuffDef = this.EliteBuffDef;
            eliteEquipmentDef.pickupIconSprite = this.AspectIcon;
            eliteEquipmentDef.pickupModelPrefab = this.PickupModelPrefab;

            foreach (var componentsInChild in eliteEquipmentDef.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = this.EliteMaterial;

            eliteEquipmentDef.name = $"Affix{this.Name}";
            eliteEquipmentDef.nameToken = $"EQUIPMENT_AFFIX_{this.Name}_NAME";
            eliteEquipmentDef.descriptionToken = $"EQUIPMENT_AFFIX_{this.Name}_DESC";
            eliteEquipmentDef.pickupToken = $"EQUIPMENT_AFFIX_{this.Name}_PICKUP_DESC";
            eliteEquipmentDef.loreToken = $"EQUIPMENT_AFFIX_{this.Name}_LORE";
            
            ContentAddition.AddEquipmentDef(eliteEquipmentDef);
            return eliteEquipmentDef;
        }

        public virtual CustomElite SetupElite(bool honor = false)
        {
            if (this.EliteTierDefs == EliteTier.T2 && honor)
                return null;

            var customElite = new CustomElite($"Elite{this.Name}", this.EliteEquipmentDef, this.EliteColor, $"ELITE_MODIFIER_{this.Name}", null, this.EliteRamp);

            switch(this.EliteTierDefs)
            {
                default:
                    customElite.EliteTierDefs = EliteBase.GetEliteTierDef($"RoR2/Base/EliteLightning/edLightning{(honor ? "Honor" : string.Empty)}.asset");
                    customElite.EliteTierDefs ??= honor ? [EliteAPI.VanillaEliteOnlyFirstTierDef] : [EliteAPI.VanillaFirstTierDef];

                    customElite.EliteDef.healthBoostCoefficient = MoreElites.t1HealthMult.Value;
                    customElite.EliteDef.damageBoostCoefficient = MoreElites.t1DamageMult.Value;
                    break;

                case EliteTier.T1Upgrade:
                    customElite.EliteTierDefs = EliteBase.GetEliteTierDef($"RoR2/DLC2/Elites/EliteAurelionite/edAurelionite{(honor ? "Honor" : string.Empty)}.asset");
                    customElite.EliteTierDefs ??= honor ? [EliteAPI.VanillaEliteOnlyFirstTierDef] : [EliteAPI.VanillaFirstTierDef];

                    customElite.EliteDef.healthBoostCoefficient = MoreElites.t1HealthMult.Value;
                    customElite.EliteDef.damageBoostCoefficient = MoreElites.t1DamageMult.Value;
                    break;

                case EliteTier.T2:
                    customElite.EliteTierDefs = EliteBase.GetEliteTierDef("RoR2/Base/ElitePoison/edPoison.asset");
                    customElite.EliteTierDefs ??= [EliteAPI.VanillaEliteTiers[5]];

                    customElite.EliteDef.healthBoostCoefficient = MoreElites.t2HealthMult.Value;
                    customElite.EliteDef.damageBoostCoefficient = MoreElites.t2DamageMult.Value;
                    break;
            }

            EliteAPI.Add(customElite);

            return customElite;
        }

        private static bool IsValid(EliteDef ed)
        {
            return ed &&
                   ed.eliteEquipmentDef &&
                   ed.eliteEquipmentDef.passiveBuffDef &&
                   ed.eliteEquipmentDef.passiveBuffDef.isElite;
        }


        private static IEnumerable<CombatDirector.EliteTierDef> GetEliteTierDef(string selector)
        {
            var affix = Addressables.LoadAssetAsync<EliteDef>(selector).WaitForCompletion();

            if (affix)
            {
                var tiers = EliteAPI.VanillaEliteTiers.Where(etd => etd.eliteTypes.Where(IsValid).Contains(affix));

                if (tiers.Any())
                    return tiers;
            }
            else
            {
                Log.Error($"Affix {selector} is null!");
            }

            Log.Error($"Unable to find vanilla tier with key {selector}. Falling back to default index!");

            return null;
        }
    }
}
