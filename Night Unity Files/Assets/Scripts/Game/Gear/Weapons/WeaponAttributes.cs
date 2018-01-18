﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public CharacterAttribute FireRate, ReloadSpeed, Damage, Accuracy, CriticalChance, Capacity, Pellets;
        public CharacterAttribute PierceChance, BurnChance, BleedChance, SicknessChance;
        private float _dps;
        private AttributeModifier _durabilityModifier = new AttributeModifier();
        public readonly Number Durability;
        private const int MaxDurability = 20;
        public float DurabilityModifier;
        public bool Automatic = true;

        public string WeaponClassDescription;
        public string SubClassName, SubClassDescription;
        public string ModifierName, ModifierDescription;
        public WeaponType WeaponType;
        public InventoryResourceType AmmoType;

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            SaveController.CreateNodeAndAppend("FireRate", root, FireRate);
            SaveController.CreateNodeAndAppend("ReloadSpeed", root, ReloadSpeed);
            SaveController.CreateNodeAndAppend("Damage", root, Damage);
            SaveController.CreateNodeAndAppend("Accuracy", root, Accuracy);
            SaveController.CreateNodeAndAppend("CriticalChance", root, CriticalChance);
            SaveController.CreateNodeAndAppend("Capacity", root, Capacity);
            SaveController.CreateNodeAndAppend("Pellets", root, Pellets);
            SaveController.CreateNodeAndAppend("Durability", root, Durability.CurrentValue());
            SaveController.CreateNodeAndAppend("Automatic", root, Automatic);
            SaveController.CreateNodeAndAppend("WeaponClassDescription", root, WeaponClassDescription);
            SaveController.CreateNodeAndAppend("WeaponSubClass", root, SubClassName);
            SaveController.CreateNodeAndAppend("WeaponSubClassDescription", root, SubClassDescription);
            SaveController.CreateNodeAndAppend("ModifierName", root, ModifierName);
            SaveController.CreateNodeAndAppend("ModifierDescription", root, ModifierDescription);
            SaveController.CreateNodeAndAppend("WeaponType", root, WeaponType);
            return root;
        }

        public string DurabilityToQuality()
        {
            if (Durability.CurrentValue() <= 4) return "Flawed";
            if (Durability.CurrentValue() <= 8) return "Worn";
            if (Durability.CurrentValue() <= 12) return "Fresh";
            return Durability.CurrentValue() <= 16 ? "Faultless" : "Perfected";
        }

        public WeaponAttributes(int durability)
        {
            if (durability == -1) durability = Random.Range(0, MaxDurability);
            Durability = new Number(durability, 0, MaxDurability);
        }

        public void SetClass(WeaponClass weaponClass)
        {
            weaponClass.ApplyToGear(this);
            WeaponType = weaponClass.Type;
            WeaponClassDescription = weaponClass.GetDescription();
            switch (WeaponType)
            {
                case WeaponType.Pistol:
                    AmmoType = InventoryResourceType.PistolMag;
                    break;
                case WeaponType.Rifle:
                    AmmoType = InventoryResourceType.RifleMag;
                    break;
                case WeaponType.Shotgun:
                    AmmoType = InventoryResourceType.ShotgunMag;
                    break;
                case WeaponType.SMG:
                    AmmoType = InventoryResourceType.SmgMag;
                    break;
                case WeaponType.LMG:
                    AmmoType = InventoryResourceType.LmgMag;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

//            Debug.Log(Print());
        }

        public void SetSubClass(GearModifier subClass)
        {
            subClass.ApplyToGear(this);
            SubClassName = subClass.Name;
            SubClassDescription = subClass.GetDescription();
            RecalculateAttributeValues();
//            Debug.Log(Print());
        }

        public void SetModifier(GearModifier secondaryModifier)
        {
            secondaryModifier.ApplyToGear(this);
            ModifierName = secondaryModifier.Name;
            ModifierDescription = secondaryModifier.GetDescription();
            RecalculateAttributeValues();
//            Debug.Log(Print());
        }

        public void AddManualModifier()
        {
            Capacity.ApplyMultMod(0.5f);
            Damage.ApplyMultMod(1.5f);
            Accuracy.ApplyMultMod(1.5f);
            FireRate.ApplyMultMod(0.5f);
            Automatic = false;
        }

        public string GetName()
        {
            return ModifierName + " " + SubClassName;
        }

        public void RecalculateAttributeValues()
        {
            DurabilityModifier = 1f / (MaxDurability * 2) * (Durability.CurrentValue() + MaxDurability);
            _durabilityModifier.SetMultiplicative(DurabilityModifier);
            _durabilityModifier.Apply();
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.CurrentValue() / 100 * Damage.CurrentValue() * 2 + (1 - CriticalChance.CurrentValue() / 100) * Damage.CurrentValue();
            float magazineDamage = (int) Capacity.CurrentValue() * averageShotDamage * (int) Pellets.CurrentValue() * Accuracy.CurrentValue() / 100;
            float magazineDuration = (int) Capacity.CurrentValue() / FireRate.CurrentValue() + ReloadSpeed.CurrentValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS()
        {
            return _dps;
        }

        protected override void CacheAttributes()
        {
            Damage = new CharacterAttribute(AttributeType.Damage, 0);
            Accuracy = new CharacterAttribute(AttributeType.Accuracy, 0, 0, 100);
            CriticalChance = new CharacterAttribute(AttributeType.CriticalChance, 0, 0, 100);
            FireRate = new CharacterAttribute(AttributeType.FireRate, 0);
            ReloadSpeed = new CharacterAttribute(AttributeType.ReloadSpeed, 0);

            Capacity = new CharacterAttribute(AttributeType.Capacity, 0);
            Pellets = new CharacterAttribute(AttributeType.Pellets, 0);

            BurnChance = new CharacterAttribute(AttributeType.BurnChance, 0);
            BleedChance = new CharacterAttribute(AttributeType.BleedChance, 0);
            PierceChance = new CharacterAttribute(AttributeType.PierceChance, 0);
            SicknessChance = new CharacterAttribute(AttributeType.SicknessChance, 0);

            AddAttribute(Damage);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(CriticalChance);
            AddAttribute(FireRate);

            AddAttribute(Capacity);
            AddAttribute(Pellets);

            AddAttribute(BurnChance);
            AddAttribute(BleedChance);
            AddAttribute(PierceChance);
            AddAttribute(SicknessChance);

            _durabilityModifier.AddTargetAttributes(new List<CharacterAttribute> {Damage, Accuracy, CriticalChance, FireRate, ReloadSpeed});
        }

        public string Print()
        {
            return WeaponType + " " + SubClassName + " " + ModifierName
                   + "\nDurability: " + Durability.CurrentValue() + " (" + DurabilityModifier + ")"
                   + "\nDPS: " + DPS()
                   + "\nAutomatic: " + Automatic
                   + "\nCapacity:   " + Capacity.CurrentValue()
                   + "\nPellets:    " + Pellets.CurrentValue()
                   + "\nDamage:     " + Damage.CurrentValue()
                   + "\nAccuracy:   " + Accuracy.CurrentValue()
                   + "\nFire Rate:  " + FireRate.CurrentValue()
                   + "\nReload:     " + ReloadSpeed.CurrentValue()
                   + "\nCritChance: " + CriticalChance.CurrentValue()
                   + "\n" + WeaponClassDescription?.Replace("\n", " ")
                   + "\n" + SubClassDescription?.Replace("\n", " ")
                   + "\n" + ModifierDescription?.Replace("\n", " ") + "\n\n";
        }
    }
}