using System;
using System.Collections.Generic;
using Extensions;
using Game.Characters;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Humans;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;


using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.Assertions;
using Nightmare = Game.Combat.Enemies.Nightmares.Nightmare;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
	public class Enemy : Character
	{
		private const           float                         DropChanceIncrement = 0.01f;
		private static          GameObject                    _enemyPrefab;
		private static          GameObject                    _footStepPrefab, _trailPrefab, _hoofprintPrefab;
		private static readonly Dictionary<EnemyType, Sprite> _enemySprites        = new Dictionary<EnemyType, Sprite>();
		private static          float                         _accessoryDropChance = -0.25f, _inscriptionDropChance = -0.25f;
		public readonly         EnemyTemplate                 Template;

		public Enemy(EnemyTemplate template) : base(template.EnemyType.ToString())
		{
			Template = template;
			GenerateArmour();
			GenerateWeapon();
		}

		private void GenerateWeapon()
		{
			if (!Template.HasWeapon) return;
			List<WeaponType> possibleTypes = new List<WeaponType>();
			switch (Template.EnemyType)
			{
				case EnemyType.Medic:
					possibleTypes.Add(WeaponType.Pistol);
					possibleTypes.Add(WeaponType.SMG);
					break;
				case EnemyType.Mountain:
					possibleTypes.Add(WeaponType.Shotgun);
					break;
				case EnemyType.Sentinel:
					possibleTypes.Add(WeaponType.SMG);
					possibleTypes.Add(WeaponType.Pistol);
					possibleTypes.Add(WeaponType.Shotgun);
					break;
				case EnemyType.Sniper:
					possibleTypes.Add(WeaponType.Rifle);
					possibleTypes.Add(WeaponType.Pistol);
					break;
				case EnemyType.Warlord:
					possibleTypes.Add(WeaponType.SMG);
					possibleTypes.Add(WeaponType.Shotgun);
					break;
				case EnemyType.Witch:
					possibleTypes.Add(WeaponType.Rifle);
					possibleTypes.Add(WeaponType.Shotgun);
					break;
			}

			Weapon weapon = WeaponGenerator.GenerateWeapon(possibleTypes.RandomElement());
			//todo generate enemy weapon
		}

		private void GenerateArmour()
		{
			Armour.AutoGenerateArmour();
		}

		private void AssignSprite(GameObject enemyObject)
		{
			if (!_enemySprites.ContainsKey(Template.EnemyType))
			{
				Sprite sprite              = Resources.Load<Sprite>("Images/Enemy Symbols/" + Template.EnemyType);
				if (sprite == null) sprite = Resources.Load<Sprite>("Images/Enemy Symbols/Sentinel");
				Assert.IsNotNull(sprite);
				_enemySprites.Add(Template.EnemyType, sprite);
			}

			enemyObject.GetComponent<SpriteRenderer>().sprite = _enemySprites[Template.EnemyType];
		}

		private void AssignTrail(GameObject enemyObject)
		{
			GameObject trailPrefab = null;
			switch (Template.Species)
			{
				case "Human":
					if (_footStepPrefab == null) _footStepPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Footstep Trail");
					trailPrefab = _footStepPrefab;
					break;
				case "Animal":
					if (_hoofprintPrefab == null) _hoofprintPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Hoofprint Trail");
					trailPrefab = _hoofprintPrefab;
					break;
				case "Nightmare":
					if (_trailPrefab == null) _trailPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Nightmare Trail");
					trailPrefab = _trailPrefab;
					break;
			}

			GameObject trailObject = Object.Instantiate(trailPrefab);
			trailObject.transform.SetParent(enemyObject.transform);
			trailObject.transform.localPosition = Vector2.zero;
		}

		public EnemyBehaviour GetEnemyBehaviour()
		{
			if (_enemyPrefab == null) _enemyPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Default Enemy");
			GameObject enemyObject                 = Object.Instantiate(_enemyPrefab);
			enemyObject.name = Template.EnemyType.ToString();
			AssignSprite(enemyObject);
			AssignTrail(enemyObject);
			EnemyBehaviour enemyBehaviour;
			switch (Template.EnemyType)
			{
				case EnemyType.Brawler:
					enemyBehaviour = enemyObject.AddComponent<Brawler>();
					break;
				case EnemyType.Martyr:
					enemyBehaviour = enemyObject.AddComponent<Martyr>();
					break;
				case EnemyType.Medic:
					enemyBehaviour = enemyObject.AddComponent<Medic>();
					break;
				case EnemyType.Mountain:
					enemyBehaviour = enemyObject.AddComponent<Mountain>();
					break;
				case EnemyType.Sentinel:
					enemyBehaviour = enemyObject.AddComponent<Sentinel>();
					break;
				case EnemyType.Sniper:
					enemyBehaviour = enemyObject.AddComponent<Sniper>();
					break;
				case EnemyType.Warlord:
					enemyBehaviour = enemyObject.AddComponent<Warlord>();
					break;
				case EnemyType.Witch:
					enemyBehaviour = enemyObject.AddComponent<Witch>();
					break;
				case EnemyType.Ghoul:
					enemyBehaviour = enemyObject.AddComponent<Ghoul>();
					break;
				case EnemyType.GhoulMother:
					enemyBehaviour = enemyObject.AddComponent<GhoulMother>();
					break;
				case EnemyType.Maelstrom:
					enemyBehaviour = enemyObject.AddComponent<Maelstrom>();
					break;
				case EnemyType.Decoy:
					enemyBehaviour = enemyObject.AddComponent<Decoy>();
					break;
				case EnemyType.Ghast:
					enemyBehaviour = enemyObject.AddComponent<Ghast>();
					break;
				case EnemyType.Nightmare:
					enemyBehaviour = enemyObject.AddComponent<Nightmare>();
					break;
				case EnemyType.Revenant:
					enemyBehaviour = enemyObject.AddComponent<Revenant>();
					break;
				case EnemyType.Shadow:
					enemyBehaviour = enemyObject.AddComponent<Shadow>();
					break;
				case EnemyType.Grazer:
					enemyBehaviour = enemyObject.AddComponent<Grazer>();
					break;
				case EnemyType.Watcher:
					enemyBehaviour = enemyObject.AddComponent<Watcher>();
					break;
				case EnemyType.Curio:
					enemyBehaviour = enemyObject.AddComponent<Curio>();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			enemyBehaviour.Initialise(this);
			return enemyBehaviour;
		}

		private Loot DropHumanLoot(Vector2 position)
		{
			if (Random.Range(0f, 1f) < Template.DropRate) SaltBehaviour.Create(position);
			if (!CanDropAccessory()) return null;
			Loot loot = new Loot(position);
			loot.SetItem(Accessory.Generate());
			return loot;
		}

		private bool CanDropAccessory()
		{
			_accessoryDropChance += DropChanceIncrement;
			bool drop                      = Random.Range(0f, 1f) < _accessoryDropChance;
			if (drop) _accessoryDropChance = -0.4f;
			return drop;
		}

		private bool CanDropInscription()
		{
			_inscriptionDropChance += DropChanceIncrement;
			bool drop                        = Random.Range(0f, 1f) < _inscriptionDropChance;
			if (drop) _inscriptionDropChance = -0.25f;
			return drop;
		}

		private Loot DropNightmareLoot(Vector2 position)
		{
			if (!CanDropInscription())
			{
				if (Random.Range(0f, 1f) < Template.DropRate) EssenceCloudBehaviour.Create(position);
				return null;
			}

			Loot loot = new Loot(position);
			loot.SetItem(Inscription.Generate());
			return loot;
		}

		private Loot DropAnimalLoot(Vector2 position)
		{
			ResourceItem item;
			if (NumericExtensions.RollDie(0, 4) || Inventory.GetResourceQuantity("Grisly Remains") == 0)
			{
				item = ResourceTemplate.Create("Grisly Remains");
			}
			else
			{
				item = ResourceTemplate.GetMeat();
				int meatAmount = (int) Template.DropRate - 1;
				if (meatAmount > 1) --meatAmount;
				item.Increment(meatAmount);
			}

			Loot loot = new Loot(position);
			loot.SetResource(item);
			return loot;
		}

		public Loot DropLoot(Vector2 position)
		{
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return null;
			switch (Template.DropResource)
			{
				case "Salt":
					return DropHumanLoot(position);
				case "Essence":
					return DropNightmareLoot(position);
				case "Meat":
					return DropAnimalLoot(position);
			}

			return null;
		}

		public int GetHealth()
		{
			float healthModifier = WorldState.Difficulty() / 17f;
			float health         = Template.Health;
			health = health + health * healthModifier;
			health = health * WorldState.GetEnemyHealthModifier();
			return (int) health;
		}
	}
}