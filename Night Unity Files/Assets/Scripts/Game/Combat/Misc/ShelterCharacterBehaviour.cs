using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class ShelterCharacterBehaviour : CharacterCombat
	{
		private static   GameObject                _shelterCharacterPrefab;
		private static   ShelterCharacterBehaviour _instance;
		private readonly string                    _textString = "Free the prisoner";
		private          MoveBehaviour             _moveBehaviour;
		private          bool                      _seenText, _freed, _leaving;
		private          Sequence                  _sequence;
		private          Cell                      _targetCell;
		private          Cell                      _targetLastCell;

		public static ShelterCharacterBehaviour Instance() => _instance;

		protected override void Awake()
		{
			SpriteFlash        = gameObject.FindChildWithName<DamageSpriteFlash>("Icon");
			_bloodSpatter      = SpriteFlash.GetComponent<BloodSpatter>();
			Sprite             = SpriteFlash.GetComponent<SpriteRenderer>();
			MovementController = GetComponent<MovementController>();
			WeaponAudio        = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");

			IsPlayer  = true;
			_instance = this;
			HealthController.SetInitialHealth(1000, this);
			transform.SetParent(GameObject.Find("World").transform);
			_moveBehaviour = GetComponent<MoveBehaviour>();
			MovementController.SetSpeed(1);
			ArmourController.AutoFillSlots(10);
		}

		public override Weapon Weapon() => null;

		public override CanTakeDamage GetTarget() => null;

		public static void Generate()
		{
			if (_shelterCharacterPrefab == null) _shelterCharacterPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shelter Character");
			GameObject characterObject                                   = Instantiate(_shelterCharacterPrefab);
			characterObject.transform.position = Vector2.zero;
		}

		public void Update()
		{
			MyUpdate();
		}

		private void UpdateRotation()
		{
			float zRot;
			if (_freed)
			{
				zRot = AdvancedMaths.AngleFromUp(Vector2.zero, MovementController.GetVelocity());
			}
			else
			{
				zRot = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Position());
			}

			Sprite.transform.rotation = Quaternion.Euler(0, 0, zRot);
		}

		public override void MyUpdate()
		{
			if (!CombatManager.Instance().IsCombatActive()) return;
			TryShowText();
			UpdateRotation();
			if (_leaving) return;
			UpdateEnemyTarget();
			CheckToLeave();
		}

		private void CheckToLeave()
		{
			if (_targetCell                                       == null) return;
			if (transform.position.Distance(_targetCell.Position) > 0.25f) return;
			_leaving = true;
			CombatManager.Instance().ClearInactiveEnemies();
			ResetEnemyTargets();
			Characters.Player character = CharacterManager.CurrentRegion().CharacterHere;
			CharacterManager.SetAlternateCharacter(character);
			CharacterManager.CurrentRegion().CharacterHere = null;
			TeleportInOnly.TeleportIn(transform.position);
			_sequence?.Kill();
			EventTextController.SetOverrideText("The prisoner has been rescued");
			_sequence = DOTween.Sequence();
			_sequence.AppendInterval(2f);
			_sequence.AppendCallback(EventTextController.CloseOverrideText);
			Destroy(gameObject);
		}

		private void ResetEnemyTargets()
		{
			CombatManager.Instance().Enemies().ForEach(e => ((EnemyBehaviour) e).SetTarget(PlayerCombat.Instance));
		}

		private void UpdateEnemyTarget()
		{
			CombatManager.Instance().Enemies().ForEach(e =>
			{
				float         distanceToPlayer = e.transform.Distance(PlayerCombat.Instance.transform);
				CanTakeDamage target           = distanceToPlayer < 5f ? PlayerCombat.Instance : (CanTakeDamage) this;
				((EnemyBehaviour) e).SetTarget(target);
			});
		}

		public override string GetDisplayName() => new[] {"Lonely Wanderer", "Stranger", "Haggard Figure"}.RandomElement();

		public override void Kill()
		{
			base.Kill();
			ResetEnemyTargets();
		}

		public override void TakeShotDamage(Shot shot)
		{
			HealthController.TakeDamage(shot.Attributes().DamageDealt());
		}

		private void TryShowText()
		{
			if (_seenText) return;
			if (transform.Distance(PlayerCombat.Instance.transform) > 5f) return;
			_seenText = true;
			EventTextController.SetOverrideText(_textString);
			_sequence = DOTween.Sequence();
			_sequence.AppendInterval(3f);
			_sequence.AppendCallback(EventTextController.CloseOverrideText);
			_seenText = true;
		}

		public void Free()
		{
			_freed      = true;
			_targetCell = WorldGrid.GetEdgeCell(transform.position);
			_moveBehaviour.GoToCell(_targetCell);
			List<EnemyType> enemies      = new List<EnemyType>();
			List<EnemyType> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
			for (int i = 0; i < 100; ++i) enemies.Add(allowedTypes.RandomElement());
			CombatManager.Instance().OverrideInactiveEnemies(enemies);
			_sequence?.Kill();
			EventTextController.SetOverrideText("Protect the prisoner");
			_sequence = DOTween.Sequence();
			_sequence.AppendInterval(2f);
			_sequence.AppendCallback(EventTextController.CloseOverrideText);
		}
	}
}