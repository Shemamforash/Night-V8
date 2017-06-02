using UnityEngine;
using System.Collections.Generic;

public class WorldTime : MonoBehaviour
{
    private static List<TimeListener> timeListeners = new List<TimeListener>();
    private static float currentTime, quarterHourTimer = 2.5f;
    private static int days = 0, hours = 6, minutes = 0;
    private static bool isNight = false;
    private static bool isPaused = false;

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

    private static void BroadcastDayChange()
    {
        foreach (TimeListener t in timeListeners)
        {
            t.ReceiveDayEvent();
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
            if (minutes == 60)
            {
                minutes = 0;
                ++hours;
                if (hours == 24)
                {
                    ++days;
                }
                if (hours >= 6 && hours < 20)
                {
                    isNight = false;
                }
                else
                {
                    isNight = true;
                }
            }
        }
    }

    void Update()
    {
        if (!isPaused)
        {
            IncrementWorldTime();
        }
    }
}
