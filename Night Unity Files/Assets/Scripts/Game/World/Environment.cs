using System.Collections.Generic;
using UnityEngine;

namespace World
{
	using System;

    public class Environment
    {
        // private List<DangerListener> dangers;
        public string EnvironmentName;
        // private List<WeightedLink> linkedEnvironments = new List<WeightedLink>();
        private float waterAbundance, foodAbundance, fuelAbundance, scrapAbundance;
        private float normalisedDanger;
        private int accessibility;
        private float defaultMinTemperature, defaultMaxTemperature;
		private Environment previousEnvironment, nextEnvironment;

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
        }

		public void SetLinkedEnvironments(Environment previousEnvironment, Environment nextEnvironment){
			this.previousEnvironment = previousEnvironment;
			this.nextEnvironment = nextEnvironment;
		}

        // public Environment GetNextEnvironment(Environment lastEnvironment)
        // {
        //     List<WeightedLink> validLinks = new List<WeightedLink>();
        //     if(previousEnvironment != null && previousEnvironment != lastEnvironment){
		// 		validLinks.Add(new WeightedLink(previousEnvironment));
		// 	}
        //     float rand = UnityEngine.Random.Range(0f, totalWeight);
        //     totalWeight = 0;
        //     foreach (WeightedLink link in validLinks)
        //     {
        //         totalWeight += link.weight;
        //         if (totalWeight >= rand)
        //         {
        //             return link.nextEnvironment;
        //         }
        //     };
        //     return null;
        // }
    }
}