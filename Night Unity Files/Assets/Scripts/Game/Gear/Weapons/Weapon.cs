using System.Xml;
using Extensions;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Weapons
{
	public class Weapon
	{
		private const   float            RangeMin = 1.5f;
		private const   float            RangeMax = 5.5f;
		public readonly Character        EquippedCharacter;
		public readonly WeaponAttributes WeaponAttributes;
		public          Skill            SkillOne, SkillTwo, SkillThree, SkillFour;

		public Weapon(Character equippedCharacter)
		{
			EquippedCharacter = equippedCharacter;
			WeaponAttributes  = new WeaponAttributes(this);
		}

		private Weapon(ItemQuality quality, WeaponType weaponType)
		{
		}

		public void Load(XmlNode root)
		{
			WeaponAttributes.Load(root);
		}

		public void Save(XmlNode root)
		{
			root = root.CreateChild("Weapon");
			WeaponAttributes.Save(root);
		}

		public static Weapon Generate(ItemQuality quality, WeaponType weaponType) => new Weapon(quality, weaponType);

		public WeaponType WeaponType() => Weapons.WeaponType.Pistol;

		public float CalculateMinimumDistance()
		{
			float range = WeaponAttributes.Val(AttributeType.Accuracy);
			range *= range;
			float minimumDistance = (RangeMax - RangeMin) * range + RangeMin * 0.5f;
			return minimumDistance;
		}

		public void ApplyInscription(Inscription inscription)
		{
			Inventory.Destroy(inscription);
			ApplyInscriptionModifier(inscription);
			WeaponAttributes.RecalculateAttributeValues();
		}

		public string GetDisplayName()
		{
			return "Gun";
		}

		private void ApplyInscriptionModifier(Inscription inscription)
		{
			Assert.IsNotNull(inscription);
			ApplyModifier(inscription.Target(), inscription.Modifier());
		}

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			WeaponAttributes.Get(target).AddModifier(modifier);
			WeaponAttributes.RecalculateAttributeValues();
		}

		public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue;

		public string GetSummary() => WeaponAttributes.DPS().Round(1) + "DPS";

		public WeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
		{
			WeaponBehaviour weaponBehaviour = player.gameObject.GetComponent<WeaponBehaviour>();
			weaponBehaviour.Initialise(player);
			return weaponBehaviour;
		}
	}
}