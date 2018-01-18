﻿using System;
using System.Collections.Generic;
using System.IO;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerationTester
    {
        private static string _testResultString = "";
        private const int TestRuns = 1000;
        private static Dictionary<AttributeType, MinMaxAverage> _attributeStats;
        private static readonly List<AttributeType> _desiredStats = new List<AttributeType> {AttributeType.Damage, AttributeType.Accuracy, AttributeType.CriticalChance, AttributeType.FireRate};

        private class MinMaxAverage
        {
            private float _average, _min = 10000, _max;
            private int _runs;
            private readonly AttributeType _type;

            public MinMaxAverage(AttributeType type)
            {
                _type = type;
            }
            
            public void AddValue(Weapon weapon)
            {
                float value = weapon.GetAttributeValue(_type);
                _average += value;
                _runs++;
                if (value < _min)
                {
                    _min = value;
                }

                if (value > _max)
                {
                    _max = value;
                }
            }

            public string GetString()
            {
                float averageString = Helper.Round(_average / _runs, 1);
                return _type + ": " + averageString + " (min: " + Helper.Round(_min, 1) + " /max: " + Helper.Round(_max, 1) + ")";
            }
        }

        private static void ResetAttributeStats()
        {
            _attributeStats = new Dictionary<AttributeType, MinMaxAverage>();
            _desiredStats.ForEach(stat =>
            {
                _attributeStats.Add(stat, new MinMaxAverage(stat));
            });
        }
        
        public static void Test()
        {
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
//                for (int durability = 0; durability <= 20; durability += 4)
//                {
                    ResetAttributeStats();
//                    TestWeapon(type, durability);
//                    TestWeapon(type, 20);
//                }

                _testResultString += "\n\n";
            }
//            Debug.Log(_testResultString);
            //File.WriteAllText(Directory.GetCurrentDirectory() + "/weapontest.txt", _testResultString.Replace("\n", Environment.NewLine));
        }

        private static void TestWeapon(WeaponType type, int durability)
        {
            float averageDps = 0, minDps = 10000, maxDps = 0;
            Weapon maxWeapon = null;
            Weapon minWeapon = null;
            for (int i = 0; i < TestRuns; ++i)
            {
                Weapon w = WeaponGenerator.GenerateWeapon(type, false, durability);
                _desiredStats.ForEach(stat => _attributeStats[stat].AddValue(w));
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
            _testResultString += "Type: " + type + "  ---  Durability: " + durability + "\n";
            _testResultString += indent + "DPS: " + Helper.Round(averageDps, 1) + " (min: " + Helper.Round(minDps, 1) + " /max: " + Helper.Round(maxDps, 1) + " )\n"; 
            _desiredStats.ForEach(stat => _testResultString += indent + _attributeStats[stat].GetString() + "\n");
            _testResultString += "\n";
            _testResultString += maxWeapon.WeaponAttributes.Print() + "\n";
            _testResultString += minWeapon.WeaponAttributes.Print() + "\n";
        }
    }
}