using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        private const float RangeMin = 1.5f;
        private const float RangeMax = 5.5f;
        public readonly WeaponAttributes WeaponAttributes;
        public readonly Skill WeaponSkillOne, WeaponSkillTwo;
        private Inscription _inscription;
        private bool _inscriptionApplied;

        private Weapon(WeaponClass weaponClass, ItemQuality _itemQuality) : base(_itemQuality + " " + weaponClass.Name, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this, weaponClass);
            WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(this);
            WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(this);
        }

        public static Weapon LoadWeapon(XmlNode root)
        {
            int weaponClassInt = root.IntFromNode("Class");
            WeaponClass weaponClass = WeaponClass.IntToWeaponClass(weaponClassInt);
            ItemQuality weaponQuality = (ItemQuality) root.IntFromNode("Quality");
            Weapon weapon = new Weapon(weaponClass, weaponQuality);
            weapon.Load(root);
            return weapon;
        }

        protected override void Load(XmlNode root)
        {
            base.Load(root);
            WeaponAttributes.Load(root);
            XmlNode inscriptionNode = root.SelectSingleNode("Inscription");
            if (inscriptionNode == null) return;
            Inscription inscription = Inscription.LoadInscription(inscriptionNode);
            SetInscription(inscription);
        }

        public override XmlNode Save(XmlNode root)
        {
            root = root.CreateChild("Weapon");
            base.Save(root);
            WeaponAttributes.Save(root);
            _inscription?.Save(root);
            return root;
        }

        public static Weapon Generate(ItemQuality quality) => new Weapon(WeaponClass.GetRandomClass(), quality);

        public static Weapon Generate(ItemQuality quality, WeaponClass weaponClass) => new Weapon(weaponClass, quality);

        public float CalculateMinimumDistance()
        {
            float range = WeaponAttributes.Val(AttributeType.Accuracy);
            range *= range;
            float minimumDistance = (RangeMax - RangeMin) * range + RangeMin * 0.5f;
            return minimumDistance;
        }

        public void SetInscription(Inscription inscription)
        {
            RemoveInscription();
            RemoveInscriptionModifier();
            _inscription = inscription;
            Inventory.Destroy(inscription);
            ApplyInscriptionModifier();
            ApplyInscription();
            WeaponAttributes.RecalculateAttributeValues();
        }

        public string GetDisplayName()
        {
            string displayName = Name;
            if (_inscription != null) displayName += " of " + _inscription.TemplateName();
            return displayName;
        }

        private void ApplyInscriptionModifier()
        {
            if (_inscription == null) return;
            ApplyModifier(_inscription.Target(), _inscription.Modifier());
        }

        private void RemoveInscriptionModifier()
        {
            if (_inscription == null) return;
            RemoveModifier(_inscription.Target(), _inscription.Modifier());
        }

        public void ApplyModifier(AttributeType target, AttributeModifier modifier)
        {
            if (CharacterAttribute.IsCharacterAttribute(target)) return;
            WeaponAttributes.Get(target).AddModifier(modifier);
            WeaponAttributes.RecalculateAttributeValues();
        }

        public void RemoveModifier(AttributeType target, AttributeModifier modifier)
        {
            if (CharacterAttribute.IsCharacterAttribute(target)) return;
            WeaponAttributes.Get(target).RemoveModifier(modifier);
            Debug.Log(target + " " + modifier.FinalBonusToString());
            WeaponAttributes.RecalculateAttributeValues();
        }


        public WeaponType WeaponType() => WeaponAttributes.WeaponType;

        public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue();

        public override string GetSummary() => WeaponAttributes.DPS().Round(1) + "DPS";

        public BaseWeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
        {
            BaseWeaponBehaviour weaponBehaviour;
            switch (WeaponAttributes.GetWeaponClass())
            {
                case WeaponClassType.Shortshooter:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireDelay>();
                    break;
                case WeaponClassType.Voidwalker:
                    weaponBehaviour = player.gameObject.AddComponent<HoldAndFire>();
                    break;
                case WeaponClassType.Skullcrusher:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireDelay>();
                    break;
                case WeaponClassType.Spitter:
                    weaponBehaviour = player.gameObject.AddComponent<Burstfire>();
                    break;
                case WeaponClassType.Gouger:
                    weaponBehaviour = player.gameObject.AddComponent<AccuracyGainer>();
                    break;
                default:
                    weaponBehaviour = player.gameObject.AddComponent<DefaultBehaviour>();
                    break;
            }

            weaponBehaviour.Initialise(player);
            return weaponBehaviour;
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
            ApplyInscription();
            EquippedCharacter.EquippedAccessory?.ApplyToWeapon(this);
        }

        public override void UnEquip()
        {
            if (EquippedCharacter == null) return;
            RemoveInscription();
            EquippedCharacter.EquippedAccessory?.RemoveFromWeapon(this);
            base.UnEquip();
        }

        private void ApplyInscription()
        {
            if (_inscriptionApplied || _inscription == null) return;
            (EquippedCharacter as Player)?.ApplyModifier(_inscription.Target(), _inscription.Modifier());
            _inscriptionApplied = true;
        }

        private void RemoveInscription()
        {
            if (!_inscriptionApplied || _inscription == null) return;
            (EquippedCharacter as Player)?.RemoveModifier(_inscription.Target(), _inscription.Modifier());
            _inscriptionApplied = false;
        }

        public Inscription GetInscription() => _inscription;
    }
}