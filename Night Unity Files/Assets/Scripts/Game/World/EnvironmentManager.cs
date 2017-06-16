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
        public Text environmentText, temperatureText;
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
            templates.ForEach(environmentType =>
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
            });
            timeListener.OnTravel(GenerateEnvironment);
            timeListener.OnMinute(UpdateTemperature);
            GenerateEnvironment();
        }

        private void UpdateTemperature(){
            temperatureText.text = currentEnvironment.GetTemperature(WorldTime.hours, WorldTime.minutes) + "\u00B0" + "C";
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
            for (int i = 0; i < pDistribution.Length; ++i)
            {
                currentPVal += pDistribution[i];
                int j = dangerIndex - 2 + i;
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
    }
}