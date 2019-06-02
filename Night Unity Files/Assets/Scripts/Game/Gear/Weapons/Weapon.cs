using System.Collections.Generic;
using System.Xml;
using Extensions;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public class Weapon : GearItem
	{
		private const    float             RangeMin = 1.5f;
		private const    float             RangeMax = 5.5f;
		public readonly  WeaponAttributes  WeaponAttributes;
		public readonly  Skill             WeaponSkillOne, WeaponSkillTwo;
		private readonly List<Inscription> _inscriptions = new List<Inscription>();

		private Weapon(WeaponClass weaponClass, ItemQuality _itemQuality) : base(_itemQuality + " " + weaponClass.Name, _itemQuality)
		{
			WeaponAttributes = new WeaponAttributes(this, weaponClass);
			WeaponSkillOne   = WeaponSkills.GetWeaponSkillOne(this);
			WeaponSkillTwo   = WeaponSkills.GetWeaponSkillTwo(this);
		}

		public static Weapon LoadWeapon(XmlNode root)
		{
			int         weaponClassInt = root.ParseInt("Class");
			WeaponClass weaponClass    = WeaponClass.IntToWeaponClass(weaponClassInt);
			ItemQuality weaponQuality  = (ItemQuality) root.ParseInt("Quality");
			Weapon      weapon         = new Weapon(weaponClass, weaponQuality);
			weapon.Load(root);
			return weapon;
		}

		protected override void Load(XmlNode root)
		{
			base.Load(root);
			WeaponAttributes.Load(root);
			XmlNode inscriptionsNode = root.SelectSingleNode("Inscriptions");
			foreach (XmlNode inscriptionNode in inscriptionsNode.SelectNodes("Inscription"))
			{
				Inscription inscription = Inscription.LoadInscription(inscriptionNode);
				AddInscription(inscription);
			}
		}

		public override XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Weapon");
			base.Save(root);
			WeaponAttributes.Save(root);
			XmlNode inscriptionsNode = root.CreateChild("Inscriptions");
			_inscriptions.ForEach(i => i.Save(inscriptionsNode));
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

		public void AddInscription(Inscription inscription)
		{
			Inventory.Destroy(inscription);
			ApplyInscriptionModifier(inscription);
			ApplyInscription(inscription);
			WeaponAttributes.RecalculateAttributeValues();
			_inscriptions.Add(inscription);
		}

		private void ApplyInscriptionModifier(Inscription inscription)
		{
			ApplyModifier(inscription.Target(), inscription.Modifier());
		}

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			if (target.IsCoreAttribute()) return;
			Debug.Log(target + " " + modifier.FinalBonus() + " " + modifier.RawBonus());
			WeaponAttributes.Get(target).AddModifier(modifier);
			WeaponAttributes.RecalculateAttributeValues();
		}

		public void RemoveModifier(AttributeType target, AttributeModifier modifier)
		{
			if (target.IsCoreAttribute()) return;
			WeaponAttributes.Get(target).RemoveModifier(modifier);
			WeaponAttributes.RecalculateAttributeValues();
		}


		public WeaponType WeaponType() => WeaponAttributes.WeaponType;

		public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue;

		public override string GetSummary() => WeaponAttributes.DPS().Round(1) + "DPS";

		public BaseWeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
		{
			BaseWeaponBehaviour weaponBehaviour;
			switch (WeaponAttributes.GetWeaponClass())
			{
				case WeaponClassType.Shortshooter:
					weaponBehaviour = player.gameObject.AddComponent<DoubleFireDelay>();
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
			if (character.Weapon == this) return;
			base.Equip(character);
			_inscriptions.ForEach(ApplyInscription);
			EquippedCharacter.Accessory?.ApplyToWeapon(this);
		}

		public override void UnEquip()
		{
			if (EquippedCharacter == null) return;
			_inscriptions.ForEach(RemoveInscription);
			EquippedCharacter.Accessory?.RemoveFromWeapon(this);
			base.UnEquip();
		}

		private void ApplyInscription(Inscription inscription)
		{
			(EquippedCharacter as Player)?.ApplyModifier(inscription.Target(), inscription.Modifier());
		}

		private void RemoveInscription(Inscription inscription)
		{
			(EquippedCharacter as Player)?.RemoveModifier(inscription.Target(), inscription.Modifier());
		}

		protected override void CalculateDismantleRewards()
		{
			base.CalculateDismantleRewards();
			int quality = (int) Quality() + 1;
			AddReward("Essence", quality);
			List<string> possibleRewards = new List<string>();
			for (int i = 0; i < quality; ++i)
			{
				if (i == 0) possibleRewards.Add("Essence");
				if (i == 1) possibleRewards.Add("Rusty Scrap");
				if (i == 2) possibleRewards.Add("Metal Shards");
				if (i == 3) possibleRewards.Add("Ancient Relics");
				if (i == 4) possibleRewards.Add("Celestial Fragments");
			}

			int count = Mathf.FloorToInt(quality / 2f) + 1;
			for (int i = 0; i < count; ++i) AddReward(possibleRewards.RemoveRandom(), 2);
		}
	}
}