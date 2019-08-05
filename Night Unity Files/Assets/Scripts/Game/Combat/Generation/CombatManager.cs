using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using Game.Global;
using Extensions;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat.Generation
{
	public class CombatManager : Menu
	{
		private static CombatManager _instance;

		private static           Region              _currentRegion;
		private static           Characters.Player   _player;
		private readonly         List<CanTakeDamage> _enemies = new List<CanTakeDamage>();
		[SerializeField] private bool                _drawGizmos;
		private                  bool                _forceShowHud;
		private                  CanvasGroup         _hudCanvas;
		private                  bool                _hudShown;
		private                  Tweener             _hudTween;
		private                  List<EnemyType>     _inactiveEnemies;
		private                  bool                _inCombat;
		private                  int                 _maxSize;
		private                  bool                _paused;
		private                  bool                _playerReachedCentre;
		private                  float               _timeSinceLastSpawn;
		private                  float               _visibilityRange;

		protected override void Awake()
		{
			base.Awake();
			PauseOnOpen = false;
//			_hudCanvas       = gameObject.FindChildWithName<CanvasGroup>("Combat");
//			_hudCanvas.alpha = 0f;
			_instance = this;
			Resume();
		}

		private void Start()
		{
			_inCombat = true;
			CalculateVisibility();

			_currentRegion = CharacterManager.CurrentRegion();
			if (_currentRegion.IsDynamic())
				SetUpDynamicRegion();
			else
				SetUpNonDynamicRegion();

			PlayerCombat.Instance.Initialise();
			_inactiveEnemies = _currentRegion.GetEnemies();
			_maxSize         = WorldState.Difficulty() / 10 + 5;
			PlaceAnimals();
			_currentRegion.CheckForRegionExplored();
		}

		private void Update()
		{
			if (!IsCombatActive()) return;
//			UpdateHud();
			TrySpawnNewEnemy();
			AIMoveManager.UpdateMoveBehaviours();
			if (Time.timeSinceLevelLoad < 1f) return;
			PlayerCombat.Instance.MyUpdate();
			for (int i = _enemies.Count - 1; i >= 0; --i)
			{
				if (i > _enemies.Count) --i;
				_enemies[i].MyUpdate();
			}
		}

		private void UpdateHud()
		{
			bool needToShow = !ClearOfEnemies() || _forceShowHud;
			bool needToHide = ClearOfEnemies() && !_forceShowHud;
			if (needToShow && !_hudShown)
			{
				_hudTween?.Kill();
				_hudTween = _hudCanvas.DOFade(1f, 1f);
				_hudShown = true;
			}
			else if (needToHide && _hudShown)
			{
				_hudTween?.Kill();
				_hudTween = _hudCanvas.DOFade(0f, 1f);
				_hudShown = false;
			}
		}

		private void OnDestroy() => _instance = null;

		private void OnDrawGizmos()
		{
			if (!_drawGizmos) return;
			Vector3 cubeSize = 1f / WorldGrid.CellResolution * Vector3.one;
			Gizmos.color = Color.red;
			List<Cell> invalid = WorldGrid.InvalidCells();
			invalid.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
			Gizmos.color = Color.yellow;
			WorldGrid._outOfRangeList.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
			WorldGrid._edgePositionList.ForEach(c =>
			{
				{
					Gizmos.color = new Color(0, 1, 0, 0.25f); //Color.green;
					Gizmos.DrawCube(c.Position, cubeSize);
				}
			});
		}

		private void PlayNightmareParticles()
		{
			ParticleSystem             _nightmareParticles = GameObject.Find("Nightmare Particles").GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shape               = _nightmareParticles.shape;
			shape.radius = WorldGrid.CombatAreaWidth / 2f + 6f;
			_nightmareParticles.Play();
		}

		private void SetUpDynamicRegion()
		{
			GameObject worldObject = GameObject.Find("World");
			switch (EnvironmentManager.CurrentEnvironmentType)
			{
				case EnvironmentType.Desert:
					worldObject.AddComponent<Desert>().Initialise();
					break;
				case EnvironmentType.Mountains:
					worldObject.AddComponent<Labyrinth>().Initialise();
					break;
				case EnvironmentType.Ruins:
					worldObject.AddComponent<Ruins>().Initialise();
					break;
				case EnvironmentType.Sea:
					worldObject.AddComponent<Steppe>().Initialise();
					break;
				case EnvironmentType.Wasteland:
					worldObject.AddComponent<Canyon>().Initialise();
					break;
			}
		}

		private void SetUpNonDynamicRegion()
		{
			GameObject worldObject = GameObject.Find("World");
			switch (_currentRegion.GetRegionType())
			{
				case RegionType.Tomb:
					worldObject.AddComponent<Tomb>().Initialise();
					break;
				case RegionType.Rite:
					worldObject.AddComponent<Rite>().Initialise();
					break;
				case RegionType.Temple:
					worldObject.AddComponent<Temple>().Initialise();
					break;
				case RegionType.Tutorial:
					worldObject.AddComponent<Tutorial>().Initialise();
					break;
			}

			AudioController.FadeWeatherOut();
			PlayNightmareParticles();
			_visibilityRange = 10f;
		}

		private void CalculateVisibility()
		{
			float visibilityModifier = 0.5f * Mathf.Sin((WorldState.Hours - 6) * Mathf.PI / 12f) + 0.5f;
			_visibilityRange *= WeatherManager.CurrentWeather().GetVisibility();
			_visibilityRange =  Mathf.Lerp(3f, 8f, visibilityModifier);
		}

		private bool HasPlayerReachedCentre()
		{
			if (_playerReachedCentre) return true;
			if (PlayerCombat.Instance                              == null) return false;
			if (PlayerCombat.Instance.transform.position.magnitude > 8) return false;
			_playerReachedCentre = true;
			return true;
		}

		private void TrySpawnNewEnemy()
		{
			if (!HasPlayerReachedCentre()) return;
			if (_inactiveEnemies.Empty()) return;
			if (_enemies.Count >= _maxSize) return;
			_timeSinceLastSpawn -= Time.deltaTime;
			if (_timeSinceLastSpawn > 0 && _enemies.Count > 0) return;
			_timeSinceLastSpawn = Random.Range(0.5f, 1f);
			EnemyType      enemyType = _inactiveEnemies.RemoveLast();
			EnemyBehaviour enemy     = EnemyTemplate.Create(enemyType);
			TeleportInOnly.TeleportIn(enemy.transform.position);
		}

		private void PlaceAnimals()
		{
			if (_currentRegion.GetRegionType() != RegionType.Animal) return;
			List<List<EnemyBehaviour>> GrazerHerds       = new List<List<EnemyBehaviour>>();
			List<EnemyBehaviour>       currentGrazerHerd = new List<EnemyBehaviour>();
			List<List<EnemyBehaviour>> FlitHerds         = new List<List<EnemyBehaviour>>();
			List<EnemyBehaviour>       currentFlitHerd   = new List<EnemyBehaviour>();
			_inactiveEnemies.ForEach(e =>
			{
				EnemyBehaviour enemyBehaviour = EnemyTemplate.Create(e);
				switch (e)
				{
					case EnemyType.Grazer:
						if (currentGrazerHerd.Count == 0)
						{
							GrazerHerds.Add(currentGrazerHerd);
						}

						currentGrazerHerd.Add(enemyBehaviour);
						if (Random.Range(3, 6) >= currentGrazerHerd.Count)
						{
							currentGrazerHerd = new List<EnemyBehaviour>();
						}

						break;
					case EnemyType.Curio:
						if (currentFlitHerd.Count == 0)
						{
							FlitHerds.Add(currentFlitHerd);
						}

						currentFlitHerd.Add(enemyBehaviour);
						if (Random.Range(5, 12) >= currentFlitHerd.Count)
						{
							currentFlitHerd = new List<EnemyBehaviour>();
						}

						break;
				}
			});

			_inactiveEnemies.Clear();
			PositionHerds(GrazerHerds, 1.5f);
			PositionHerds(FlitHerds,   1);
		}

		private void PositionHerds(List<List<EnemyBehaviour>> herds, float range)
		{
			herds.ForEach(herd =>
			{
				Vector3    animalSpawnPosition                                   = WorldGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
				List<Cell> cells                                                 = WorldGrid.GetCellsNearMe(WorldGrid.WorldToCellPosition(animalSpawnPosition), herd.Count, range);
				for (int i = 0; i < cells.Count; ++i) herd[i].transform.position = cells[i].Position;
			});
		}

		private void ReturnToMap()
		{
			if (_currentRegion.GetRegionType() == RegionType.Rite)
			{
				_player.TravelAction.SetCurrentRegion(Rite.GetLastRegion());
			}

			SceneChanger.GoToGameScene();
			MapMenuController.CharacterReturning = _player;
			InputHandler.SetCurrentListener(null);
		}

		public bool IsPlayerInCombat() => _inCombat && PlayerCombat.Alive;

		public void ExitCombat(bool returnToMap = true)
		{
			if (!_inCombat) return;
			RestoreEnemies();
			if (!_currentRegion.IsDynamic())
			{
				AudioController.FadeWeatherIn();
			}

			_inCombat = false;
			PlayerCombat.Instance.ExitCombat();
			if (returnToMap) ReturnToMap();
			_instance = null;
		}

		public void RestoreEnemies()
		{
			if (!_currentRegion.IsDynamic()) return;
			_inactiveEnemies.ForEach(e => _currentRegion.RestoreSize(e));
			_enemies.ForEach(e => _currentRegion.RestoreSize(e));
		}

		public void ClearInactiveEnemies() => _inactiveEnemies.Clear();

		public void OverrideMaxSize(int maxSize) => _maxSize = maxSize;

		public void OverrideInactiveEnemies(List<EnemyType> inactiveEnemies) => _inactiveEnemies = inactiveEnemies;

		public List<CanTakeDamage> GetCharactersInRange(Vector2 position, float range)
		{
			List<CanTakeDamage> charactersInRange = GetEnemiesInRange(position, range);
			if (Vector2.Distance(PlayerCombat.Position(), position) <= range) charactersInRange.Add(PlayerCombat.Instance);
			return charactersInRange;
		}

		public List<CanTakeDamage> GetEnemiesInRange(Vector2 position, float range)
		{
			return new List<CanTakeDamage>(_enemies.Where(e => Vector2.Distance(e.transform.position, position) <= range));
		}

		public void RemoveEnemy(CanTakeDamage enemy) => _enemies.Remove(enemy);

		public void AddEnemy(CanTakeDamage enemy) => _enemies.Add(enemy);

		public EnemyBehaviour SpawnEnemy(EnemyType enemyType, Vector2 position)
		{
			EnemyBehaviour enemy = EnemyTemplate.Create(enemyType);
			enemy.transform.position = position;
			TeleportInOnly.TeleportIn(position);
			return enemy;
		}

		public CanTakeDamage NearestEnemy(Vector2 position)
		{
			CanTakeDamage nearestEnemy    = null;
			float         nearestDistance = 10000;
			Enemies().ForEach(e =>
			{
				float distance = Vector2.Distance(e.transform.position, position);
				if (distance >= nearestDistance) return;
				nearestDistance = distance;
				nearestEnemy    = e;
			});
			return nearestEnemy;
		}

		public CanTakeDamage NearestCharacter(Vector2 position)
		{
			CanTakeDamage nearestEnemy   = NearestEnemy(position);
			float         playerDistance = Vector2.Distance(position, PlayerCombat.Position());
			float         enemyDistance  = Vector2.Distance(position, nearestEnemy.transform.position);
			return playerDistance < enemyDistance ? PlayerCombat.Instance : nearestEnemy;
		}

		public List<CanTakeDamage> Enemies() => _enemies;

		public void Pause()
		{
			if (_instance == null) return;
			_paused        = true;
			Time.timeScale = 0;
		}

		public void Resume()
		{
			if (_instance == null) return;
			if (PauseMenuController.IsOpen()) return;
			_paused        = false;
			Time.timeScale = 1;
		}


		public bool ClearOfEnemies() => Enemies().Count == 0 && _inactiveEnemies.Count == 0;

		public void LeaveCombat() => _inCombat = false;

		public void SetForceShowHud(bool forceShow) => _forceShowHud = forceShow;

		public int InactiveEnemyCount() => _inactiveEnemies.Count;

		public static CombatManager Instance()
		{
			if (_instance == null) _instance = FindObjectOfType<CombatManager>();
			return _instance;
		}

		public override void Enter()
		{
			base.Enter();
			_forceShowHud = false;
			InputHandler.SetCurrentListener(PlayerCombat.Instance);
		}

		public bool IsCombatActive() => _instance != null && PlayerCombat.Alive && !_paused;

		public float VisibilityRange() => _visibilityRange;

		public static void SetPlayer(Characters.Player player)
		{
			_player = player;
		}
	}
}