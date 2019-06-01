using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Extensions;

namespace Game.Gear.Weapons
{
	public class WeaponClass
	{
		public readonly         bool                  Automatic;
		public readonly         WeaponClassType       Name;
		public readonly         WeaponType            Type;
		public readonly         string                Description, FireType, FireMode;
		public readonly         int                   Pellets,     Capacity, Recoil,   Damage;
		public readonly         float                 ReloadSpeed, FireRate, Accuracy, Range;
		private static readonly List<WeaponClassType> _weaponClassTypes = new List<WeaponClassType>();
		private static readonly List<WeaponClass>     _weaponClasses    = new List<WeaponClass>();

		public WeaponClass(XmlNode subtypeNode, WeaponType type)
		{
			Type        = type;
			Automatic   = subtypeNode.ParseAttribute("automatic") == "True";
			Name        = NameToClassType(subtypeNode.Attributes["name"].Value);
			Damage      = subtypeNode.ParseInt("Damage");
			FireRate    = subtypeNode.ParseFloat("FireRate");
			ReloadSpeed = subtypeNode.ParseFloat("ReloadSpeed");
			Accuracy    = subtypeNode.ParseFloat("Accuracy") / 100f;
			Recoil      = subtypeNode.ParseInt("Recoil");
			Capacity    = subtypeNode.ParseInt("Capacity");
			Range       = subtypeNode.ParseFloat("Range");
			FireType    = subtypeNode.ParseString("FireType");
			FireMode    = subtypeNode.ParseString("FireMode");
			Description = FireType + " - " + Type;
			Pellets     = type == WeaponType.Shotgun ? 10 : 1;
			_weaponClasses.Add(this);
		}


		private static WeaponClassType NameToClassType(string name)
		{
			if (_weaponClassTypes.Count == 0)
			{
				foreach (WeaponClassType classType in Enum.GetValues(typeof(WeaponClassType))) _weaponClassTypes.Add(classType);
			}

			foreach (WeaponClassType classType in _weaponClassTypes)
			{
				if (classType.ToString() == name)
				{
					return classType;
				}
			}

			throw new ArgumentOutOfRangeException("Unknown class type: '" + name + "'");
		}

		public static WeaponClass IntToWeaponClass(int weaponClassString)
		{
			WeaponGenerator.LoadBaseWeapons();
			return _weaponClasses.First(w => (int) w.Name == weaponClassString);
		}

		public static WeaponClass GetRandomClass()
		{
			WeaponGenerator.LoadBaseWeapons();
			return _weaponClasses.RandomElement();
		}
	}
}