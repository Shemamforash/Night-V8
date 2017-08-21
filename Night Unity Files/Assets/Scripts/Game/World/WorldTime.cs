using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Game.World
{
    public class WorldTime : MonoBehaviour
    {
        private static readonly List<TimeListener> TimeListeners = new List<TimeListener>();
        private static float _currentTime, _quarterHourTimer = .2f;
        public static int Days, Hours = 6, Minutes;
        public const int MinutesPerHour = 12;
        private static bool _isNight;
        private static bool _isPaused;
        private Text _timeText, _dayText;

        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").transform.Find("Text").GetComponent<Text>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").transform.Find("Text").GetComponent<Text>();
        }
        
        public static void SubscribeTimeListener(TimeListener t)
        {
            TimeListeners.Add(t);
        }

        private static void BroadcastPause()
        {
            foreach (TimeListener t in TimeListeners)
            {
                t.ReceivePauseEvent(_isPaused);
            }
        }

        private static void BroadcastHourChange()
        {
            foreach (TimeListener t in TimeListeners)
            {
                t.ReceiveHourEvent();
            }
        }

        private static void BroadcastMinuteChange(){
            foreach(TimeListener t in TimeListeners){
                t.ReceiveMinuteEvent();
            }
        }

        private static void BroadcastDayChange()
        {
            foreach (TimeListener t in TimeListeners)
            {
                t.ReceiveDayEvent();
            }
        }

        public void BroadcastTravel(){
            foreach(TimeListener t in TimeListeners){
                t.ReceiveTravelEvent();
            }
        }

        public static void Pause()
        {
            _isPaused = true;
            BroadcastPause();
        }

        public static void UnPause()
        {
            _isPaused = false;
            BroadcastPause();
        }

        private void IncrementWorldTime()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= _quarterHourTimer)
            {
                _currentTime -= _quarterHourTimer;
                Minutes += 60 / MinutesPerHour;
                BroadcastMinuteChange();
                if (Minutes == 60)
                {
                    Minutes = 0;
                    ++Hours;
                    BroadcastHourChange();
                    if (Hours == 24)
                    {
                        ++Days;
                        BroadcastDayChange();
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