using System.Collections.Generic;
using System.Xml;
using Extensions;
using Game.Characters;
using Game.Combat.Misc;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Weapons
{
	public class Weapon : GearItem
	{
		public readonly  WeaponAttributes  WeaponAttributes;
		private readonly List<Inscription> _inscriptions = new List<Inscription>();

		private Weapon(WeaponClass weaponClass, ItemQuality _itemQuality) : base(_itemQuality + " " + weaponClass.Name, _itemQuality)
		{
			WeaponAttributes = new WeaponAttributes(this, weaponClass);
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
			if (inscriptionsNode == null)
			{
				for (int i = 0; i < 3; ++i)
				{
					Inscription inscription = Inscription.Generate(false);
					Inventory.Move(inscription);
					Inventory.IncrementResource("Essence", inscription.InscriptionCost());
				}

				return;
			}

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

		public void AddInscription(Inscription inscription)
		{
			Inventory.Destroy(inscription);
			ApplyInscriptionModifier(inscription);
			ApplyInscription(inscription);
			WeaponAttributes.CalculateDPS();
			_inscriptions.Add(inscription);
		}

		private void ApplyInscriptionModifier(Inscription inscription)
		{
			ApplyModifier(inscription.Target(), inscription.Modifier());
		}

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			if (target.IsCoreAttribute()) return;
			WeaponAttributes.Get(target).AddModifier(modifier);
			WeaponAttributes.CalculateDPS();
		}

		public WeaponType WeaponType() => WeaponAttributes.WeaponType;

		public override string GetSummary() => WeaponAttributes.DPS().Round(1) + "DPS";

		public WeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
		{
			WeaponBehaviour weaponBehaviour = player.gameObject.AddComponent<WeaponBehaviour>();
			switch (WeaponAttributes.GetWeaponClass())
			{
				case WeaponClassType.Shortshooter:
					weaponBehaviour.SetRepeat(2, 0.1f);
					break;
				case WeaponClassType.Skullcrusher:
					weaponBehaviour.SetRepeat(2, 0.1f);
					break;
				case WeaponClassType.Spitter:
					weaponBehaviour.SetRepeat(4, 0.05f);
					break;
				case WeaponClassType.Gouger:
					weaponBehaviour.InvertedAccuracy = true;
					break;
			}

			weaponBehaviour.Initialise(player);
			return weaponBehaviour;
		}

		public override void Equip(Character character)
		{
			base.Equip(character);
			_inscriptions.ForEach(ApplyInscription);
			WeaponAttributes.CalculateDPS();
		}

		public override void UnEquip()
		{
			_inscriptions.ForEach(RemoveInscription);
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
			int         essenceReward = 0;
			Inscription inscription   = Inscription.Generate(Quality(), false);
			essenceReward += (int) inscription.Quality();
			AddReward(inscription.Name, () => Inventory.Move(inscription));

			if (_inscriptions.Count > 4)
			{
				Inscription extraInscription = Inscription.Generate(Quality(), false);
				AddReward(inscription.Name, () => Inventory.Move(extraInscription));
				essenceReward += (int) extraInscription.Quality();
			}

			if (essenceReward <= 0) return;
			AddReward(essenceReward + " Essence", () => Inventory.IncrementResource("Essence", essenceReward));
		}
	}
}