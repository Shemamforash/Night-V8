using System;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using Game.Characters;
using Game.Combat.Enemies.Misc;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
	public static class CharacterSkills
	{
		public static Skill GetCharacterSkillOne(Characters.Player player)
		{
			switch (player.CharacterTemplate.CharacterClass)
			{
				case CharacterClass.Villain:
					return new Drain();
				case CharacterClass.Beast:
					return new Immolate();
				case CharacterClass.Survivor:
					return new Scour();
				case CharacterClass.Protector:
					return new Regenerate();
				case CharacterClass.Hunter:
					return new Lacerate();
				case CharacterClass.Wanderer:
					return new Staunch();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static Skill GetCharacterSkillTwo(Characters.Player player)
		{
			switch (player.CharacterTemplate.CharacterClass)
			{
				case CharacterClass.Villain:
					return new Defile();
				case CharacterClass.Beast:
					return new Leach();
				case CharacterClass.Survivor:
					return new Erupt();
				case CharacterClass.Protector:
					return new Wake();
				case CharacterClass.Hunter:
					return new Curse();
				case CharacterClass.Wanderer:
					return new Fracture();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
	//Villain

	public class Drain : Skill
	{
		public Drain() : base(nameof(Drain))
		{
		}

		protected override void InstantEffect()
		{
			List<CanTakeDamage> characters = CombatManager.Instance().GetCharactersInRange(PlayerPosition(), 1);
			characters.Remove(Player());
			List<CanTakeDamage> sickened = SickenBehaviour.Create(PlayerPosition(), new List<CanTakeDamage> {Player()});
			Heal(sickened.Count * 0.025f);
		}
	}

	public class Defile : Skill
	{
		public Defile() : base(nameof(Defile))
		{
		}

		protected override void InstantEffect()
		{
			List<CanTakeDamage> sickened = SickenBehaviour.Create(PlayerPosition(), new List<CanTakeDamage> {Player()});
			sickened.ForEach(e =>
			{
				if (e.HealthController.GetHealth().CurrentValue != 0) return;
				Explosion explosion = Explosion.CreateExplosion(e.transform.position, 0.5f);
				explosion.AddIgnoreTarget(Player());
				explosion.InstantDetonate();
			});
		}
	}

	//Beast

	public class Immolate : Skill
	{
		public Immolate() : base(nameof(Immolate))
		{
		}

		protected override void InstantEffect()
		{
			VortexBehaviour.Create(PlayerPosition(), () =>
			{
				Explosion explosion = Explosion.CreateExplosion(PlayerPosition());
				explosion.AddIgnoreTarget(Player());
				explosion.SetBurn();
				explosion.InstantDetonate();
			});
		}
	}

	public class Leach : Skill
	{
		private static float _timePassed;

		public Leach() : base(nameof(Leach))
		{
		}

		protected override void PassiveEffect(Shot s)
		{
			List<CanTakeDamage> enemiesNearby = CombatManager.Instance().GetEnemiesInRange(PlayerPosition(), 1.5f);
			enemiesNearby.Remove(Player());
			int enemyCount = enemiesNearby.Count;
			_timePassed += Time.deltaTime;
			if (_timePassed < 1f) return;
			_timePassed -= 1f;
			float healAmount = enemyCount * 0.005f;
			Heal(healAmount);
		}
	}


	//Survivor

	public class Scour : Skill
	{
		public Scour() : base(nameof(Scour))
		{
		}

		protected override void PassiveEffect(Shot s)
		{
			s.Attributes().AddOnHit(() =>
			{
				int refillProbability = 2 * s._origin.Weapon().WeaponAttributes.Pellets();
				if (!NumericExtensions.RollDie(0, refillProbability)) return;
				Player()._weaponBehaviour.IncreaseAmmo(1);
				Heal(0.01f);
			});
		}
	}

	public class Erupt : Skill
	{
		public Erupt() : base(nameof(Erupt))
		{
		}

		private void CreateExplosion(Vector2 position, float radius)
		{
			Explosion e = Explosion.CreateExplosion(position, radius);
			e.AddIgnoreTarget(Player());
			e.InstantDetonate();
		}

		protected override void InstantEffect()
		{
			Vector2  position = PlayerPosition();
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(0.5f);
			sequence.AppendCallback(() =>
			{
				int   explosionCount = 8;
				float angleInterval  = 360f / explosionCount;
				for (int i = 0; i < explosionCount; ++i)
				{
					Vector2 pos  = AdvancedMaths.CalculatePointOnCircle(angleInterval * i, Random.Range(1.5f, 2f), position);
					Cell    cell = WorldGrid.WorldToCellPosition(pos, false);
					if (cell == null || !cell.Reachable) continue;
					Sequence subSequence = DOTween.Sequence();
					subSequence.AppendInterval(Random.Range(0.1f, 0.2f));
					subSequence.AppendCallback(() => CreateExplosion(pos, 0.5f));
				}
			});
		}
	}

	//Protector

	public class Regenerate : Skill
	{
		public Regenerate() : base(nameof(Regenerate))
		{
		}

		protected override void InstantEffect()
		{
			float t = 0f;
			Player().UpdateSkillActions.Add(() =>
			{
				t -= Time.deltaTime;
				if (t > 0) return;
				Heal(0.01f);
				t = 0.5f;
			});
		}
	}

	public class Wake : Skill
	{
		private Sequence _sequence;

		public Wake() : base(nameof(Wake))
		{
		}

		protected override void InstantEffect()
		{
			_sequence?.Kill();
			LeaveFireTrail fireTrail = Player().GetComponent<LeaveFireTrail>();
			if (fireTrail == null)
			{
				fireTrail = Player().gameObject.AddComponent<LeaveFireTrail>();
				fireTrail.Initialise();
			}

			_sequence = DOTween.Sequence();
			_sequence.AppendInterval(10f);
			_sequence.AppendCallback(() =>
			{
				if (Player() == null) return;
				Object.Destroy(fireTrail);
			});
		}
	}

	//Hunter

	public class Lacerate : Skill
	{
		public Lacerate() : base(nameof(Lacerate))
		{
		}

		protected override void InstantEffect()
		{
			Vector3 playerPosition = PlayerPosition();
			Vector2 targetPosition = playerPosition + PlayerTransform().up;
			Grenade g              = Grenade.CreateBasic(playerPosition, targetPosition, true);
			g.AddOnDetonate(enemies => { Heal(enemies.Count * 0.025f); });
		}
	}

	public class Curse : Skill
	{
		public Curse() : base(nameof(Curse))
		{
		}

		protected override void InstantEffect()
		{
			MarkController.Create(PlayerPosition());
		}
	}

	//Wanderer

	public class Staunch : Skill
	{
		public Staunch() : base(nameof(Staunch))
		{
		}

		protected override void InstantEffect()
		{
			Heal(0.25f);
		}
	}

	public class Fracture : Skill
	{
		public Fracture() : base(nameof(Fracture))
		{
		}

		protected override void InstantEffect()
		{
			Vector3 playerPosition = PlayerPosition();
			Vector2 targetPosition = playerPosition + PlayerTransform().up;
			Grenade.CreateDecay(playerPosition, targetPosition, true);
		}
	}
}