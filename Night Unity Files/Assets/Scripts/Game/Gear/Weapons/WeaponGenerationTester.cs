using System;
using System.Collections.Generic;
using System.IO;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
	public static class WeaponGenerationTester
	{
		private const  int                                      TestRuns          = 100;
		private static string                                   _testResultString = "";
		private static Dictionary<AttributeType, MinMaxAverage> _attributeStats;

		private static readonly List<AttributeType> DesiredStats = new List<AttributeType>
		{
			AttributeType.Damage,
			AttributeType.Accuracy,
			AttributeType.FireRate
		};

		private static void ResetAttributeStats()
		{
			_attributeStats = new Dictionary<AttributeType, MinMaxAverage>();
			DesiredStats.ForEach(stat => { _attributeStats.Add(stat, new MinMaxAverage(stat)); });
		}

		public static void Test()
		{
			foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
			{
				ResetAttributeStats();
				TestWeapon(type, ItemQuality.Dark);
				TestWeapon(type, ItemQuality.Radiant);
				_testResultString += "\n\n";
			}

			File.WriteAllText(Directory.GetCurrentDirectory() + "/weapontest.txt", _testResultString.Replace("\n", Environment.NewLine));
		}

		private static void TestWeapon(WeaponType type, ItemQuality quality)
		{
			float  averageDps = 0, minDps = 10000, maxDps = 0;
			Weapon maxWeapon  = null;
			Weapon minWeapon  = null;
			for (int i = 0; i < TestRuns; ++i)
			{
				Weapon w = WeaponGenerator.GenerateWeapon(quality, type);
				DesiredStats.ForEach(stat => _attributeStats[stat].AddValue(w));
				float dps = w.WeaponAttributes.DPS();
				averageDps += dps;
				if (dps < minDps)
				{
					minDps    = dps;
					minWeapon = w;
				}

				if (dps > maxDps)
				{
					maxDps    = dps;
					maxWeapon = w;
				}
			}

			averageDps /= TestRuns;
			string indent = "        ";
			_testResultString += "Type: " + type + "  ---  Quality: " + quality                                                     + "\n";
			_testResultString += indent + "DPS: " + averageDps.Round(1) + " (min: " + minDps.Round(1) + " /max: " + maxDps.Round(1) + " )\n";
			DesiredStats.ForEach(stat => _testResultString += indent + _attributeStats[stat].GetString() + "\n");
			_testResultString += "\n";
			_testResultString += maxWeapon.WeaponAttributes.GetPrintMessage() + "\n";
			_testResultString += minWeapon.WeaponAttributes.GetPrintMessage() + "\n";
		}

		private class MinMaxAverage
		{
			private readonly AttributeType _type;
			private          float         _average, _min = 10000, _max;
			private          int           _runs;

			public MinMaxAverage(AttributeType type) => _type = type;

			public void AddValue(Weapon weapon)
			{
				float value = weapon.WeaponAttributes.Val(_type);
				_average += value;
				_runs++;
				if (value < _min) _min = value;

				if (value > _max) _max = value;
			}

			public string GetString()
			{
				float averageString = (_average / _runs).Round(1);
				return _type + ": " + averageString + " (min: " + _min.Round(1) + " /max: " + _max.Round(1) + ")";
			}
		}
	}
}