using System.Xml;
using Extensions;
using Facilitating.Persistence;
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
		public readonly Skill            WeaponSkillOne, WeaponSkillTwo;

		public Weapon(Character equippedCharacter)
		{
			EquippedCharacter = equippedCharacter;
			WeaponAttributes  = new WeaponAttributes(this);
			WeaponSkillOne    = WeaponSkills.GetWeaponSkillOne(this);
			WeaponSkillTwo    = WeaponSkills.GetWeaponSkillTwo(this);
		}

		private Weapon(WeaponClass getRandomClass, ItemQuality quality)
		{
			throw new System.NotImplementedException();
		}

		public void Load(XmlNode root)
		{
			WeaponAttributes.Load(root);
		}

		public XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Weapon");
			WeaponAttributes.Save(root);
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

		public void ApplyInscription(Inscription inscription)
		{
			Inventory.Destroy(inscription);
			ApplyInscriptionModifier(inscription);
			WeaponAttributes.RecalculateAttributeValues();
		}

		public string GetDisplayName()
		{
//            string displayName = Name;
//            if (_inscription != null) displayName += " of " + _inscription.TemplateName();
//            return displayName;
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

		public BaseWeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
		{
			BaseWeaponBehaviour weaponBehaviour = player.gameObject.AddComponent<DefaultBehaviour>();
			weaponBehaviour.Initialise(player);
			return weaponBehaviour;
		}
	}
}