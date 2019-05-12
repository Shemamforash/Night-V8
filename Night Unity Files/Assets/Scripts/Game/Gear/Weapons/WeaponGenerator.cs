using System;
using System.Collections.Generic;
using System.Xml;
using Game.Global;
using Extensions;
using Extensions;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public static class WeaponGenerator
	{
		private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();
		private static          bool                                      _readWeapons;
		private static readonly List<WeaponType>                          _weaponTypes = new List<WeaponType>();

		public static Weapon GenerateWeapon(WeaponType type)
		{
			LoadBaseWeapons();
			return Weapon.Generate(WorldState.GenerateGearLevel(), WeaponClasses[type].RandomElement());
		}

		public static Weapon GenerateWeapon(ItemQuality quality, WeaponType type)
		{
			LoadBaseWeapons();
			return Weapon.Generate(quality, WeaponClasses[type].RandomElement());
		}

		public static Weapon GenerateWeapon(ItemQuality quality)
		{
			LoadBaseWeapons();
			Weapon weapon = Weapon.Generate(quality);
			return weapon;
		}

		public static Weapon GenerateWeapon(bool forceMaxGearLevel = false)
		{
			LoadBaseWeapons();
			if (!forceMaxGearLevel) return GenerateWeapon(WorldState.GenerateGearLevel());
			int qualityLevel = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
			return GenerateWeapon((ItemQuality) qualityLevel);
		}

		public static void LoadBaseWeapons()
		{
			if (_readWeapons) return;
			XmlNode classesNode = Helper.OpenRootNode("WeaponClasses", "Weapons");
			foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
			{
				_weaponTypes.Add(type);
				WeaponClasses[type] = new List<WeaponClass>();
				XmlNode classNode = classesNode.GetChild("Class[@name='" + type + "']");
				foreach (XmlNode subtypeNode in classNode.GetNodesWithName("Subtype"))
					WeaponClasses[type].Add(new WeaponClass(subtypeNode, type));
			}

			_readWeapons = true;
		}

		public static List<WeaponType> GetWeaponTypes()
		{
			LoadBaseWeapons();
			return _weaponTypes;
		}
	}
}