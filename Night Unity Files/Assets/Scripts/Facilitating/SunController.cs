using System;
using Game.World;
using UnityEngine;

public class SunController : MonoBehaviour
{
    public GameObject Sun, Stars, Moon;
    public int MinBrightness, MaxBrightness;
    private const float Value = 1f;
    private static float _weatherModifier = 1f;
    private float radius = 10;
    private Vector2 origin;

    public static void SetWeatherModifier(float weatherModifier) => _weatherModifier = weatherModifier;

    public void Awake()
    {
        origin = new Vector2(0, -radius);
    }
    
    public void Update()
    {
        ParticleSystem.EmissionModule starEmission = Stars.GetComponent<ParticleSystem>().emission;
        if (WorldState.Hours >= 6 && WorldState.Hours <= 18)
        {
            starEmission.rateOverTime = 0;
            UpdateSun();
        }
        else
        {
            starEmission.rateOverTime = 50;
            UpdateMoon();
        }
    }

    private float GetTime()
    {
        float time = WorldState.Hours;
        float minutes = (float) WorldState.Minutes / WorldState.MinutesPerHour;
        minutes /= 5f;
        time += minutes;
        return time;
    }

    private void UpdateMoon()
    {
    }

    private void UpdateSun()
    {
        float time = GetTime();
        float timeOfDayModifier = (float) (-0.028f * Math.Pow(time - 12, 2) + 1f);
        timeOfDayModifier *= _weatherModifier;
        
        float minBrightness = MinBrightness * timeOfDayModifier / 255f;
        float maxBrightness = MaxBrightness * timeOfDayModifier / 255f;
        ParticleSystem.MainModule sunMain = Sun.GetComponent<ParticleSystem>().main;
        sunMain.startColor = new ParticleSystem.MinMaxGradient(new Color(Value, Value, Value, maxBrightness), new Color(Value, Value, Value, minBrightness));
    }
}