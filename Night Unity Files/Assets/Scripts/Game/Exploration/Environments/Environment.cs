using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class Environment
    {
        private readonly float minTemp, maxTemp;
        private readonly List<float> _temperatureArray = new List<float>();
        public readonly int LevelNo, Temples, Monuments, Shrines, Fountains, Shelters, Animals, Dangers;
        public readonly EnvironmentType EnvironmentType;
        private static List<EnvironmentType> _environmentTypes;
        private readonly List<string> _environmentNames = new List<string>();
        public readonly int WaterSources;
        public readonly int FoodSources;
        public readonly int ResourceSources;

        public Environment(XmlNode environmentNode)
        {
            EnvironmentType = StringToEnvironmentType(environmentNode.Name);
            LevelNo = environmentNode.IntFromNode("Level");
            int temperature = environmentNode.IntFromNode("Temperature");
            maxTemp = temperature * 10;
            minTemp = maxTemp - 20;
            CalculateTemperatures();
            Temples = environmentNode.IntFromNode("Temples");
            Monuments = environmentNode.IntFromNode("Monuments");
            Shrines = environmentNode.IntFromNode("Shrines");
            Fountains = environmentNode.IntFromNode("Fountains");
            Shelters = environmentNode.IntFromNode("Shelters");
            Animals = environmentNode.IntFromNode("Animals");
            Dangers = environmentNode.IntFromNode("Danger");
            WaterSources = environmentNode.IntFromNode("WaterSources");
            FoodSources = environmentNode.IntFromNode("FoodSources");
            ResourceSources = environmentNode.IntFromNode("ResourceSources");
            RegionCount = Temples + Monuments + Shrines + Fountains + Shelters + Animals + Dangers;
            LoadEnvironmentNames();
        }

        private void LoadEnvironmentNames()
        {
            XmlNode root = Helper.OpenRootNode("Regions", "EnvironmentSuffixes");
            string[] environmentNames = root.StringFromNode(EnvironmentType.ToString()).Split(',');
            foreach (string name in environmentNames)
            {
                _environmentNames.Add(name);
            }
        }

        public readonly int RegionCount;

        private static EnvironmentType StringToEnvironmentType(string environmentString)
        {
            if (_environmentTypes == null)
            {
                _environmentTypes = new List<EnvironmentType>();
                foreach (EnvironmentType environmentType in Enum.GetValues(typeof(EnvironmentType))) _environmentTypes.Add(environmentType);
            }

            return _environmentTypes.FirstOrDefault(e => e.ToString() == environmentString);
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

        public List<string> Suffixes()
        {
            return _environmentNames;
        }
    }
}