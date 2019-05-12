using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global;

using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public class FountainBehaviour : BasicShrineBehaviour, ICombatEvent
	{
		private static GameObject        _fountainPrefab;
		private static FountainBehaviour _instance;
		private        bool              _allEnemiesDead;
		private        AudioSource       _audioSource;
		private        string            _controlText;

		private List<EnemyBehaviour> _enemies;
		private ParticleSystem[]     _particleSystems;
		private Region               _region;
		private bool                 _shownText;

		public float InRange() => IsInRange && !Triggered ? 1 : -1;

		public string GetEventText() => "Drink from the fountain... [" + _controlText + "]";

		public void Activate()
		{
			if (Triggered) return;
			StopEffects();
			StartCoroutine(SpawnEnemies());
		}

		public void Awake()
		{
			_instance        = this;
			_particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
			_audioSource     = GetComponent<AudioSource>();

			List<Vector2> points = new List<Vector2>();
			for (int i = 0; i < points.Count; ++i)
			{
				float   angle    = 360f / 25;
				Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 1.25f, Vector2.zero);
				points.Add(position);
			}

			Polygon b = new Polygon(points, Vector2.zero);
			WorldGrid.AddBarrier(b);
			ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
			controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
		}

		private void UpdateText()
		{
			_controlText = InputHandler.GetBindingForKey(InputAxis.TakeItem);
		}

		private void OnDestroy()
		{
			_instance = null;
		}

		public static FountainBehaviour Instance() => _instance;

		public static void Generate(Region region)
		{
			if (_fountainPrefab == null) _fountainPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fountain");
			GameObject riteShrineObject                  = Instantiate(_fountainPrefab);
			riteShrineObject.GetComponent<FountainBehaviour>().Initialise(region);
		}

		private void Initialise(Region region)
		{
			_region            = region;
			transform.position = Vector2.zero;
			WorldGrid.AddBlockingArea(Vector2.zero, 1.5f);
			if (!_region.FountainVisited) return;
			StopEffects();
			Destroy(this);
		}

		private IEnumerator SpawnEnemies()
		{
			int             daysSpent      = WorldState.GetDaysSpentHere() + 5;
			List<EnemyType> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
			float           timeToSpawn    = 0f;
			for (int i = 0; i < Random.Range(daysSpent / 2f, daysSpent); ++i)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				while (timeToSpawn > 0f)
				{
					if (!CombatManager.Instance().IsCombatActive()) yield return null;
					timeToSpawn -= Time.deltaTime;
					yield return null;
				}

				Vector2 spawnPosition = WorldGrid.GetCellNearMe(transform.position, 5).Position;
				SpawnTrailController.Create(transform.position, spawnPosition, allowedEnemies.RandomElement());
				timeToSpawn = Random.Range(0.5f, 1f);
				yield return null;
			}
		}

		public void Update()
		{
			if (!_shownText && PlayerCombat.Instance != null && PlayerCombat.Instance.transform.position.magnitude < 6)
			{
				CombatLogController.PostLog("The fountain beckons");
				_shownText = true;
			}

			if (!Triggered || _allEnemiesDead) return;
			if (!CombatManager.Instance().ClearOfEnemies()) return;
			Succeed();
			_allEnemiesDead = true;
		}

		protected override void Succeed()
		{
			CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Life).Increment();
			CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Will).Increment();
			PlayerCombat.Instance.HealthController.Heal(1000000);
			PlayerCombat.Instance.ResetCompass();
			CombatLogController.PostLog("Health recovered");
			CombatLogController.PostLog("Hunger and Thirst restored");
			foreach (ParticleSystem system in _particleSystems) system.Stop();
			_audioSource.DOFade(0f, 2f);
		}

		protected override void StartShrine()
		{
		}

		private void StopEffects()
		{
			_region.FountainVisited = true;
			GetComponent<CompassItem>().Die();
			Triggered = true;
		}
	}
}