using System;
using System.Collections.Generic;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
	public class CharacterAttributes : DesolationAttributes
	{
		private const           int                 PlayerHealthChunkSize = 100;
		private static readonly List<AttributeType> _attributeTypes       = new List<AttributeType>();

		private readonly string[] _hungerEvents =
		{
			"I have to get something to eat",
			"My stomach has been empty for too long",
			"I can't go on if I don't get any food"
		};

		private readonly Player _player;

		private readonly string[] _thirstEvents =
		{
			"I'm going to die if I don't get any water",
			"I'm so thirsty, it's been so long since I had a drink",
			"I need to get some water soon"
		};

		public readonly HashSet<WeaponType> WeaponSkillOneUnlocks = new HashSet<WeaponType>();
		public readonly HashSet<WeaponType> WeaponSkillTwoUnlocks = new HashSet<WeaponType>();
		public          float               ClaimRegionWillGainModifier;
		public          float               DecayExplodeChance;
		public          float               FireExplodeChance;
		public          float               FreeSkillChance;

		public float RallyHealthModifier;
		public bool  ReloadOnEmptyMag;
		public bool  ReloadOnFatalShot;

		public bool SkillOneUnlocked, SkillTwoUnlocked;
		public bool SpreadVoid;

		public CharacterAttributes(Player player) => _player = player;

		public static AttributeType StringToAttributeType(string attributeType)
		{
			LoadAttributeTypes();
			foreach (AttributeType a in _attributeTypes)
			{
				if (a.ToString() != attributeType) continue;
				return a;
			}

			throw new ArgumentOutOfRangeException();
		}

		private static void LoadAttributeTypes()
		{
			if (_attributeTypes.Count != 0) return;
			foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType))) _attributeTypes.Add(attributeType);
		}

		public void IncreaseAttribute(AttributeType attributeType)
		{
			if (Max(attributeType) == 20) return;
			Get(attributeType).Max += 1;
		}

		public float CalculateSpeed() => 5f + Max(AttributeType.Life) * 0.25f;

		public float CalculateSkillCooldownModifier() => -0.025f * Val(AttributeType.Will) + 1;

		public int CalculateMaxHealth() => (int) (Max(AttributeType.Life) * PlayerHealthChunkSize);

		public int CalculateInitialHealth()
		{
			int startingHealth = (int) (Val(AttributeType.Life) * PlayerHealthChunkSize);
			return startingHealth;
		}

		public void ResetValues()
		{
			Get(AttributeType.Life).SetToMax();
			Get(AttributeType.Will).SetToMax();
		}

		public void CalculateNewLife(float health)
		{
			float newLife = Mathf.CeilToInt(health / PlayerHealthChunkSize);
			SetVal(AttributeType.Life, newLife);
		}

		public void UnlockWeaponSkillTwo(WeaponType weaponType, bool showScreen)
		{
			if (WeaponSkillTwoUnlocks.Contains(weaponType)) return;
			WeaponSkillTwoUnlocks.Add(weaponType);
			if (!showScreen) return;
			UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.Weapon.WeaponSkillTwo, 4);
		}

		public void UnlockWeaponSkillOne(WeaponType weaponType, bool showScreen)
		{
			if (WeaponSkillOneUnlocks.Contains(weaponType)) return;
			WeaponSkillOneUnlocks.Add(weaponType);
			if (!showScreen) return;
			UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.Weapon.WeaponSkillOne, 3);
		}

		public void UnlockCharacterSkillOne(bool showScreen)
		{
			if (SkillOneUnlocked) return;
			SkillOneUnlocked = true;
			if (!showScreen) return;
			UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillOne, 1);
		}

		public void UnlockCharacterSkillTwo(bool showScreen)
		{
			if (SkillTwoUnlocked) return;
			SkillTwoUnlocked = true;
			if (!showScreen) return;
			UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillTwo, 2);
		}

		public float CalculateDashCooldown() => 5f - Max(AttributeType.Life) * 0.2f;
	}
}