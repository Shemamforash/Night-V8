using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using Sirenix.Utilities;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class Environment
    {
        private static List<EnvironmentType> _environmentTypes;
        private readonly float minTemp, maxTemp;
        private readonly List<float> _temperatureArray = new List<float>();
        private readonly int Monuments, Shrines, Fountains, Shelters, Animals, Dangers, RegionCount;
        public readonly EnvironmentType EnvironmentType;
        public readonly int ResourceSources, FoodSources, WaterSources, Temples;
        private readonly Dictionary<RegionType, List<string>> _environmentRegionNames = new Dictionary<RegionType, List<string>>();

        public Environment(XmlNode environmentNode)
        {
            EnvironmentType = StringToEnvironmentType(environmentNode.Name);
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
            XmlNode root = Helper.OpenRootNode("Regions", "Names");
            RegionType[] regionTypes = {RegionType.Danger, RegionType.Animal, RegionType.Temple, RegionType.Shelter, RegionType.Shrine, RegionType.Monument, RegionType.Fountain, RegionType.Cache};
            regionTypes.ForEach(r =>
            {
                XmlNode regionNode = root.GetNode(r.ToString());
                string nameString = regionNode.StringFromNode(EnvironmentType.ToString());
                List<string> names = new List<string>();
                if (nameString != "") names = nameString.Split(',').ToList();
                _environmentRegionNames.Add(r, names);
            });
        }

        public static EnvironmentType StringToEnvironmentType(string environmentString)
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

        public static string EnvironmentTypeToName(string environmentName)
        {
            return EnvironmentTypeToName(StringToEnvironmentType(environmentName));
        }

        public static string EnvironmentTypeToName(EnvironmentType environmentType)
        {
            switch (environmentType)
            {
                case EnvironmentType.Desert:
                    return "The Desert of Whispers";
                case EnvironmentType.Mountains:
                    return "The Shattered Peaks";
                case EnvironmentType.Sea:
                    return "The Sea of Salt";
                case EnvironmentType.Ruins:
                    return "The Ruined City";
                case EnvironmentType.Wasteland:
                    return "The Eternal Wasteland";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetTemperature()
        {
            int hours = WorldState.Hours;
            int minutes = WorldState.Minutes;
            hours -= 6;
            if (hours < 0) hours = 24 + hours;
            int arrayPosition = hours * 12 + minutes / 5;
            return (int) _temperatureArray[arrayPosition];
        }

        public string GetRegionName(RegionType regionType)
        {
            if (regionType == RegionType.Temple)
            {
                Debug.Log(_environmentRegionNames[regionType].Count);
                _environmentRegionNames[regionType].Print();
            }

            return _environmentRegionNames[regionType].Count == 0 ? null : _environmentRegionNames[regionType].RemoveRandom();
        }
    }
}