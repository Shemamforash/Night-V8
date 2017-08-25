using Game.World;
using SamsHelper.BaseGameFunctionality;

namespace World
{
    using System.Collections.Generic;
    using UnityEngine;
    public class Environment : ProbabalisticState
    {
        private float _waterAbundance, _foodAbundance, _fuelAbundance, _scrapAbundance;
        private readonly float _defaultMinTemperature, _defaultMaxTemperature;
        private readonly List<float> _temperatureArray = new List<float>();
        private string _displayName;

        public Environment(string name, string displayName, EnvironmentManager environmentManager, float waterAbundance, float foodAbundance, float fuelAbundance, float scrapAbundance,
            float defaultMinTemperature, float defaultMaxTemperature) : base(name, environmentManager)
        {
            _displayName = displayName;
            _waterAbundance = waterAbundance;
            _foodAbundance = foodAbundance;
            _fuelAbundance = fuelAbundance;
            _scrapAbundance = scrapAbundance;
            _defaultMinTemperature = defaultMinTemperature;
            _defaultMaxTemperature = defaultMaxTemperature;
            CalculateTemperatures();
        }

        public string GetDisplayName()
        {
            if (_displayName != "")
            {
                return _displayName;
            }
            return Name();
        }
        
        private void CalculateTemperatures(){
            float temperatureVariation = _defaultMaxTemperature - _defaultMinTemperature;
            temperatureVariation = 20f;
            for(int i = 0; i < 24 * 12; ++i){
                float normalisedTime = (float)i / (24f * 12f);
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

        public void NextEnvironment(List<string> visitedEnvironments)
        {
            ParentMachine.NavigateToState(NextState(visitedEnvironments));
        }

        public int GetTemperature(int hours, int minutes){
            hours -= 6;
            if(hours < 0){
                hours = 24 + hours;
            }
            int arrayPosition = hours * 12 + (minutes / 5) - 1;
            return (int)_temperatureArray[arrayPosition];
        }
    }
}