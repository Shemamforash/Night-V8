﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Exploration.Regions;
using Game.Global;
using Extensions;
using Extensions;
using UnityEngine;

namespace Game.Exploration.Environment
{
	public class Environment
	{
		private static   List<EnvironmentType>                _environmentTypes;
		private readonly Dictionary<RegionType, List<string>> _environmentRegionNames = new Dictionary<RegionType, List<string>>();
		private readonly List<float>                          _temperatureArray       = new List<float>();
		public readonly  EnvironmentType                      EnvironmentType;
		private readonly float                                minTemp,         maxTemp;
		public readonly  int                                  ResourceSources, FoodSources, WaterSources, Temples;

		public Environment(XmlNode environmentNode)
		{
			EnvironmentType = StringToEnvironmentType(environmentNode.Name);
			int temperature = environmentNode.ParseInt("Temperature");
			maxTemp = temperature * 10;
			minTemp = maxTemp - 20;
			CalculateTemperatures();
			Temples         = environmentNode.ParseInt("Temples");
			WaterSources    = environmentNode.ParseInt("WaterSources");
			FoodSources     = environmentNode.ParseInt("FoodSources");
			ResourceSources = environmentNode.ParseInt("ResourceSources");
			LoadEnvironmentNames();
		}

		private void LoadEnvironmentNames()
		{
			XmlNode      root        = Helper.OpenRootNode("Regions", "Names");
			RegionType[] regionTypes = {RegionType.Danger, RegionType.Animal, RegionType.Temple, RegionType.Shelter, RegionType.Shrine, RegionType.Monument, RegionType.Fountain, RegionType.Cache};
			Array.ForEach(regionTypes, r =>
			{
				XmlNode      regionNode     = root.GetChild(r.ToString());
				string       nameString     = regionNode.ParseString(EnvironmentType.ToString());
				List<string> names          = new List<string>();
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
				float tempAtTime     = Mathf.Pow(normalisedTime, 3);
				tempAtTime -= 2f * Mathf.Pow(normalisedTime, 2);
				tempAtTime += normalisedTime;
				tempAtTime *= 6.5f;
				tempAtTime *= temperatureVariation;
				tempAtTime += minTemp;
				_temperatureArray.Add(tempAtTime);
				// temperature equation = 6.5(x^3-2x^2+x)
			}
		}

		public static string EnvironmentTypeToName(string environmentName) => EnvironmentTypeToName(StringToEnvironmentType(environmentName));

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
					return "Dedicated to Yvonne Roberts";
			}
		}

		public int GetTemperature()
		{
			int hours   = WorldState.Hours;
			int minutes = WorldState.Minutes;
			hours -= 6;
			if (hours < 0) hours = 24 + hours;
			int arrayPosition    = hours * 12 + minutes / 5;
			return (int) _temperatureArray[arrayPosition];
		}

		public string GetRegionName(RegionType regionType) => _environmentRegionNames[regionType].Count == 0 ? null : _environmentRegionNames[regionType].RemoveRandom();

		public void RemoveExistingName(RegionType regionType, string name)
		{
			if (!_environmentRegionNames.ContainsKey(regionType)) return;
			_environmentRegionNames[regionType].Remove(name);
		}
	}
}