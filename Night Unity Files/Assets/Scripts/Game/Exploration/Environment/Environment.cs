using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace Game.World.Environment_and_Weather
{
    public class Environment : ProbabalisticState
    {
        private float _waterAbundance, _foodAbundance, _fuelAbundance, _scrapAbundance;
        private readonly float _defaultMinTemperature, _defaultMaxTemperature;
        private readonly List<float> _temperatureArray = new List<float>();
        private Climate _climate;

        private enum Climate
        {
            Dry,
            Normal,
            Wet
        }

        public Environment(StateMachine stateMachine, string name, int temperature, int climate, int waterAbundance, int foodAbundance, int fuelAbundance, int scrapAbundance) : base(stateMachine,
            name, StateSubtype.Environment)
        {
            _waterAbundance = waterAbundance;
            _foodAbundance = foodAbundance;
            _fuelAbundance = fuelAbundance;
            _scrapAbundance = scrapAbundance;
            if (climate == 0) _climate = Climate.Dry;
            else if (climate == 1) _climate = Climate.Normal;
            else _climate = Climate.Wet;
            _defaultMaxTemperature = temperature * 10;
            _defaultMinTemperature = temperature - 20;
            CalculateTemperatures();
        }

        private void CalculateTemperatures()
        {
            float temperatureVariation = _defaultMaxTemperature - _defaultMinTemperature;
            for (int i = 0; i < 24 * 12; ++i)
            {
                float normalisedTime = i / (24f * 12f);
                float tempAtTime = Mathf.Pow(normalisedTime, 3);
                tempAtTime -= 2f * Mathf.Pow(normalisedTime, 2);
                tempAtTime += normalisedTime;
                tempAtTime *= 6.5f;
                tempAtTime *= temperatureVariation;
                tempAtTime += _defaultMinTemperature;
                _temperatureArray.Add(tempAtTime);
                // temperature equation = 6.5(x^3-2x^2+x)
            }
        }

        public int GetTemperature()
        {
            int hours = WorldState.Hours;
            int minutes = WorldState.Minutes;
            hours -= 6;
            if (hours < 0)
            {
                hours = 24 + hours;
            }
            int arrayPosition = hours * 12 + minutes / 5 - 1;
            return (int) _temperatureArray[arrayPosition];
        }
    }
}