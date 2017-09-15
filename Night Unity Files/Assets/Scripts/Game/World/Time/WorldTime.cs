﻿using System;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using TMPro;
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

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        public const int MinutesPerHour = 12;
        private static bool _isNight, _isPaused;
        private TextMeshProUGUI _timeText, _dayText;
        private static WorldTime _instance;
        public static float MinuteInSeconds = .2f;

        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<TextMeshProUGUI>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").GetComponent<TextMeshProUGUI>();
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
            if (_currentTime >= MinuteInSeconds)
            {
                _currentTime = 0;
                Minutes += 60 / MinutesPerHour;
                MinuteEvent?.Invoke();
                if (Minutes == 60)
                {
                    Minutes = 0;
                    ++Hours;
                    HourEvent?.Invoke();
                    if (Hours == 24)
                    {
                        ++Days;
                        DayEvent?.Invoke();
                        Hours = 0;
                    }
                    //TODO make me make sense
                    if (Hours >= 6 && Hours < 20 && _isNight)
                    {
                        _isNight = false;
                    }
                    else if ((Hours < 6 || Hours >= 20) && !_isNight)
                    {
                        _isNight = true;
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