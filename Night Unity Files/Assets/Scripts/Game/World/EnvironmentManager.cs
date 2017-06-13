using System.Collections.Generic;
using UnityEngine;

namespace World
{
    using Articy.Unity;
    using Articy.Night;
    using UnityEngine.UI;
    using Menus;

    public class EnvironmentManager : MonoBehaviour
    {
        public ArticyRef articyEnvironments;
        public Text environmentText;
        private Environment[] environments;
        private Environment currentEnvironment = null;
        private TimeListener timeListener = new TimeListener();

		public EnvironmentManager(){
			WorldState.environmentManager = this;
		}

        void Start()
        {
            List<EnvironmentTemplate> templates = ArticyDatabase.GetAllOfType<EnvironmentTemplate>();
            environments = new Environment[templates.Count];
            foreach (EnvironmentTemplate environmentType in templates)
            {
                int arrayPosition = (int)environmentType.Template.Environment.NormalisedDanger;

                Environment e = new Environment(
                    environmentType.DisplayName,
                    environmentType.Template.Environment.WaterLevel,
                    environmentType.Template.Environment.FoodLevel,
                    environmentType.Template.Environment.FuelLevel,
                    environmentType.Template.Environment.ScrapLevel,
                    environmentType.Template.Environment.NormalisedDanger,
                    (int)environmentType.Template.Environment.TerrainAccessibility,
                    environmentType.Template.Environment.MinTemperature,
                    environmentType.Template.Environment.MaxTemperature
                    );
                environments[arrayPosition] = e;
            }
            for (int i = 0; i < environments.Length; ++i)
            {
                Environment previous = null, next = null;
                if (i - 1 > 0)
                {
                    previous = environments[i - 1];
                }
                if (i + 1 < environments.Length)
                {
                    next = environments[i + 1];
                }
                environments[i].SetLinkedEnvironments(previous, next);
            }
            timeListener.OnTravel(GenerateEnvironment);
            GenerateEnvironment();
        }

        private void GenerateEnvironment()
        {
            if (currentEnvironment == null)
            {
                currentEnvironment = environments[0];
            }
            else
            {
                Environment option1 = SelectEnvironmentChoice(null);
                Environment option2 = SelectEnvironmentChoice(option1);
                WorldState.menuNavigator.ShowDestinationChoices(option1, option2);
            }
        }

        public void SetCurrentEnvironment(Environment e)
        {
			currentEnvironment = e;
            environmentText.text = currentEnvironment.EnvironmentName;
        }

        private Environment SelectEnvironmentChoice(Environment disallowed)
        {
            int dangerIndex = (int)Mathf.Floor(WorldState.currentDanger);
            float[] pDistribution = new float[] { 0.15f, 0.2f, 0.3f, 0.2f, 0.15f };
            float rand = Random.Range(0f, 1f);
            float currentPVal = 0f;
			int pIndex = 0;
            for (int i = dangerIndex - 2; i <= dangerIndex + 2; ++i)
            {
                currentPVal += pDistribution[pIndex];
				++pIndex;
                int j = i;
                if (j < 0)
                {
                    j = 0;
                }
                else if (j >= environments.Length)
                {
                    j = environments.Length - 1;
                }
                if (rand <= currentPVal)
                {
                    if (environments[j] == disallowed)
                    {
                        if (j - 1 > 0)
                        {
                            return environments[j - 1];
                        }
                        else
                        {
                            return environments[j + 1];
                        }
                    }
                    return environments[j];
                }
            }
            return null;
        }

        class WeightedLink
        {
            public Environment nextEnvironment;
            public float weight = 0;

            public WeightedLink(Environment nextEnvironment, float weight)
            {
                this.nextEnvironment = nextEnvironment;
                this.weight = weight;
            }
        }
    }
}