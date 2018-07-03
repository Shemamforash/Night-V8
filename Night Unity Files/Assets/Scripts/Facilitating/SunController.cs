using System;
using Game.Global;
using UnityEngine;

namespace Facilitating
{
    public class SunController : MonoBehaviour
    {
        private const float Value = 1f;
        private static float _weatherModifier = 1f;

        [Range(0, 1)] public float MinBrightness, MaxBrightness;

        private readonly float radius = 10;
        public GameObject Sun, Stars;

        public static void SetWeatherModifier(float weatherModifier)
        {
            _weatherModifier = weatherModifier;
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
            float minBrightness = MinBrightness * timeOfDayModifier;
            float maxBrightness = MaxBrightness * timeOfDayModifier;
            ParticleSystem.MainModule sunMain = Sun.GetComponent<ParticleSystem>().main;
            sunMain.startColor = new ParticleSystem.MinMaxGradient(new Color(Value, Value, Value, maxBrightness), new Color(Value, Value, Value, minBrightness));
        }
    }
}