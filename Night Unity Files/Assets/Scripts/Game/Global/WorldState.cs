using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using DG.Tweening;
using Facilitating;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Game.Global
{
    public class WorldState : MonoBehaviour
    {
        public const int MinutesPerHour = 12;
        private const int IntervalSize = 60 / MinutesPerHour;
        public const float MinuteInSeconds = 1f;
        private const float DayLengthInSeconds = 24f * MinutesPerHour * MinuteInSeconds;
        private const int MinuteInterval = 60 / MinutesPerHour;

        private static int DaysSpentHere;
        public static int _currentLevel = 1;

        private static readonly List<EnemyTemplate> _allowedHumanEnemies = new List<EnemyTemplate>();
        private static readonly List<EnemyTemplate> _allowedNightmareEnemies = new List<EnemyTemplate>();
        private static bool _gateActive;

        private static int MinutesPassed;
        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        private static int _difficulty;
        private static bool _isNight, _isPaused;
        public static int Seed = -1;
        private static int _templesActivated;
        private static int _timeAtLastSave;
        public const int MaxDifficulty = 50;

        public static int GetRemainingTemples()
        {
            return EnvironmentManager.CurrentEnvironment.Temples - _templesActivated;
        }

        public void Awake()
        {
            UnPause();
        }

        public static void Load(XmlNode doc)
        {
            ResetWorld(false);
            XmlNode worldStateValues = doc.GetNode("WorldState");
            Seed = worldStateValues.IntFromNode("Seed");
            DaysSpentHere = worldStateValues.IntFromNode("DaysSpentHere");
            _currentLevel = worldStateValues.IntFromNode("CurrentLevel");
            Days = worldStateValues.IntFromNode("Days");
            Hours = worldStateValues.IntFromNode("Hours");
            Minutes = worldStateValues.IntFromNode("Minutes");
            MinutesPassed = worldStateValues.IntFromNode("MinutesPassed");
            _difficulty = worldStateValues.IntFromNode("Difficulty");
            Inventory.Load(doc);
            MapGenerator.Load(doc);
            CharacterManager.Load(doc);
            WeatherManager.Load(doc);
            EnvironmentManager.Load(doc);
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
            worldStateValues.CreateChild("Seed", Seed);
            worldStateValues.CreateChild("DaysSpentHere", DaysSpentHere);
            worldStateValues.CreateChild("CurrentLevel", _currentLevel);
            worldStateValues.CreateChild("Days", Days);
            worldStateValues.CreateChild("Hours", Hours);
            worldStateValues.CreateChild("Minutes", Minutes);
            worldStateValues.CreateChild("MinutesPassed", MinutesPassed);
            worldStateValues.CreateChild("Difficulty", _difficulty);
            Inventory.Save(doc);
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

        public static void ResetWorld(bool clearSave = true, int currentLevel = 1, int difficulty = 0)
        {
            Inventory.Reset();
            CharacterManager.Reset(clearSave);
            DaysSpentHere = 0;
            _currentLevel = currentLevel;
            Days = 0;
            Hours = 6;
            Minutes = 0;
            _difficulty = difficulty;
            _isNight = false;
            _isPaused = false;
            Seed = Random.Range(0, int.MaxValue);
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
            WorldView.Update(Hours);
            CharacterManager.Update();
            List<TutorialOverlay> overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(),
                new TutorialOverlay(WorldView.GetEnvironmentRect()),
                new TutorialOverlay()
            };
            TutorialManager.TryOpenTutorial(1, overlays);
        }

        public static void ActivateTemple()
        {
            ++_templesActivated;
            bool complete = _templesActivated == EnvironmentManager.CurrentEnvironment.Temples;
            if (complete) _gateActive = true;
        }

        public static bool AllTemplesActivate()
        {
            return _gateActive;
        }
        
        private void IncrementDaysSpentHere()
        {
            ++DaysSpentHere;
        }

        public static string TimeToHours(int duration)
        {
            int hours = Mathf.FloorToInt((float) duration / MinutesPerHour);
            int minutes = duration - hours * MinutesPerHour;
            string timeString = "";
            if (hours != 0)
            {
                if (hours == 1)
                {
                    timeString += hours + "hr";
                }
                else
                {
                    timeString += hours + "hrs ";
                }
            }

            if (minutes != 0)
            {
                timeString += minutes * IntervalSize + "mins";
            }

            return timeString;
        }

        public static int Difficulty() => _difficulty;

        public static int NormalisedDifficulty() => _difficulty / MaxDifficulty;

        public static int GetDaysSpentHere() => DaysSpentHere;

        public static int CurrentLevel() => _currentLevel;

        public static void TravelToNextEnvironment()
        {
            ++_currentLevel;
            _templesActivated = 0;
            _gateActive = false;
            DaysSpentHere = 0;
            EnvironmentManager.NextLevel(false, false);
            CharacterManager.Characters.ForEach(c => { c.TravelAction.ReturnToHomeInstant(); });
            StoryController.ShowText(JournalEntry.GetStoryText(_currentLevel), _currentLevel == 6);
        }

        public static void Pause()
        {
            _isPaused = true;
        }

        public static void UnPause()
        {
            _isPaused = false;
        }

        private void IncrementWorldTime()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime < MinuteInSeconds) return;
            _currentTime = _currentTime - MinuteInSeconds;
            IncrementMinutes();
            WorldView.Update(Hours);
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
            Campfire.Die();
            Inventory.UpdateBuildings();
            ++_timeAtLastSave;
            if (_timeAtLastSave % 12 == 0) WorldEventManager.SuggestSave();
            if (Hours == 12 || Hours == 24) ++_difficulty;
        }

        private void IncrementHours()
        {
            ++Hours;
            HourPasses();
            if (Hours == 24)
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
            IncrementWorldTime();
        }

        private static void UpdateScenery()
        {
            int minutesPassed = Hours * MinutesPerHour + Minutes / MinuteInterval;
            float timePassed = minutesPassed * MinuteInSeconds + _currentTime;
            float normalisedTime = timePassed / DayLengthInSeconds;
            SceneryController.SetTime(normalisedTime);
        }

        public static ItemQuality GenerateGearLevel()
        {
            int difficulty = Mathf.FloorToInt(Difficulty() / 10f);
            int difficultyMin = difficulty - 1;
            if (difficultyMin < 0) difficultyMin = 0;
            else if (difficultyMin > 4) difficultyMin = 4;
            int difficultyMax = difficulty + 1;
            if (difficultyMax > 4) difficultyMax = 4;
            return (ItemQuality) Random.Range(difficultyMin, difficultyMax);
        }

        private static void CheckEnemyUnlock()
        {
            int difficulty = Mathf.FloorToInt(Difficulty() / 5f) + 1;
            List<EnemyTemplate> enemyTypes = EnemyTemplate.GetEnemyTypes();
            enemyTypes.ForEach(e =>
            {
                if (e.Value == 0) return;
                if (e.Value > difficulty) return;
                switch (e.Species)
                {
                    case "Animal":
                        return;
                    case "Human":
                        if (_allowedHumanEnemies.Contains(e)) return;
                        _allowedHumanEnemies.Add(e);
                        return;
                    case "Nightmare":
                        if (_allowedNightmareEnemies.Contains(e)) return;
                        _allowedNightmareEnemies.Add(e);
                        return;
                }
            });
        }

        public static List<EnemyTemplate> GetAllowedHumanEnemyTypes()
        {
            CheckEnemyUnlock();
            return _allowedHumanEnemies;
        }

        public static List<EnemyTemplate> GetAllowedNightmareEnemyTypes()
        {
            CheckEnemyUnlock();
            return _allowedNightmareEnemies;
        }

        public static bool Paused()
        {
            return _isPaused;
        }

        public static int ScaleDamage(int damage)
        {
            return (int)(damage + damage * _difficulty / 50f);
        }
    }
}