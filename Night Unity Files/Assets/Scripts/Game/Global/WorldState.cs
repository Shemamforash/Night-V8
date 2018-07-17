using System;
using System.Collections.Generic;
using Facilitating;
using Facilitating.Persistence;
using Facilitating.UI;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Global
{
    public class WorldState : MonoBehaviour
    {
        public const int MinutesPerHour = 12;
        private const int IntervalSize = 60 / MinutesPerHour;

        public const float MinuteInSeconds = 0.2f;
        private const float DayLengthInSeconds = 24f * MinutesPerHour * MinuteInSeconds;
        private const int MinuteInterval = 60 / MinutesPerHour;
        private static int DaysSpentHere;
        private static bool _started;
        private static CharacterManager _homeInventory;
        private static readonly RegionManager _regionManager = new RegionManager();

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        private static int _difficulty;
        private static bool _isNight, _isPaused;
        public static int Seed = -1;

        public void Awake()
        {
            if (Seed == -1) Seed = Random.Range(0, int.MaxValue);
            Cursor.visible = false;
            if (_homeInventory == null) _homeInventory = new CharacterManager();
            SaveController.AddPersistenceListener(_regionManager);
        }

        public void Start()
        {
            _homeInventory.Start();
            if (_started) return;
//            foreach (ResourceTemplate template in ResourceTemplate.AllResources) _homeInventory.IncrementResource(template.Name, 1);
            _started = true;
            EnvironmentManager.Start();
            WeatherManager.Start();

//            SaveController.LoadSettings();
//todo            SaveController.LoadGame();

#if UNITY_EDITOR
//            _homeInventory.AddTestingResources(3, 3);
#endif
        }

        private void IncrementDaysSpentHere()
        {
            ++_difficulty;
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

        public static int GetDaysSpentHere() => DaysSpentHere;

        public static void TravelToNextEnvironment()
        {
            DaysSpentHere = 0;
            EnvironmentManager.NextLevel();
        }

        public static CharacterManager HomeInventory() => _homeInventory;

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
            WorldView.SetTime(Hours);
        }

        private void IncrementMinutes()
        {
            Minutes += MinuteInterval;
            MinutePasses();
            if (Minutes != 60) return;
            Minutes = 0;
            IncrementHours();
        }

        private void MinutePasses()
        {
            WeatherManager.CurrentWeather().Update();
            EnvironmentManager.UpdateTemperature();
            CharacterManager.Update();
            Campfire.Die();
        }

        private void HourPasses()
        {
            Campfire.Die();
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
            DayChangeSequence.Instance().ChangeDay();
        }

        public void Update()
        {
//            Debug.Log(EventSystem.current.currentSelectedGameObject.name);
            if (!_isPaused) IncrementWorldTime();
            UpdateScenery();
        }

        private void UpdateScenery()
        {
            int minutesPassed = Hours * MinutesPerHour + Minutes / MinuteInterval;
            float timePassed = minutesPassed * MinuteInSeconds + _currentTime;
            float normalisedTime = timePassed / DayLengthInSeconds;
            SceneryController.SetTime(normalisedTime);
        }

        public static int GenerateGearLevel()
        {
            int difficulty = Mathf.FloorToInt(Difficulty() / 10f);
            int difficultyMin = difficulty - 1;
            if (difficultyMin < 0) difficultyMin = 0;
            else if (difficultyMin > 4) difficultyMin = 4;
            int difficultyMax = difficulty + 1;
            if (difficultyMax > 4) difficultyMax = 4;
            return Random.Range(difficultyMin, difficultyMax);
        }

        public static List<EnemyType> GetAllowedHumanEnemyTypes()
        {
            int difficulty = Difficulty();
            List<EnemyType> allowedTypes = new List<EnemyType>();
            allowedTypes.Add(EnemyType.Sentinel);
            allowedTypes.Add(EnemyType.Brawler);

            if (difficulty >= 5)
            {
                allowedTypes.Add(EnemyType.Sniper);
                allowedTypes.Add(EnemyType.Martyr);
            }

            if (difficulty >= 10)
            {
                allowedTypes.Add(EnemyType.Witch);
                allowedTypes.Add(EnemyType.Medic);
            }

            if (difficulty >= 15)
            {
                allowedTypes.Add(EnemyType.Warlord);
                allowedTypes.Add(EnemyType.Mountain);
            }

            return allowedTypes;
        }
    }
}