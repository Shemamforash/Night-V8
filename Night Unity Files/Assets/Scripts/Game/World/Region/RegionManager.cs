using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using UnityEngine;
using UnityEngine.UI;
using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class RegionManager : MonoBehaviour, IPersistenceTemplate
    {
        private static readonly Dictionary<string, RegionTemplate> Templates = new Dictionary<string, RegionTemplate>();
        private static TextMeshProUGUI _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Player _character;
        private static readonly List<Region> _regions = new List<Region>();
        private static bool _loaded;

        public static List<Region> GenerateRegions(int numberOfRegions)
        {
            LoadRegionTemplates();
            while (numberOfRegions > 0)
            {
                GenerateNewRegion();
                --numberOfRegions;
            }

            return _regions;
        }

        private static void GenerateNewRegion()
        {
            RegionTemplate template = Templates[Templates.Keys.ToList()[Random.Range(0, Templates.Keys.Count)]];
            string regionName = template.GenerateName();
            Region region = new Region(regionName, template);
            _regions.Add(region);
        }
        
        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            XmlNode regionNode = SaveController.CreateNodeAndAppend("Regions", doc);
            foreach (Region region in _regions)
            {
                region.Save(regionNode, saveType);
            }
            return regionNode;
        }

        private static void LoadNames(RegionTemplate template, string[] prefixes, string[] suffixes)
        {
            List<string> combinations = new List<string>();
            foreach (string prefix in prefixes)
            {
                foreach (string suffix in suffixes)
                {
                    if (prefix != suffix)
                    {
                        combinations.Add(prefix + "'s " + suffix);
                    }
                }
            }
            Helper.Shuffle(ref combinations);
            template.Names = combinations;
        }

        private static Player Character()
        {
            return _character;
        }

        protected void Awake()
        {
            SaveController.AddPersistenceListener(this);
            _regionInfoNameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            LoadRegionTemplates();
        }

        private static void AllocateTravelResources(Player currentCharacter, int duration)
        {
            int foodRequired = (int) (currentCharacter.Attributes.Hunger.Max / 12f * duration);
            int waterRequired = (int) (currentCharacter.Attributes.Thirst.Max / 12f * duration);
            currentCharacter.Inventory().IncrementResource(InventoryResourceType.Food, foodRequired);
            currentCharacter.Inventory().IncrementResource(InventoryResourceType.Water, waterRequired);
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
                foreach (XmlNode regionNode in regionTypeNode.SelectNodes("Region"))
                {
                    string name = regionNode.SelectSingleNode("Name").InnerText;
                    //TODO import type and tier separately
                    string type = regionNode.SelectSingleNode("Type").InnerText.Split(' ')[0];
                    int food = int.Parse(regionNode.SelectSingleNode("Food").InnerText);
                    int water = int.Parse(regionNode.SelectSingleNode("Water").InnerText);
                    int fuel = int.Parse(regionNode.SelectSingleNode("Fuel").InnerText);
                    int scrap = int.Parse(regionNode.SelectSingleNode("Scrap").InnerText);
                    int ammo = int.Parse(regionNode.SelectSingleNode("Ammo").InnerText);
                    RegionTemplate template = new RegionTemplate
                    {
                        DisplayName = name,
                        Type = StringToRegionType(type),
                        WaterAvailable = water,
                        FoodAvailable = food,
                        FuelAvailable = fuel,
                        ScrapAvailable = scrap,
                        AmmoAvailable = ammo,
                    };
                    LoadNames(template, prefixes, suffixes);
                    Templates[name] = template;
                }
            }

            _loaded = true;
        }

        private static RegionType StringToRegionType(string type)
        {
            foreach (RegionType regionType in Enum.GetValues(typeof(RegionType)))
            {
                if (regionType.ToString() == type)
                {
                    return regionType;
                }
            }
            throw new Exceptions.UnknownRegionTypeException(type);
        }

        public static void UpdateRegionInfo(Region region)
        {
            string nameText = "Undiscovered Region", typeText = "", descriptionText = "Explore to discover new regions";
            if (region != null)
            {
                nameText = region.Name;
                typeText = region.GetRegionType().ToString();
                descriptionText = region.Description();
            }
            _regionInfoNameText.text = nameText;
            _regionInfoTypeText.text = typeText;
            _regionInfoDescriptionText.text = descriptionText;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }
    }
}