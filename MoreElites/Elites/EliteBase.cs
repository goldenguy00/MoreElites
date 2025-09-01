using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace MoreElites
{
    public abstract class EliteBase<T> : EliteBase where T : EliteBase<T>
    {
        public static T Instance { get; private set; }

        public EliteBase()
        {
            if (Instance != null) 
                throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");

            Instance = this as T;
            EliteInstances.Add(Instance);
        }
    }

    public abstract class EliteBase
    {
        public enum EliteTier
        {
            None,
            T1,
            T1Honor,
            T1GuildedHonor,
            T1Guilded,
            T2,
            Lunar
        }

        public static float affixDropChance = 0.00025f;
        public static List<EliteBase> EliteInstances = [];

        public abstract string Name { get; }
        public virtual string NameToken => this.Name.ToUpper() + "_SCORE";
        public abstract string EquipmentName { get; }
        public abstract string DescriptionText { get; }
        public abstract string PickupText { get; }
        public abstract string LoreText { get; }

        public abstract EliteTier EliteTierDef { get; }
        public abstract Color EliteColor { get; }
        public abstract Texture2D EliteRamp { get; set; }
        public abstract Sprite EliteIcon { get; set; }
        public abstract Sprite AspectIcon { get; set; }

        public abstract Material EliteMaterial { get; set; }
        public abstract GameObject PickupModelPrefab { get; set; }

        public virtual BuffDef EliteBuffDef { get; set; }
        public virtual CustomEquipment CustomEquipmentDef { get; set; }
        public virtual CustomElite CustomEliteDef { get; set; }
        public virtual CustomElite CustomEliteDefHonor { get; set; }

        public float HealthBoostCoefficient => this.EliteTierDef < EliteTier.T2 ? PluginConfig.t1HealthMult.Value : PluginConfig.t2HealthMult.Value;
        public float DamageBoostCoefficient => this.EliteTierDef < EliteTier.T2 ? PluginConfig.t1DamageMult.Value : PluginConfig.t2DamageMult.Value;

        public abstract void OnBuffGained(CharacterBody body);
        public abstract void OnBuffLost(CharacterBody body);

        public static void CreateElites()
        {
            if (EliteInstances.Any())
            {
                foreach (var elite in EliteBase.EliteInstances)
                {
                    elite.Init();
                }

                On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
                On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            }
        }

        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (!buffDef)
                return;

            foreach (var elite in EliteInstances)
            {
                if (elite?.EliteBuffDef == buffDef)
                {
                    elite.OnBuffGained(self);
                    return;
                }
            }
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (!buffDef)
                return;

            foreach (var elite in EliteInstances)
            {
                if (elite?.EliteBuffDef == buffDef)
                {
                    elite.OnBuffLost(self);
                    return;
                }
            }
        }

        public virtual void Init()
        {
            Log.Info($"Initializing elite {this.Name}");

            this.AddLanguageTokens();
            this.SetupAssets();
            this.SetupBuff();
            this.SetupEquipment();
            this.SetupElite();
        }

        public virtual void AddLanguageTokens()
        {
            LanguageAPI.Add($"ELITE_MODIFIER_{this.NameToken}", this.Name + " {0}");

            LanguageAPI.Add($"EQUIPMENT_AFFIX_{this.NameToken}_NAME", this.EquipmentName ?? $"{this.Name} Aspect");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{this.NameToken}_DESC", this.DescriptionText ?? "");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{this.NameToken}_LORE", this.LoreText ?? "");
            LanguageAPI.Add($"EQUIPMENT_AFFIX_{this.NameToken}_PICKUP_DESC", this.PickupText ?? $"Aspect of {this.Name}");
        }

        public virtual void SetupAssets() { }

        public virtual void SetupBuff()
        {
            this.EliteBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            this.EliteBuffDef.name = $"Affix{this.Name}Buff";
            this.EliteBuffDef.canStack = false;
            this.EliteBuffDef.isCooldown = false;
            this.EliteBuffDef.isDebuff = false;
            this.EliteBuffDef.buffColor = EliteColor;
            this.EliteBuffDef.iconSprite = EliteIcon;

            ContentAddition.AddBuffDef(this.EliteBuffDef);
        }

        public virtual void SetupEquipment()
        {
            this.CustomEquipmentDef = new CustomEquipment(
                name: $"Affix{this.Name}",
                nameToken: $"EQUIPMENT_AFFIX_{this.NameToken}_NAME",
                descriptionToken: $"EQUIPMENT_AFFIX_{this.NameToken}_DESC",
                loreToken: $"EQUIPMENT_AFFIX_{this.NameToken}_LORE",
                pickupToken: $"EQUIPMENT_AFFIX_{this.NameToken}_PICKUP_DESC",
                pickupIconSprite: this.AspectIcon,
                pickupModelPrefab: this.PickupModelPrefab,
                cooldown: 0f,
                canDrop: false,
                enigmaCompatible: false,
                isBoss: false,
                isLunar: false,
                passiveBuffDef: this.EliteBuffDef,
                unlockableDef: null,
                colorIndex: ColorCatalog.ColorIndex.Equipment,
                appearsInMultiPlayer: true,
                appearsInSinglePlayer: true,
                itemDisplayRules: (ItemDisplayRuleDict)null);

            foreach (var componentsInChild in this.CustomEquipmentDef.EquipmentDef.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = this.EliteMaterial;

            ItemAPI.Add(this.CustomEquipmentDef);
        }

        public virtual void SetupElite()
        {
            var tierDefs = this.GetVanillaEliteTierDef(this.EliteTierDef);
            if (tierDefs is null)
                return;

            this.CustomEliteDef = new CustomElite($"Elite{this.Name}", this.CustomEquipmentDef.EquipmentDef, this.EliteColor, $"ELITE_MODIFIER_{this.NameToken}", tierDefs, this.EliteRamp);
            this.CustomEliteDef.EliteDef.healthBoostCoefficient = this.HealthBoostCoefficient;
            this.CustomEliteDef.EliteDef.damageBoostCoefficient = this.DamageBoostCoefficient;

            this.EliteBuffDef.eliteDef = this.CustomEliteDef.EliteDef;
            EliteAPI.Add(this.CustomEliteDef);

            var honorTierDefs = this.GetVanillaEliteHonorTierDef(this.EliteTierDef);
            if (honorTierDefs is not null)
            {
                this.CustomEliteDefHonor = new CustomElite($"Elite{this.Name}Honor", this.CustomEquipmentDef.EquipmentDef, this.EliteColor, $"ELITE_MODIFIER_{this.NameToken}", honorTierDefs, this.EliteRamp);
                this.CustomEliteDefHonor.EliteDef.healthBoostCoefficient = PluginConfig.t1HonorHealthMult.Value;
                this.CustomEliteDefHonor.EliteDef.damageBoostCoefficient = PluginConfig.t1HonorDamageMult.Value;
                EliteAPI.Add(this.CustomEliteDefHonor);
            }
        }

        // 0 - none
        // 1 - t1
        // 2 - t1 honor
        // 3 - t1 + gold honor
        // 4 - t1 + gold
        // 5 - t2
        // 6 - lunar
        internal IEnumerable<CombatDirector.EliteTierDef> GetVanillaEliteTierDef(EliteTier tier) => tier switch
        {
            EliteTier.None => null,
            EliteTier.T1 => [EliteAPI.VanillaEliteTiers[(int)EliteTier.T1], EliteAPI.VanillaEliteTiers[(int)EliteTier.T1Guilded]],
            EliteTier.T1Honor => [EliteAPI.VanillaEliteTiers[(int)EliteTier.T1Honor], EliteAPI.VanillaEliteTiers[(int)EliteTier.T1GuildedHonor]],
            _ => [EliteAPI.VanillaEliteTiers[(int)tier]]
        };

        internal IEnumerable<CombatDirector.EliteTierDef> GetVanillaEliteHonorTierDef(EliteTier tier) => tier switch
        {
            EliteTier.T1 => [EliteAPI.VanillaEliteTiers[(int)EliteTier.T1Honor], EliteAPI.VanillaEliteTiers[(int)EliteTier.T1GuildedHonor]],
            EliteTier.T1Guilded => [EliteAPI.VanillaEliteTiers[(int)EliteTier.T1GuildedHonor]],
            _ => null
        };
    }
}
