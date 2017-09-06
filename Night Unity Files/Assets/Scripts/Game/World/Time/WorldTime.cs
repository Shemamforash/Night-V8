using System;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.Time
{
    public class WorldTime : MonoBehaviour
    {
        public event Action MinuteEvent;
        public event Action HourEvent;
        public event Action DayEvent;
        public event Action TravelEvent;
        public event Action<bool> PauseEvent;
        
        private static float _currentTime, _quarterHourTimer = .2f;
        public static int Days, Hours = 6, Minutes;
        public const int MinutesPerHour = 12;
        private static bool _isNight, _isPaused;
        private Text _timeText, _dayText;
        private static WorldTime _instance;

        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<Text>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").GetComponent<Text>();
            _instance = this;
        }

        public static WorldTime Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(WorldTime)) as WorldTime;
            }
            return _instance;
        }
        
        public void Pause()
        {
            _isPaused = true;
            if (PauseEvent != null) PauseEvent(true);
        }

        public void UnPause()
        {
            _isPaused = false;
            if (PauseEvent != null) PauseEvent(false);
        }

        private void IncrementWorldTime()
        {
            _currentTime += UnityEngine.Time.deltaTime;
            if (_currentTime >= _quarterHourTimer)
            {
                _currentTime -= _quarterHourTimer;
                Minutes += 60 / MinutesPerHour;
                if (MinuteEvent != null) MinuteEvent();
                if (Minutes == 60)
                {
                    Minutes = 0;
                    ++Hours;
                    if (HourEvent != null) HourEvent();
                    if (Hours == 24)
                    {
                        ++Days;
                        if (DayEvent != null) DayEvent();
                        Hours = 0;
                    }
                    //TODO make me make sense
                    if (Hours >= 6 && Hours < 20 && _isNight)
                    {
                        _isNight = false;
                        _quarterHourTimer *= 2;
                    }
                    else if ((Hours < 6 || Hours >= 20) && !_isNight)
                    {
                        _isNight = true;
                        _quarterHourTimer /= 2;
                    }
                }
            }
            if (Minutes < 10)
            {
                _timeText.text = Hours + ":0" + Minutes;
            }
            else
            {
                _timeText.text = Hours + ":" + Minutes;
            }
            _dayText.text = "Day " + Days;
        }

        void Update()
        {
            if (!_isPaused)
            {
                IncrementWorldTime();
            }
            CooldownManager.UpdateCooldowns();
        }
    }
}