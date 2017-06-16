namespace World
{
    using System.Collections.Generic;
    using UnityEngine;
    public class Environment
    {
        // private List<DangerListener> dangers;
        public string EnvironmentName;
        private float waterAbundance, foodAbundance, fuelAbundance, scrapAbundance;
        private float normalisedDanger;
        private int accessibility;
        private float defaultMinTemperature, defaultMaxTemperature;
        private List<float> temperatureArray = new List<float>();

        public Environment(string EnvironmentName, float waterAbundance, float foodAbundance, float fuelAbundance, float scrapAbundance,
            float normalisedDanger, int accessibility, float defaultMinTemperature, float defaultMaxTemperature)
        {
            this.EnvironmentName = EnvironmentName;
            this.waterAbundance = waterAbundance;
            this.foodAbundance = foodAbundance;
            this.fuelAbundance = fuelAbundance;
            this.scrapAbundance = scrapAbundance;
            this.normalisedDanger = normalisedDanger;
            this.accessibility = accessibility;
            this.defaultMinTemperature = defaultMinTemperature;
            this.defaultMaxTemperature = defaultMaxTemperature;
            CalculateTemperatures();
        }

        private void CalculateTemperatures(){
            float temperatureVariation = defaultMaxTemperature - defaultMinTemperature;
            temperatureVariation = 20f;
            for(int i = 0; i < 24 * 12; ++i){
                float normalisedTime = (float)i / (24f * 12f);
                float tempAtTime = Mathf.Pow(normalisedTime, 3);
                tempAtTime -= 2f * Mathf.Pow(normalisedTime, 2);
                tempAtTime += normalisedTime;
                tempAtTime *= 6.5f;
                tempAtTime *= temperatureVariation;
                tempAtTime += defaultMinTemperature;
                temperatureArray.Add(tempAtTime);
                // temperature equation = 6.5(x^3-2x^2+x)
            }
        }

        public int GetTemperature(int hours, int minutes){
            hours -= 6;
            if(hours < 0){
                hours = 24 + hours;
            }
            int arrayPosition = hours * 12 + (minutes / 5) - 1;
            return (int)temperatureArray[arrayPosition];
        }
    }
}