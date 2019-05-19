using System.Collections.Generic;
using Extensions;
using Game.Global;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public static class WeaponGenerator
	{
		private static readonly List<WeaponType> _weaponTypes = CollectionExtensions.ValuesToList<WeaponType>();

		public static Weapon Generate(WeaponType  type)                     => Generate(WorldState.GenerateGearLevel(), type);
		public static Weapon Generate(ItemQuality quality, WeaponType type) => Weapon.Generate(quality, type);
		public static Weapon Generate(ItemQuality quality) => Weapon.Generate(quality, _weaponTypes.RandomElement());

		public static Weapon Generate(bool forceMaxGearLevel = false)
		{
			if (!forceMaxGearLevel) return Generate(WorldState.GenerateGearLevel());
			int qualityLevel = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
			return Generate((ItemQuality) qualityLevel);
		}
	}
}