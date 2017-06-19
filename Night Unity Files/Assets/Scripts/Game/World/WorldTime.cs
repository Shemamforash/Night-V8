using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace World
{
    public class WorldTime : MonoBehaviour
    {
        private static List<TimeListener> timeListeners = new List<TimeListener>();
        private static float currentTime, quarterHourTimer = .2f;
        public static int days = 0, hours = 6, minutes = 0;
        private static bool isNight = false;
        private static bool isPaused = true;
        public Text timeText, dayText;

        public static void SubscribeTimeListener(TimeListener t)
        {
            timeListeners.Add(t);
        }

        private static void BroadcastPause()
        {
            foreach (TimeListener t in timeListeners)
            {
                t.ReceivePauseEvent(isPaused);
            }
        }

        private static void BroadcastHourChange()
        {
            foreach (TimeListener t in timeListeners)
            {
                t.ReceiveHourEvent();
            }
        }

        private static void BroadcastMinuteChange(){
            foreach(TimeListener t in timeListeners){
                t.ReceiveMinuteEvent();
            }
        }

        private static void BroadcastDayChange()
        {
            foreach (TimeListener t in timeListeners)
            {
                t.ReceiveDayEvent();
            }
        }

        public void BroadcastTravel(){
            foreach(TimeListener t in timeListeners){
                t.ReceiveTravelEvent();
            }
        }

        public static void Pause()
        {
            isPaused = true;
            BroadcastPause();
        }

        public static void UnPause()
        {
            isPaused = false;
            BroadcastPause();
        }

        private void IncrementWorldTime()
        {
            currentTime += Time.deltaTime;
            if (currentTime >= quarterHourTimer)
            {
                currentTime -= quarterHourTimer;
                minutes += 5;
                BroadcastMinuteChange();
                if (minutes == 60)
                {
                    minutes = 0;
                    ++hours;
                    BroadcastHourChange();
                    if (hours == 24)
                    {
                        ++days;
                        BroadcastDayChange();
                        hours = 0;
                    }
                    //TODO make me make sense
                    if (hours >= 6 && hours < 20 && isNight)
                    {
                        isNight = false;
                        quarterHourTimer *= 2;
                    }
                    else if ((hours < 6 || hours >= 20) && !isNight)
                    {
                        isNight = true;
                        quarterHourTimer /= 2;
                    }
                }
            }
            if (minutes < 10)
            {
                timeText.text = hours + ":0" + minutes;
            }
            else
            {
                timeText.text = hours + ":" + minutes;
            }
            dayText.text = "Day " + days;
        }

        void Update()
        {
            if (!isPaused)
            {
                IncrementWorldTime();
            }
        }
    }
}