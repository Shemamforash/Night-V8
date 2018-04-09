using System;
using System.Collections.Generic;
using System.IO;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerationTester
    {
        private const int TestRuns = 1000;
        private static string _testResultString = "";
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
            foreach (ItemQuality quality in Enum.GetValues(typeof(ItemQuality)))
            {
                ResetAttributeStats();
                TestWeapon(type, 0, quality);
                TestWeapon(type, 10, quality);
                _testResultString += "\n\n";
            }

//            Debug.Log(_testResultString);
            File.WriteAllText(Directory.GetCurrentDirectory() + "/weapontest.txt", _testResultString.Replace("\n", Environment.NewLine));
        }

        private static void TestWeapon(WeaponType type, int durability, ItemQuality quality)
        {
            float averageDps = 0, minDps = 10000, maxDps = 0;
            Weapon maxWeapon = null;
            Weapon minWeapon = null;
            for (int i = 0; i < TestRuns; ++i)
            {
                Weapon w = WeaponGenerator.GenerateWeapon(quality, type, durability);
                DesiredStats.ForEach(stat => _attributeStats[stat].AddValue(w));
                float dps = w.WeaponAttributes.DPS();
                averageDps += dps;
                if (dps < minDps)
                {
                    minDps = dps;
                    minWeapon = w;
                }

                if (dps > maxDps)
                {
                    maxDps = dps;
                    maxWeapon = w;
                }
            }

            averageDps /= TestRuns;
            string indent = "        ";
            _testResultString += "Type: " + type + "  ---  Durability: " + durability + "  ---  Quality: " + quality + "\n";
            _testResultString += indent + "DPS: " + Helper.Round(averageDps, 1) + " (min: " + Helper.Round(minDps, 1) + " /max: " + Helper.Round(maxDps, 1) + " )\n";
            DesiredStats.ForEach(stat => _testResultString += indent + _attributeStats[stat].GetString() + "\n");
            _testResultString += "\n";
//            _testResultString += maxWeapon.WeaponAttributes.Print() + "\n";
//            _testResultString += minWeapon.WeaponAttributes.Print() + "\n";
        }

        private class MinMaxAverage
        {
            private readonly AttributeType _type;
            private float _average, _min = 10000, _max;
            private int _runs;

            public MinMaxAverage(AttributeType type)
            {
                _type = type;
            }

            public void AddValue(Weapon weapon)
            {
                float value = weapon.GetAttributeValue(_type);
                _average += value;
                _runs++;
                if (value < _min) _min = value;

                if (value > _max) _max = value;
            }

            public string GetString()
            {
                float averageString = Helper.Round(_average / _runs, 1);
                return _type + ": " + averageString + " (min: " + Helper.Round(_min, 1) + " /max: " + Helper.Round(_max, 1) + ")";
            }
        }
    }
}