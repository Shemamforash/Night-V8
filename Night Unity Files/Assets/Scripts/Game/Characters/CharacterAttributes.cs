using System.Collections.Generic;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
	public class CharacterAttributes : DesolationAttributes
	{
		private const    int    PlayerHealthChunkSize = 100;
		private readonly Player _player;
		public float DecayExplodeChance;
		public float FireExplodeChance;
		public float FreeSkillChance;
		public float CompassBonus = 0;
		public float RallyHealthModifier;
		public bool  ReloadOnEmptyMag;
		public bool  ReloadOnFatalShot;
		public bool  SpreadVoid;

		private readonly string[] _hungerEvents =
		{
			"I have to get something to eat",
			"My stomach has been empty for too long",
			"I can't go on if I don't get any food"
		};

		private readonly string[] _thirstEvents =
		{
			"I'm going to die if I don't get any water",
			"I'm so thirsty, it's been so long since I had a drink",
			"I need to get some water soon"
		};

		public CharacterAttribute Life => Get(AttributeType.Life);
		public CharacterAttribute Will => Get(AttributeType.Will);

		public CharacterAttributes(Player player) => _player = player;

		public void IncreaseAttribute(AttributeType attributeType)
		{
			CharacterAttribute attribute = attributeType == AttributeType.Life ? Life : Will;
			if (attribute.Max == 20) return;
			attribute.Max += 1;
		}

		public void ResetValues()
		{
			Life.SetToMax();
			Will.SetToMax();
		}

		public float Speed()                    => 5f + Life.Max * 0.25f;
		public float CooldownModifier()         => -0.025f       * Will.CurrentValue + 1;
		public void  HealthToLife(float health) => Life.CurrentValue = (Mathf.CeilToInt(health / PlayerHealthChunkSize));
		public int   Health()                   => (int) Life.CurrentValue * PlayerHealthChunkSize;
		public int   MaxHealth()                => (int) Life.Max            * PlayerHealthChunkSize;
		public float DashCooldown()             => 5f - Life.Max * 0.2f;
	}
}