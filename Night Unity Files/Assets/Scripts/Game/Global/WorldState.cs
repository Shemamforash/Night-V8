using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Facilitating;
using Extensions;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Gear;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Global
{
	public class WorldState : MonoBehaviour
	{
		public const  int   MinutesPerHour     = 12;
		public const  float MinuteInSeconds    = 1f;
		private const float DayLengthInSeconds = 24f * MinutesPerHour * MinuteInSeconds;
		private const int   MinuteInterval     = 60                   / MinutesPerHour;
		private const int   MaxDifficulty      = 50;

		private static int DaysSpentHere;

		private static readonly List<EnemyType> _allowedHumanEnemies     = new List<EnemyType>();
		private static readonly List<EnemyType> _allowedNightmareEnemies = new List<EnemyType>();

		private static int   MinutesPassed;
		private static float _currentTime;
		public static  int   Days, Hours = 6, Minutes;
		private static int   _difficulty;
		private static bool  _isNight, _isPaused;
		public static  int   Seed = -1;
		private static int   _templesActivated;
		private static int   _timeAtLastSave;

		private static DifficultySetting _difficultySetting;
		private static float             EnemyDamageModifier, EnemyHealthModifier;
		private        bool              _seenTutorial;

		public static float GetEnemyDamageModifier() => EnemyDamageModifier;
		public static float GetEnemyHealthModifier() => EnemyHealthModifier;

		public static void SetDifficultyEasy()
		{
			_difficultySetting = DifficultySetting.Easy;
			SetDifficultyModifiers();
		}

		public static void SetDifficultyHard()
		{
			_difficultySetting = DifficultySetting.Hard;
			SetDifficultyModifiers();
		}

		private static void SetDifficultyModifiers()
		{
			if (_difficultySetting == DifficultySetting.Easy)
			{
				EnemyDamageModifier = 0.1f;
				EnemyHealthModifier = 0.8f;
				return;
			}

			EnemyDamageModifier = 0.2f;
			EnemyHealthModifier = 1f;
		}

		public static int GetRemainingTemples() => EnvironmentManager.CurrentEnvironment.Temples - _templesActivated;

		public void Awake()
		{
			Resume();
		}

		public static void Load(XmlNode doc)
		{
			ResetWorld(false);
			XmlNode worldStateValues = doc.GetChild("WorldState");
			Seed          = worldStateValues.ParseInt("Seed");
			DaysSpentHere = worldStateValues.ParseInt("DaysSpentHere");
			Days          = worldStateValues.ParseInt("Days");
			Hours         = worldStateValues.ParseInt("Hours");
			Assert.IsTrue(Hours <= 24);
			Minutes            = worldStateValues.ParseInt("Minutes");
			MinutesPassed      = worldStateValues.ParseInt("MinutesPassed");
			_difficulty        = worldStateValues.ParseInt("Difficulty");
			_difficultySetting = (DifficultySetting) worldStateValues.ParseInt("DifficultySetting");
			_templesActivated  = worldStateValues.ParseInt("TemplesActivated");
			SetDifficultyModifiers();
			Inventory.Load(doc);
			Building.LoadBuildings(doc);
			Recipe.Load(doc);
			EnvironmentManager.Load(doc);
			MapGenerator.Load(doc);
			CharacterManager.Load(doc);
			WeatherManager.Load(doc);
			WorldEventManager.Load(doc);
			JournalEntry.Load(doc);
			TutorialManager.Load(doc);
			Campfire.Load(doc);
			UiGearMenuController.Load(doc);
			_timeAtLastSave = 0;
		}

		public static void Save(XmlNode doc)
		{
			XmlNode worldStateValues = doc.CreateChild("WorldState");
			worldStateValues.CreateChild("Seed",              Seed);
			worldStateValues.CreateChild("DaysSpentHere",     DaysSpentHere);
			worldStateValues.CreateChild("Days",              Days);
			worldStateValues.CreateChild("Hours",             Hours);
			worldStateValues.CreateChild("Minutes",           Minutes);
			worldStateValues.CreateChild("MinutesPassed",     MinutesPassed);
			worldStateValues.CreateChild("Difficulty",        _difficulty);
			worldStateValues.CreateChild("RealTime",          DateTime.Now.ToString("MMMM dd '-' hh:mm tt", CultureInfo.InvariantCulture));
			worldStateValues.CreateChild("DifficultySetting", (int) _difficultySetting);
			worldStateValues.CreateChild("TemplesActivated",  _templesActivated);
			Inventory.Save(doc);
			Building.SaveBuildings(doc);
			Recipe.Save(doc);
			CharacterManager.Save(doc);
			MapGenerator.Save(doc);
			WeatherManager.Save(doc);
			EnvironmentManager.Save(doc);
			WorldEventManager.Save(doc);
			JournalEntry.Save(doc);
			TutorialManager.Save(doc);
			Campfire.Save(doc);
			UiGearMenuController.Save(doc);
			_timeAtLastSave = 0;
		}

		public static void ResetWorld(bool clearSave = true, int difficulty = 0)
		{
			Inventory.Reset();
			CharacterManager.Reset(clearSave);
			JournalEntry.Reset();
			TutorialManager.ResetTutorial();
			DaysSpentHere = 0;
			Days          = 0;
			Hours         = 6;
			Minutes       = 0;
			_difficulty   = difficulty;
			_isNight      = false;
			_isPaused     = false;
			Seed          = Random.Range(0, int.MaxValue);
#if UNITY_EDITOR
//            _difficulty = 40;
#endif
			Random.InitState(Seed);
			EnvironmentManager.Reset(!clearSave);
			WeatherManager.Reset();
			WorldEventManager.Clear();
		}

		public void Start()
		{
			CharacterManager.Start();
			EnvironmentManager.Start();
			WeatherManager.Start();
			WorldView.MyUpdate(Hours);
			CharacterManager.Update();
			ShowExplorationTutorial();
		}

		private void ShowExplorationTutorial()
		{
			if (_seenTutorial || !TutorialManager.Active()) return;
			List<TutorialOverlay> overlays = new List<TutorialOverlay>
			{
				new TutorialOverlay(),
				new TutorialOverlay(WorldView.GetEnvironmentRect()),
				new TutorialOverlay()
			};
			TutorialManager.Instance.TryOpenTutorial(1, overlays);
			_seenTutorial = true;
		}

		public static void ActivateTemple()
		{
			++_templesActivated;
		}

		public static bool AllTemplesActive()
		{
#if UNITY_EDITOR
			return true;
#endif
			return _templesActivated >= EnvironmentManager.CurrentEnvironment.Temples;
		}

		private void IncrementDaysSpentHere()
		{
			++DaysSpentHere;
		}

		public static int Difficulty() => _difficulty;

		public static int NormalisedDifficulty() => _difficulty / MaxDifficulty;

		public static int GetDaysSpentHere() => DaysSpentHere;

//        public static int CurrentLevel() => _currentLevel;

		public static void TravelToNextEnvironment()
		{
			_templesActivated = 0;
			DaysSpentHere     = 0;
			EnvironmentManager.NextLevel(false, false);
			CharacterManager.Wanderer.TravelAction.ReturnToHomeInstant(false);
			CharacterManager.AlternateCharacter?.TravelAction.ReturnToHomeInstant(false);
			StoryController.Show();
		}

		public static void Pause()
		{
			_isPaused = true;
			PauseMenuController.Pause();
		}

		public static void Resume()
		{
			_isPaused = false;
			PauseMenuController.Resume();
		}

		private void IncrementWorldTime()
		{
			_currentTime += Time.deltaTime;
			if (_currentTime < MinuteInSeconds) return;
			_currentTime = _currentTime - MinuteInSeconds;
			IncrementMinutes();
			WorldView.MyUpdate(Hours);
		}

		private void IncrementMinutes()
		{
			Minutes += MinuteInterval;
			if (Minutes == 60)
			{
				Minutes = 0;
				IncrementHours();
			}

			MinutePasses();
		}

		private static void MinutePasses()
		{
			WeatherManager.CurrentWeather().Update();
			EnvironmentManager.UpdateTemperature();
			MapGenerator.DiscoveredRegions().ForEach(r => r.Update());
			CharacterManager.Update();
			Campfire.Die();
			++MinutesPassed;
		}

		private static void HourPasses()
		{
			Inventory.UpdateBuildings();
			++_timeAtLastSave;
			if (_timeAtLastSave % 12 == 0) WorldEventManager.SuggestSave();
			bool increaseDifficulty = Hours % 4 == 0;
			if (!increaseDifficulty) return;
			if (_difficulty < ((int) EnvironmentManager.CurrentEnvironmentType + 1) * 10) ++_difficulty;
			SaveIconController.AutoSave();
		}

		private void IncrementHours()
		{
			++Hours;
			HourPasses();
			if (Hours >= 24)
			{
				IncrementDays();
				Hours = 0;
			}

			if (Hours >= 6 && Hours < 20 && _isNight)
			{
				_isNight = false;
			}
			else if ((Hours < 6 || Hours >= 20) && !_isNight)
			{
				_isNight = true;
			}
		}

		private void IncrementDays()
		{
			++Days;
			IncrementDaysSpentHere();
		}

		public void Update()
		{
//            Debug.Log(EventSystem.current.currentSelectedGameObject.name);
			UpdateScenery();
			if (_isPaused) return;
			if (MapMenuController.CharacterReturning != null) return;
			IncrementWorldTime();
		}

		private static void UpdateScenery()
		{
			int   minutesPassed  = Hours         * MinutesPerHour  + Minutes / MinuteInterval;
			float timePassed     = minutesPassed * MinuteInSeconds + _currentTime;
			float normalisedTime = timePassed / DayLengthInSeconds;
			SceneryController.SetTime(normalisedTime);
		}

		public static ItemQuality GenerateGearLevel()
		{
			int difficulty    = Mathf.FloorToInt(Difficulty() / 10f);
			int difficultyMin = difficulty - 1;
			if (difficultyMin < 0)
			{
				difficultyMin = 0;
			}
			else if (difficultyMin > 4) difficultyMin = 4;

			int difficultyMax                    = difficulty + 1;
			if (difficultyMax > 4) difficultyMax = 4;
			return (ItemQuality) Random.Range(difficultyMin, difficultyMax);
		}

		private static void CheckEnemyUnlock()
		{
			List<EnemyTemplate> enemyTypes = EnemyTemplate.GetEnemyTypes();
			enemyTypes.ForEach(e =>
			{
				if (e.Difficulty > Difficulty()) return;
				EnemyType enemyType = e.EnemyType;
				switch (e.Species)
				{
					case "Animal":
						return;
					case "Human":
						_allowedHumanEnemies.AddOnce(enemyType);
						return;
					case "Nightmare":
						_allowedNightmareEnemies.AddOnce(enemyType);
						return;
				}
			});
		}

		public static List<EnemyType> GetAllowedHumanEnemyTypes()
		{
			CheckEnemyUnlock();
			return _allowedHumanEnemies;
		}

		public static List<EnemyType> GetAllowedNightmareEnemyTypes()
		{
			CheckEnemyUnlock();
			return _allowedNightmareEnemies;
		}

		public static bool Paused() => _isPaused;

		public static int ScaleValue(int value) => (int) (value + value * _difficulty / 25f);

		public static void OverrideDifficulty(object difficulty)
		{
			_difficulty = (int) difficulty;
		}

		private enum DifficultySetting
		{
			Easy,
			Hard
		}
	}
}