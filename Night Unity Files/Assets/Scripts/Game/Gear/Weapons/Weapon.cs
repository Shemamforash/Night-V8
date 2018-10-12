using System.Xml;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public const float MaxAccuracyOffsetInDegrees = 25f;
        private const float RangeMin = 1f;
        private const float RangeMax = 5.5f;
        public readonly WeaponAttributes WeaponAttributes;
        public readonly Skill WeaponSkillOne, WeaponSkillTwo;
        private Character _character;
        private Inscription _inscription;
        private bool _inscriptionApplied;

        private Weapon(WeaponClass weaponClass, ItemQuality _itemQuality) : base(weaponClass.Type.ToString(), GearSubtype.Weapon, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this, weaponClass);
            WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(this);
            WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(this);
            string quality = Quality().ToString();
            Name = quality + " " + WeaponAttributes.GetWeaponClass();
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

        public override void Load(XmlNode root)
        {
            base.Load(root);
            WeaponAttributes.Load(root);
        }

        public override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
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
            UnapplyInscription();
            _inscription?.RemoveModifierFromWeapon(this);
            _inscription = inscription;
            Inventory.DestroyItem(inscription);
            _inscription.ApplyModifierToWeapon(this);
            ApplyInscription();
        }

        public float CalculateBaseAccuracy()
        {
            float accuracy = 1f - WeaponAttributes.Val(AttributeType.Accuracy);
            accuracy *= MaxAccuracyOffsetInDegrees;
            return accuracy;
        }

        public override bool IsStackable() => false;

        public WeaponType WeaponType() => WeaponAttributes.WeaponType;

        public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue();

        public string GetWeaponType() => WeaponAttributes.WeaponType.ToString();

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
                case WeaponClassType.Spewer:
                    weaponBehaviour = player.gameObject.AddComponent<AccuracyGainer>();
                    break;
                case WeaponClassType.Breacher:
                    weaponBehaviour = player.gameObject.AddComponent<AttributeGainer>();
                    break;
                case WeaponClassType.Annihilator:
                    weaponBehaviour = player.gameObject.AddComponent<RandomFire>();
                    break;
                case WeaponClassType.Gouger:
                    weaponBehaviour = player.gameObject.AddComponent<Spoolup>();
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
            if (_inscriptionApplied) return;
            _inscription?.ApplyModifierToCharacter(_character);
            _inscriptionApplied = true;
        }

        private void UnapplyInscription()
        {
            if (!_inscriptionApplied) return;
            _inscription?.RemoveModifierFromCharacter(_character);
            _inscriptionApplied = false;
        }

        public Inscription GetInscription() => _inscription;
    }
}