using System.Collections.Generic;
using Game.Global;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class Environment
    {
        private readonly float minTemp, maxTemp;
        private readonly List<float> _temperatureArray = new List<float>();
        public readonly int LevelNo, Shelters, Temples, CompleteKeys, Resources, Dangers;
        public readonly string Name;

        public Environment(string name, int level, int temperature, int shelterCount, int templeCount, int completeKeyCount, int resourceCount, int dangerCount)
        {
            Name = name;
            maxTemp = temperature * 10;
            minTemp = maxTemp - 20;
            CalculateTemperatures();
            LevelNo = level;
            Shelters = shelterCount;
            Temples = templeCount;
            CompleteKeys = completeKeyCount;
            Resources = resourceCount;
            Dangers = dangerCount;
        }

        private void CalculateTemperatures()
        {
            float temperatureVariation = maxTemp - minTemp;
            for (int i = 0; i < 24 * 12; ++i)
            {
                float normalisedTime = i / (24f * 12f);
                float tempAtTime = Mathf.Pow(normalisedTime, 3);
                tempAtTime -= 2f * Mathf.Pow(normalisedTime, 2);
                tempAtTime += normalisedTime;
                tempAtTime *= 6.5f;
                tempAtTime *= temperatureVariation;
                tempAtTime += minTemp;
                _temperatureArray.Add(tempAtTime);
                // temperature equation = 6.5(x^3-2x^2+x)
            }
        }

        public int GetTemperature()
        {
            int hours = WorldState.Hours;
            int minutes = WorldState.Minutes;
            hours -= 6;
            if (hours < 0) hours = 24 + hours;
            int arrayPosition = hours * 12 + minutes / 5 - 1;
            return (int) _temperatureArray[arrayPosition];
        }
    }
}