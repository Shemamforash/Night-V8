using System.Xml;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponAttributes WeaponAttributes;
        public Skill WeaponSkillOne, WeaponSkillTwo;
        private const float MaxAccuracyOffsetInDegrees = 25f;
        private const float RangeMin = 1f;
        private const float RangeMax = 4.5f;
        private Inscription _inscription;
        private Character _character;

        public Weapon(string name, float weight, ItemQuality _itemQuality) : base(name, GearSubtype.Weapon, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this);
        }

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            WeaponAttributes.Save(root, saveType);
            return root;
        }

        public float CalculateIdealDistance()
        {
            float range = WeaponAttributes.Val(AttributeType.Accuracy) / 100f;
            float idealDistance = (RangeMax - RangeMin) * range + RangeMin;
            return idealDistance;
        }

        public void SetInscription(Inscription inscription)
        {
            _inscription?.RemoveModifier(this);
            _inscription = inscription;
            _inscription.ParentInventory.DestroyItem(inscription);
            inscription.ApplyModifier(this);
        }

        public float CalculateBaseAccuracy()
        {
            float accuracy = 1f - WeaponAttributes.Val(AttributeType.Accuracy) / 100f;
            accuracy *= MaxAccuracyOffsetInDegrees;
            return accuracy;
        }

        public override bool IsStackable() => false;

        public WeaponType WeaponType() => WeaponAttributes.WeaponType;

        public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue();

        public void SetName()
        {
            string quality = Quality().ToString();
            Name = quality + " " + WeaponAttributes.GetWeaponClass();
        }

        public string GetWeaponType() => WeaponAttributes.WeaponType.ToString();

        public override string GetSummary() => Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";

        public int GetUpgradeCost() => (int) (WeaponAttributes.Durability.CurrentValue() * 10 + 100);

        public bool Inscribable() => Quality() == ItemQuality.Shining;

        public BaseWeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
        {
            BaseWeaponBehaviour weaponBehaviour;
            switch (WeaponAttributes.GetWeaponClass())
            {
                case WeaponClassType.Shortshooter:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireInstant>();
                    break;
//                case WeaponClassType.Voidwalker:
//                    break;
                case WeaponClassType.Skullcrusher:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireDelay>();
                    break;
                case WeaponClassType.Spitter:
                    weaponBehaviour = player.gameObject.AddComponent<Burstfire>();
                    break;
                case WeaponClassType.Spewer:
                    weaponBehaviour = player.gameObject.AddComponent<AccuracyGainer>();
                    break;
                case WeaponClassType.Breacher:
                    weaponBehaviour = player.gameObject.AddComponent<AttributeGainer>();
                    break;
                case WeaponClassType.Annihilator:
                    weaponBehaviour = player.gameObject.AddComponent<RandomFire>();
                    break;
                default:
                    weaponBehaviour = player.gameObject.AddComponent<DefaultBehaviour>();
                    break;
            }

            weaponBehaviour.Initialise(player.Weapon());
            return weaponBehaviour;
        }

        private void AddInscription(Inscription i)
        {
            if (_inscription != null)
            {
                _inscription.RemoveModifier(this);
                UnapplyInscription();
            }

            _inscription = i;
            ParentInventory.DestroyItem(i);
            ApplyInscription();
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
            _character = character;
            ApplyInscription();
        }

        public override void Unequip()
        {
            base.Unequip();
            UnapplyInscription();
        }

        private void ApplyInscription()
        {
            if (_inscription == null) return;
            _inscription.ApplyModifier(this);
            _inscription.ApplyModifier(_character);
        }

        private void UnapplyInscription()
        {
            if (_inscription == null) return;
            _inscription.RemoveModifier(this);
            _inscription.RemoveModifier(_character);
        }

        public Inscription GetInscription()
        {
            return _inscription;
        }
    }
}