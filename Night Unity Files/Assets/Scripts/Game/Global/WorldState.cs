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
        public const int IntervalSize = 60 / MinutesPerHour;
        public static int StormDistance;
        public static readonly int StormDistanceMax = 10;
        public static int DaysSpentHere;
        private static bool _started;
        private static CharacterManager _homeInventory;
        private static readonly RegionManager _regionManager = new RegionManager();

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        private static bool _isNight, _isPaused;
        private static WorldState _instance;

        public static float MinuteInSeconds = 0.2f;
        private float _dayLengthInSeconds = 24f * MinutesPerHour * MinuteInSeconds;
        private int _minuteInterval = 60 / MinutesPerHour;

        public void Awake()
        {
            if(_homeInventory == null) _homeInventory = new CharacterManager();
            SaveController.AddPersistenceListener(_regionManager);
            _instance = this;
        }

        public void Start()
        {
            _homeInventory.Start();
            if (_started) return;
            _started = true;
            EnvironmentManager.Start();
            WeatherManager.Start();
            StormDistance = StormDistanceMax;
            WorldView.SetStormDistance(StormDistance);

//            SaveController.LoadSettings();
//todo            SaveController.LoadGame();

#if UNITY_EDITOR
            _homeInventory.AddTestingResources(3);
#endif
        }

        private void UpdateStormDistance()
        {
            WorldView.SetStormDistance(StormDistance);
        }

        private void IncrementDaysSpentHere()
        {
            ++DaysSpentHere;
            --StormDistance;
            UpdateStormDistance();
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

        public float GetCurrentDanger()
        {
            if (StormDistance <= 30f) return StormDistance / 30f;
            return 1;
        }

        private void FindNewLocation()
        {
            if (StormDistance == 0)
            {
                InitiateFinalEncounter();
                return;
            }

            if (DaysSpentHere >= 4) StormDistance += 2;
            else if (DaysSpentHere >= 3)
            {
                ++StormDistance;
                if (StormDistance > StormDistanceMax) StormDistance = StormDistanceMax;
            }

            UpdateStormDistance();
            EnvironmentManager.NextLevel();
        }

        private void InitiateFinalEncounter()
        {
            //TODO
        }

        public static CharacterManager HomeInventory()
        {
            return _homeInventory;
        }

        public static WorldState Instance()
        {
            return _instance ?? (_instance = (WorldState) FindObjectOfType(typeof(WorldState)));
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
            int minutesPassed = Hours * MinutesPerHour + Minutes / _minuteInterval;
            float timePassed = minutesPassed * MinuteInSeconds + _currentTime;
            float normalisedTime = timePassed / _dayLengthInSeconds;
            SceneryController.SetTime(normalisedTime);
            _currentTime += Time.deltaTime;
            if (_currentTime < MinuteInSeconds) return;
            _currentTime = _currentTime - MinuteInSeconds;
            IncrementMinutes();
            WorldView.SetTime(Days, Hours, Minutes);
        }

        private void IncrementMinutes()
        {
            Minutes += _minuteInterval;
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
                c.UpdateCurrentState();
                c.Attributes.UpdateThirstAndHunger();
            });
        }

        private void HourPasses()
        {
            CharacterManager.Characters.ForEach(c => { c.Attributes.Fatigue(); });
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


//        public static void RegisterMinuteEvent(Action a)
//        {
//            if (Instance() != null) Instance().MinuteEvent += a;
//        }
//
//        public static void UnregisterMinuteEvent(Action a)
//        {
//            if (Instance() != null) Instance().MinuteEvent -= a;
//        }
//
//        public static void RegisterHourEvent(Action a)
//        {
//            if (Instance() != null) Instance().HourEvent += a;
//        }
//
//        public static void UnregisterHourEvent(Action a)
//        {
//            if (Instance() != null) Instance().HourEvent -= a;
//        }
    }
}