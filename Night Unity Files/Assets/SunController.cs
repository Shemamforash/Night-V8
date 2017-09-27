using System;
using Game.World;
using UnityEngine;

public class SunController : MonoBehaviour
{
    public ParticleSystem Sun, Stars;
    public int MinBrightness, MaxBrightness;
    private const float Value = 1f;
    private static float _weatherModifier = 1f;

    public static void SetWeatherModifier(float weatherModifier) => _weatherModifier = weatherModifier;
    
    public void Update()
    {
        if (WorldState.Hours >= 6 && WorldState.Hours <= 18)
        {
            ParticleSystem.EmissionModule starEmission = Stars.emission;
            starEmission.rateOverTime = 0;
            UpdateSun();
        }
        else
        {
            ParticleSystem.EmissionModule starEmission = Stars.emission;
            starEmission.rateOverTime = 50;
        }
    }

    private void UpdateStars()
    {
        ParticleSystem.EmissionModule starEmission = Stars.emission;
        starEmission.rateOverTime = 50;
    }

    private void UpdateSun()
    {
        float hours = WorldState.Hours;
        float minutes = (float) WorldState.Minutes / WorldState.MinutesPerHour;
        minutes /= 5f;
        hours += minutes;
        
        float timeOfDayModifier = (float) (-0.028f * Math.Pow(hours - 12, 2) + 1f);
        timeOfDayModifier *= _weatherModifier;
        
        ParticleSystem.MainModule mainModule = Sun.main;
        float minBrightness = MinBrightness * timeOfDayModifier / 255f;
        float maxBrightness = MaxBrightness * timeOfDayModifier / 255f;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(new Color(Value, Value, Value, maxBrightness), new Color(Value, Value, Value, minBrightness));
    }
}