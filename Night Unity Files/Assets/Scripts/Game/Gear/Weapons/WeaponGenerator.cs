using System.Collections.Generic;
using Extensions;
using Game.Global;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public static class WeaponGenerator
	{
		private static readonly List<WeaponType> _weaponTypes = CollectionExtensions.ValuesToList<WeaponType>();

		public static Weapon GenerateWeapon(WeaponType  type)                     => GenerateWeapon(WorldState.GenerateGearLevel(), type);
		public static Weapon GenerateWeapon(ItemQuality quality, WeaponType type) => Weapon.Generate(quality, type);
		public static Weapon GenerateWeapon(ItemQuality quality) => Weapon.Generate(quality, _weaponTypes.RandomElement());

		public static Weapon GenerateWeapon(bool forceMaxGearLevel = false)
		{
			if (!forceMaxGearLevel) return GenerateWeapon(WorldState.GenerateGearLevel());
			int qualityLevel = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
			return GenerateWeapon((ItemQuality) qualityLevel);
		}
	}
}