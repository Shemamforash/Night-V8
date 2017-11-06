using System;
using Facilitating.Audio;
using Game.Characters;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Gear.UI;
using Game.World;
using Game.World.WorldEvents;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponClass WeaponClass;
        public readonly WeaponModifier SubClass, SecondaryModifier;
        public readonly bool Automatic;
        public bool Cocked = true;
        public readonly MyValue AmmoInMagazine = new MyValue(0);
        public readonly MyValue Durability;
        private const int MaxDurability = 20;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;
        public readonly int Capacity, Pellets;

        public Weapon(WeaponClass weaponClass, WeaponModifier subClass, WeaponModifier secondaryModifier, bool automatic, float weight, int durability) : base(weaponClass.Type.ToString(), weight,
            GearSubtype.Weapon)
        {
            WeaponClass = weaponClass;
            SubClass = subClass;
            SecondaryModifier = secondaryModifier;
            Automatic = automatic;

            Durability = new MyValue(durability, 0, MaxDurability);
            Durability.OnMin(() => { _canEquip = false; });
            Capacity = (int) Math.Ceiling((double) subClass.Capacity * secondaryModifier.CapacityModifier);
            Pellets = (int) Math.Ceiling((double) (subClass.Pellets * secondaryModifier.Pellets));

            if (!automatic)
            {
//                Damage *= 2;
//                AmmoInMagazine.Max = (int) Mathf.Ceil(AmmoInMagazine.Max / 2f);
//                Accuracy *= 1.5f;
//                Mathf.Clamp(Accuracy, 0, 100);
//                ReloadSpeed /= 2f;
            }
            WeaponAttributes = new WeaponAttributes(this);
            AmmoInMagazine.Max = Capacity;
#if UNITY_EDITOR
//            Print();
#endif
            UpdateDurability();
            WorldEventManager.GenerateEvent(new WeaponFindEvent(Name));
        }

        private void Print()
        {
            Debug.Log(WeaponClass.Type + " " + SubClass.Name
                      + "\nAutomatic:  " + Automatic
                      + "\nDurability: " + Durability.GetCurrentValue()
                      + "\nAmmo Left:  " + AmmoInMagazine.GetCurrentValue()
                      + "\nCapacity:   " + Capacity
                      + "\nPellets:    " + Pellets
                      + "\nDamage:     " + WeaponAttributes.Damage.GetCalculatedValue()
                      + "\nAccuracy:   " + WeaponAttributes.Accuracy.GetCalculatedValue()
                      + "\nFire Rate:  " + WeaponAttributes.FireRate.GetCalculatedValue()
                      + "\nHandling:   " + WeaponAttributes.Handling.GetCalculatedValue()
                      + "\nReload:     " + WeaponAttributes.ReloadSpeed.GetCalculatedValue()
                      + "\nCritChance: " + WeaponAttributes.CriticalChance.GetCalculatedValue() + "\n\n");
        }

        public float GetAttributeValue(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).GetCalculatedValue();
        }

        public void IncreaseDurability()
        {
            _canEquip = true;
            Durability.SetCurrentValue(Durability.GetCurrentValue() + 1);
            UpdateDurability();
        }

        private void UpdateDurability()
        {
            string quality = "Perfected";
            if (Durability < 4)
            {
                quality = "Flawed";
            }
            else if (Durability < 8)
            {
                quality = "Worn";
            }
            else if (Durability < 12)
            {
                quality = "Fresh";
            }
            else if (Durability < 16)
            {
                quality = "Faultless";
            }
            Name = SecondaryModifier.Name + " " + SubClass.Name + " -- (" + quality + ")";
            WeaponAttributes.RecalculateAttributeValues();
        }

        public void DecreaseDurability()
        {
            Durability.SetCurrentValue(Durability.GetCurrentValue() - 1);
            UpdateDurability();
        }

        public string GetWeaponType()
        {
            return WeaponClass.Type.ToString();
        }

        public float HitProbability(float distanceToTarget)
        {
            float range = distanceToTarget;
            float accuracy = WeaponAttributes.Accuracy.GetCalculatedValue();
            float hitProbability = Mathf.Pow(accuracy / range, 2);
            return hitProbability;
        }

        private float LogAndClamp(float normalisedRange, float extra = 0)
        {
            float probability = (float) (-0.35f * Math.Log(normalisedRange));
            probability += extra;
            if (probability < 0)
            {
                probability = 0;
            }
            else if (probability > 1)
            {
                probability = 1;
            }
            return probability;
        }

        private float CalculateCriticalProbability(float distanceToTarget)
        {
            float maxRange = WeaponAttributes.Accuracy.GetCalculatedValue();
            float normalisedRange = distanceToTarget / maxRange;
            float probability = LogAndClamp(normalisedRange, WeaponAttributes.CriticalChance.GetCalculatedValue() / 100f);
            return probability;
        }

        private float CalculateMissProbability(float distanceToTarget)
        {
            float maxRange = WeaponAttributes.Accuracy.GetCalculatedValue();
            float normalisedRange = maxRange / distanceToTarget;
            if (normalisedRange > 1)
            {
                return 0;
            }
            float probability = LogAndClamp(normalisedRange);
            return probability;
        }
        
        private float GetPelletDamage(float missProbability, float criticalProbability, bool rageModeOn)
        {
            if (Random.Range(0f, 1f) <= missProbability)
            {
                CombatManager.CombatUi.ShowHitMessage("Miss");
                return 0;
            }
            float pelletDamage = WeaponAttributes.Damage.GetCalculatedValue();
            if (Random.Range(0f, 1f) < criticalProbability || rageModeOn)
            {
                CombatManager.CombatUi.ShowHitMessage("Critical!");
                pelletDamage *= 2;
            }
            return pelletDamage;
        }

        //Returns damage
        public int Fire(float distanceToTarget, bool rageModeOn, bool guaranteeHit = false, bool guaranteeCritical = false)
        {
            if (AmmoInMagazine.GetCurrentValue() <= 0)
            {
                throw new Exceptions.FiredWithNoAmmoException();
            }
            AmmoInMagazine.SetCurrentValue(AmmoInMagazine.GetCurrentValue() - 1);
            GunFire.Fire();
            float damageDealt = 0f;
            for (int i = 0; i < Pellets; ++i)
            {
                float missProbability = guaranteeHit ? 0 : CalculateMissProbability(distanceToTarget);
                float criticalProbability = guaranteeCritical ?  1 : CalculateCriticalProbability(distanceToTarget);
                damageDealt += GetPelletDamage(missProbability, criticalProbability, rageModeOn);
            }
            return (int) damageDealt;
        }

        public void Reload(Inventory inventory)
        {
            if (inventory == null) return;
            float ammoAvailable = inventory.DecrementResource(InventoryResourceType.Ammo, Capacity);
            AmmoInMagazine.SetCurrentValue(AmmoInMagazine.GetCurrentValue() + (int) ammoAvailable);
        }

        public bool FullyLoaded()
        {
            //TODO check if character has any ammo left
            return GetRemainingAmmo() == Capacity;
        }

        public bool Empty()
        {
            return GetRemainingAmmo() == 0;
        }

        public int GetRemainingAmmo()
        {
            return (int) AmmoInMagazine.GetCurrentValue();
        }

        public override string GetSummary()
        {
            return Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";
        }

        public override ViewParent CreateUi(Transform parent)
        {
            return new WeaponUi(this, parent);
        }
    }
}