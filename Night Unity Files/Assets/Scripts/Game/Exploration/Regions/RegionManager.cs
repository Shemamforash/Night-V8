using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Exploration.Regions
{
    public class RegionManager : IPersistenceTemplate
    {
        private static readonly List<Region> _regions = new List<Region>();
        private static bool _loaded;
        private static readonly Dictionary<RegionType, List<string>> _regionNames = new Dictionary<RegionType, List<string>>();
        private static readonly List<RegionType> _regionTypes = new List<RegionType>();
        private static int _noTemples;
        private static int _regionsBeforeTemple;
        private static int _regionsDiscovered;


        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            XmlNode regionNode = SaveController.CreateNodeAndAppend("Regions", doc);
            foreach (Region region in _regions) region.Save(regionNode, saveType);
            return regionNode;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public static List<Region> GenerateRegions()
        {
            LoadRegionTemplates();

            _regionsDiscovered = 0;
            _regions.Clear();
            _regionTypes.Clear();
            _regions.Add(new Region());
            _regions[0].Discover();
            Environment.Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            for (int i = 0; i < currentEnvironment.Shelters; ++i) _regionTypes.Add(RegionType.Shelter);
            for (int i = 0; i < currentEnvironment.Resources; ++i) _regionTypes.Add(RegionType.Resource);
            for (int i = 0; i < currentEnvironment.Dangers; ++i) _regionTypes.Add(RegionType.Danger);
            _regionTypes.Add(RegionType.Shelter);
            _noTemples = currentEnvironment.Temples;
            _regionsBeforeTemple = _regionTypes.Count / 2;

            int numberOfRegions = _regionTypes.Count + _noTemples;
            while (numberOfRegions > 0)
            {
                _regions.Add(new Region());
                --numberOfRegions;
            }

            return _regions;
        }

        public static void GetRegionType(Region region)
        {
            if (_regionsDiscovered == 0)
            {
                region.SetRegionType(RegionType.Gate);
                region.Name = "Gate";
            }
            else
            {
                if (_regionsDiscovered == _regionsBeforeTemple)
                {
                    for (int i = 0; i < _noTemples; ++i) _regionTypes.Add(RegionType.Temple);
                }

                int randomRegionIndex = Random.Range(0, _regionTypes.Count);
                RegionType randomRegionType = _regionTypes[randomRegionIndex];
                _regionTypes.RemoveAt(randomRegionIndex);
                region.SetRegionType(randomRegionType);
                region.Name = GenerateName(randomRegionType) + " " + _regionsDiscovered;
            }

            ++_regionsDiscovered;
        }

        private static string GenerateName(RegionType type)
        {
            int pos = Random.Range(0, _regionNames.Count);
            string chosenName = _regionNames[type][pos];
            _regionNames[type].RemoveAt(pos);
            return chosenName;
        }

        private static void LoadNames(RegionType type, string[] prefixes, string[] suffixes)
        {
            List<string> combinations = new List<string>();
            foreach (string prefix in prefixes)
            foreach (string suffix in suffixes)
                if (prefix != suffix)
                    combinations.Add(prefix + "'s " + suffix);
            Helper.Shuffle(ref combinations);
            _regionNames.Add(type, combinations);
        }

        private static void AllocateTravelResources(Player currentCharacter, int duration)
        {
            int foodRequired = (int) (currentCharacter.Attributes.Hunger.Max / 12f * duration);
            int waterRequired = (int) (currentCharacter.Attributes.Thirst.Max / 12f * duration);
            currentCharacter.Inventory().IncrementResource("Fruit", foodRequired);
            currentCharacter.Inventory().IncrementResource("Water", waterRequired);
        }

        private static void LoadRegionTemplates()
        {
            if (_loaded) return;
            string regionText = Resources.Load<TextAsset>("XML/Regions").text;
            XmlDocument regionXml = new XmlDocument();
            regionXml.LoadXml(regionText);
            XmlNode root = regionXml.SelectSingleNode("RegionType");
            foreach (XmlNode regionTypeNode in root.ChildNodes)
            {
                string[] prefixes = regionTypeNode.SelectSingleNode("Prefixes").InnerText.Split(',');
                string[] suffixes = regionTypeNode.SelectSingleNode("Suffixes").InnerText.Split(',');
                string regionName = regionTypeNode.Name;
                foreach (RegionType type in Enum.GetValues(typeof(RegionType)))
                {
                    if (regionName == type.ToString())
                    {
                        LoadNames(type, prefixes, suffixes);
                    }
                }
            }

            _loaded = true;
        }
    }
}