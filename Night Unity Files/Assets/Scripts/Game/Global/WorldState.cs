using Facilitating;
using Facilitating.Persistence;
using Facilitating.UI;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using UnityEngine;

namespace Game.Global
{
    public class WorldState : MonoBehaviour
    {
        public const int MinutesPerHour = 12;
        private const int IntervalSize = 60 / MinutesPerHour;
        private static int DaysSpentHere;
        private static bool _started;
        private static CharacterManager _homeInventory;
        private static readonly RegionManager _regionManager = new RegionManager();

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        private static bool _isNight, _isPaused;

        public const float MinuteInSeconds = 0.2f;
        private const float DayLengthInSeconds = 24f * MinutesPerHour * MinuteInSeconds;
        private const int MinuteInterval = 60 / MinutesPerHour;

        public void Awake()
        {
            if(_homeInventory == null) _homeInventory = new CharacterManager();
            SaveController.AddPersistenceListener(_regionManager);
        }

        public void Start()
        {
            _homeInventory.Start();
            if (_started) return;
            _started = true;
            EnvironmentManager.Start();
            WeatherManager.Start();

//            SaveController.LoadSettings();
//todo            SaveController.LoadGame();

#if UNITY_EDITOR
            _homeInventory.AddTestingResources(3);
#endif
        }

        private void IncrementDaysSpentHere()
        {
            ++DaysSpentHere;
            if (DaysSpentHere == 7)
            {
                //TODO gameover
            }
        }

        public static string TimeToHours(int duration)
        {
            int hours = Mathf.FloorToInt((float) duration / MinutesPerHour);
            int minutes = duration - hours * MinutesPerHour;
            string timeString = "";
            if (hours != 0)
            {
                if (hours == 1) timeString += hours + "hr";
                else timeString += hours + "hrs ";
            }

            if (minutes != 0)
            {
                timeString += minutes * IntervalSize + "mins";
            }

            return timeString;
        }

        public static int Difficulty()
        {
            return DaysSpentHere + (Days - DaysSpentHere) * EnvironmentManager.CurrentEnvironment.LevelNo;
        }

        private void FindNewLocation()
        {
            EnvironmentManager.NextLevel();
        }

        public static CharacterManager HomeInventory()
        {
            return _homeInventory;
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
            int minutesPassed = Hours * MinutesPerHour + Minutes / MinuteInterval;
            float timePassed = minutesPassed * MinuteInSeconds + _currentTime;
            float normalisedTime = timePassed / DayLengthInSeconds;
            SceneryController.SetTime(normalisedTime);
            _currentTime += Time.deltaTime;
            if (_currentTime < MinuteInSeconds) return;
            _currentTime = _currentTime - MinuteInSeconds;
            IncrementMinutes();
            WorldView.SetTime(Days, Hours, Minutes);
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
            CharacterManager.Characters.ForEach(c =>
            {
                c.Update();
                c.Attributes.UpdateThirstAndHunger();
            });
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
                _isNight = false;
            else if ((Hours < 6 || Hours >= 20) && !_isNight)
                _isNight = true;
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
        }
    }
}