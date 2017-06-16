
using System.Collections;
using UnityEngine;

public static class GenericTimer
{
    public delegate float InnerTimeMethod(float currentTime, float duration);
    public delegate void EndOfTimerMethod();

    public static IEnumerator StartTimer(float duration, EndOfTimerMethod EndOfTimerMethod)
    {
        return StartTimer(duration, EndOfTimerMethod, null);
    }

    public static IEnumerator StartTimer(float duration, EndOfTimerMethod EndOfTimerMethod, InnerTimeMethod DuringTimerMethod)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            if (DuringTimerMethod != null)
            {
                currentTime = DuringTimerMethod(currentTime, duration);
            }
            yield return null;
        }
        if (EndOfTimerMethod != null)
        {
            EndOfTimerMethod();
        }
    }
}