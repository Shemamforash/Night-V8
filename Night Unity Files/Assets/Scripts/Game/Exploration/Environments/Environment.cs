using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Global;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class Environment
    {
        private readonly float minTemp, maxTemp;
        private readonly List<float> _temperatureArray = new List<float>();
        public readonly int LevelNo, Shelters, Temples, CompleteKeys, Resources, Dangers;
        public readonly EnvironmentType EnvironmentType;
        private static List<EnvironmentType> _environmentTypes;

        public Environment(XmlNode environmentNode)
        {
            EnvironmentType = StringToEnvironmentType(environmentNode.Name);
            LevelNo = int.Parse(environmentNode.SelectSingleNode("Level").InnerText);
            int temperature = int.Parse(environmentNode.SelectSingleNode("Temperature").InnerText);
            maxTemp = temperature * 10;
            minTemp = maxTemp - 20;
            CalculateTemperatures();
            Shelters = int.Parse(environmentNode.SelectSingleNode("Shelter").InnerText);
            Temples = int.Parse(environmentNode.SelectSingleNode("Temples").InnerText);
            CompleteKeys = int.Parse(environmentNode.SelectSingleNode("CompleteKeys").InnerText);
            Resources = int.Parse(environmentNode.SelectSingleNode("Resources").InnerText);
            Dangers = int.Parse(environmentNode.SelectSingleNode("Danger").InnerText);
        }

        private EnvironmentType StringToEnvironmentType(string environmentString)
        {
            if (_environmentTypes == null)
            {
                _environmentTypes = new List<EnvironmentType>();
                foreach(EnvironmentType environmentType in Enum.GetValues(typeof(EnvironmentType))) _environmentTypes.Add(environmentType);
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
    }
}