using System;
using System.Collections.Generic;
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
        public CharacterAttribute FireRate, ReloadSpeed, Damage, Range, Handling, Capacity, Pellets, Accuracy;
        public CharacterAttribute PierceChance, BurnChance, BleedChance, SicknessChance;
        private float _dps;
        private AttributeModifier _durabilityModifier;
        public readonly Number Durability;
        private const int MaxDurability = 10;
        private const float MinDurabilityMod = 0.75f;
        public float DurabilityModifier;
        public bool Automatic = true;

        public string WeaponClassName, WeaponClassDescription;
        public string ModifierName, ModifierDescription;
        public WeaponType WeaponType;
        public InventoryResourceType AmmoType;
        private Weapon _weapon;

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            SaveController.CreateNodeAndAppend("FireRate", root, FireRate);
            SaveController.CreateNodeAndAppend("ReloadSpeed", root, ReloadSpeed);
            SaveController.CreateNodeAndAppend("Damage", root, Damage);
            SaveController.CreateNodeAndAppend("Range", root, Range);
            SaveController.CreateNodeAndAppend("Accuracy", root, Accuracy);
            SaveController.CreateNodeAndAppend("Handling", root, Capacity);
            SaveController.CreateNodeAndAppend("Capacity", root, Capacity);
            SaveController.CreateNodeAndAppend("Pellets", root, Pellets);
            SaveController.CreateNodeAndAppend("Durability", root, Durability.CurrentValue());
            SaveController.CreateNodeAndAppend("Automatic", root, Automatic);
            SaveController.CreateNodeAndAppend("WeaponClassDescription", root, WeaponClassDescription);
            SaveController.CreateNodeAndAppend("ModifierName", root, ModifierName);
            SaveController.CreateNodeAndAppend("ModifierDescription", root, ModifierDescription);
            SaveController.CreateNodeAndAppend("WeaponType", root, WeaponType);
            return root;
        }

        public WeaponAttributes(Weapon weapon, int durability = -1)
        {
            _weapon = weapon;
            _durabilityModifier = new AttributeModifier(new List<AttributeType> {AttributeType.Damage, AttributeType.FireRate, AttributeType.Accuracy});
            if (durability == -1) durability = Random.Range(0, MaxDurability / 4);
            Durability = new Number(durability, 0, MaxDurability);
            Durability.AddOnValueChange(a => RecalculateAttributeValues());
        }

        public void SetDurability(int value)
        {
            Durability.SetCurrentValue(value);
        }

        public void SetClass(WeaponClass weaponClass)
        {
            weaponClass.Modifiers.ForEach(m => m.ApplyOnce(this));
            WeaponType = weaponClass.Type;
            Automatic = weaponClass.Automatic;
            WeaponClassName = weaponClass.Name;
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

            RecalculateAttributeValues();
        }

        public void SetInscription(Inscription inscription)
        {
//            inscription.ApplyToGear(this);
//            ModifierName = inscription.Name;
//            ModifierDescription = inscription.GetDescription();
//            RecalculateAttributeValues();
        }

        public string GetName()
        {
            return WeaponClassName;
        }

        private void RecalculateAttributeValues()
        {
            _durabilityModifier.Remove();
            float normalisedDurability = Durability.CurrentValue() / MaxDurability;
            float qualityModifier = (int) _weapon.Quality() + 1 / 2f;
            DurabilityModifier = MinDurabilityMod + (1 - MinDurabilityMod) * normalisedDurability * qualityModifier;
            _durabilityModifier.SetMultiplicative(DurabilityModifier);
            _durabilityModifier.Apply(this);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = Damage.CurrentValue();
            float magazineDamage = (int) Capacity.CurrentValue() * averageShotDamage * (int) Pellets.CurrentValue();
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
            Range = new CharacterAttribute(AttributeType.Range, 0);
            Accuracy = new CharacterAttribute(AttributeType.Accuracy, 0, 0, 100);
            FireRate = new CharacterAttribute(AttributeType.FireRate, 0);
            ReloadSpeed = new CharacterAttribute(AttributeType.ReloadSpeed, 0);
            Handling = new CharacterAttribute(AttributeType.Handling, 0);

            Capacity = new CharacterAttribute(AttributeType.Capacity, 0);
            Pellets = new CharacterAttribute(AttributeType.Pellets, 0);

            BurnChance = new CharacterAttribute(AttributeType.BurnChance, 0);
            BleedChance = new CharacterAttribute(AttributeType.BleedChance, 0);
            PierceChance = new CharacterAttribute(AttributeType.PierceChance, 0);
            SicknessChance = new CharacterAttribute(AttributeType.SicknessChance, 0);

            AddAttribute(Damage);
            AddAttribute(Range);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(FireRate);
            AddAttribute(Handling);

            AddAttribute(Capacity);
            AddAttribute(Pellets);

            AddAttribute(BurnChance);
            AddAttribute(BleedChance);
            AddAttribute(PierceChance);
            AddAttribute(SicknessChance);
        }

        public string Print()
        {
            return WeaponType + " " + WeaponClassName + " " + ModifierName
                   + "\nDurability: " + Durability.CurrentValue() + " (" + DurabilityModifier + ")"
                   + "\nDPS: " + DPS()
                   + "\nAutomatic: " + Automatic
                   + "\nCapacity:   " + Capacity.CurrentValue()
                   + "\nPellets:    " + Pellets.CurrentValue()
                   + "\nDamage:     " + Damage.CurrentValue()
                   + "\nRange:   " + Range.CurrentValue()
                   + "\nFire Rate:  " + FireRate.CurrentValue()
                   + "\nReload:     " + ReloadSpeed.CurrentValue()
                   + "\nAccuracy: " + Accuracy.CurrentValue()
                   + "\n" + WeaponClassDescription?.Replace("\n", " ")
                   + "\n" + ModifierDescription?.Replace("\n", " ") + "\n\n";
        }
    }
}