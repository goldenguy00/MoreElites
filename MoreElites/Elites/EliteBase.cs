using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace MoreElites.Elites
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
            Invalid,
            T1,
            T1Honor,
            T1Upgrade,
            T1UpgradeHonor,
            T2,
            Lunar
        }

        public static float affixDropChance = 0.00025f;
        public static List<EliteBase> EliteInstances = [];

        public abstract string Name { get; }
        public abstract string EquipmentName { get; }
        public abstract string DescriptionText { get; }
        public abstract string PickupText { get; }
        public abstract string LoreText { get; }

        public abstract EliteTier EliteTierDef { get; }
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
            if (PluginConfig.enableEcho.Value)
                new Echo();
            if (PluginConfig.enableEmpowering.Value)
                new Empowering();
            if (PluginConfig.enableFrenzied.Value)
                new Frenzied();
            if (PluginConfig.enableVolatile.Value)
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
            eliteEquipmentDef.dropOnDeathChance = affixDropChance;
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

        public virtual CustomElite SetupElite()
        {
            var tierDefs = this.GetVanillaEliteTierDef(this.EliteTierDef);
            if (tierDefs is null)
                return null;

            var customElite = new CustomElite($"Elite{this.Name}", this.EliteEquipmentDef, this.EliteColor, $"ELITE_MODIFIER_{this.Name}", tierDefs, this.EliteRamp);
            if (this.EliteTierDef < EliteTier.T2)
            {
                tierDefs = this.GetVanillaEliteTierDef(this.EliteTierDef + 1);
                if (tierDefs is not null)
                {
                    var customHonorElite = new CustomElite($"Elite{this.Name}Honor", this.EliteEquipmentDef, this.EliteColor, $"ELITE_MODIFIER_{this.Name}", tierDefs, this.EliteRamp);

                    customHonorElite.EliteDef.healthBoostCoefficient = PluginConfig.t1HonorHealthMult.Value;
                    customHonorElite.EliteDef.damageBoostCoefficient = PluginConfig.t1HonorDamageMult.Value;

                    EliteAPI.Add(customHonorElite);
                }

                customElite.EliteDef.healthBoostCoefficient = PluginConfig.t1HealthMult.Value;
                customElite.EliteDef.damageBoostCoefficient = PluginConfig.t1DamageMult.Value;
            }
            else
            {
                customElite.EliteDef.healthBoostCoefficient = PluginConfig.t2HealthMult.Value;
                customElite.EliteDef.damageBoostCoefficient = PluginConfig.t2DamageMult.Value;
            }

            EliteAPI.Add(customElite);

            return customElite;
        }

        private IEnumerable<CombatDirector.EliteTierDef> GetVanillaEliteTierDef(EliteTier tier)
        {
            // 0 - none
            // 1 - t1
            // 2 - t1 honor
            // 3 - t1 + gold
            // 4 - t1 + gold honor
            // 5 - t2
            // 6 - lunar

            if (tier == EliteTier.Invalid)
            {
                Log.Error("Invalid tier");
                Log.Debug(new System.Diagnostics.StackTrace());

                return null;
            }

            List<CombatDirector.EliteTierDef> tierDefs = [EliteAPI.VanillaEliteTiers[(int)tier]];

            if (this.EliteTierDef is EliteTier.T1 or EliteTier.T1Honor)
                tierDefs.Add(EliteAPI.VanillaEliteTiers[(int)tier + 2]);

            return tierDefs;
        }
    }
}
