﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Characters;
using UnityEngine;
using UnityEngine.UI;
using Game.Characters.CharacterActions;
using SamsHelper;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class RegionManager : Menu
    {
        private static readonly List<Region> DiscoveredRegions = new List<Region>();
        private static readonly Dictionary<string, RegionTemplate> Templates = new Dictionary<string, RegionTemplate>();
        private static readonly int NoRegionsToGenerate = 40;
        private static Button _backButton;
        private static InventoryUi _exploreButton;
        private static TextMeshProUGUI _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Player _character;
        private static RegionManager _instance;
        private static MenuList _menuList;
        private static MapGenerator.Node MapNode;

        public static RegionManager Instance()
        {
            return _instance ?? FindObjectOfType<RegionManager>();
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

        private Player Character()
        {
            return _character;
        }

        protected void Awake()
        {
            _instance = this;
            _menuList = gameObject.AddComponent<MenuList>();
            _backButton = Helper.FindChildWithName<Button>(gameObject, "Back");
            _backButton.onClick.AddListener(delegate { ExitManager(false); });
            _regionInfoNameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            _exploreButton = new InventoryUi(null, _menuList.InventoryContent);
            _exploreButton.SetCentralTextCallback(() => "Explore...");
            Helper.SetReciprocalNavigation(_exploreButton.GetNavigationButton(), _backButton);
            _exploreButton.PrimaryButton.AddOnClick(delegate
            {
                Player currentCharacter = Character();
                ExitManager(true);
                InventoryTransferManager.Instance().ShowInventories(WorldState.HomeInventory(), currentCharacter.Inventory(), () =>
                {
                    MapNode.Discover(currentCharacter);
                });
            });
            _menuList.AddPlainButton(_exploreButton);
            LoadRegionTemplates();
        }

        private void LoadRegionTemplates()
        {
            string regionText = Resources.Load<TextAsset>("Regions").text;
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
                    string type = regionNode.SelectSingleNode("Type").InnerText;
                    int food = int.Parse(regionNode.SelectSingleNode("Food").InnerText);
                    int water = int.Parse(regionNode.SelectSingleNode("Water").InnerText);
                    int fuel = int.Parse(regionNode.SelectSingleNode("Fuel").InnerText);
                    int scrap = int.Parse(regionNode.SelectSingleNode("Scrap").InnerText);
                    int ammo = int.Parse(regionNode.SelectSingleNode("Ammo").InnerText);
                    RegionTemplate template = new RegionTemplate
                    {
                        DisplayName = name,
                        Type = type,
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
        }

        public void OnEnable()
        {
            RefreshExploreButton();
        }

        public static void GenerateNewRegions()
        {
            MapNode = MapGenerator.Generate();
            _menuList.Items.ForEach(i =>
            {
                if (i == _exploreButton) return;
                i.Destroy();
            });
            DiscoveredRegions.Clear();
            RefreshExploreButton();
        }

        public static Region GenerateNewRegion()
        {
            RegionTemplate template = Templates[Templates.Keys.ToList()[Random.Range(0, Templates.Keys.Count)]];
            string regionName = template.GenerateName();
            Region region = new Region(regionName, template);
            return region;
        }

        public static void DiscoverRegion(Region region)
        {
            _menuList.AddItem(region);
            DiscoveredRegions.Add(region);
            RefreshExploreButton();
        }

        private static void RefreshExploreButton()
        {
            _menuList.SendToLast(_exploreButton);
            _menuList.RefreshNavigation();
        }

        public static void UpdateRegionInfo(Region region)
        {
            string nameText = "Undiscovered Region", typeText = "", descriptionText = "Explore to discover new regions";
            if (region != null)
            {
                nameText = region.Name;
                typeText = region.RegionType();
                descriptionText = region.Description();
            }
            _regionInfoNameText.text = nameText;
            _regionInfoTypeText.text = typeText;
            _regionInfoDescriptionText.text = descriptionText;
        }

        public static void EnterManager(Player character)
        {
            _character = character;
            MenuStateMachine.States.NavigateToState("Region Menu");
        }

        private static void ExitManager(bool characterIsExploring)
        {
            if (!characterIsExploring)
            {
                _character.States.ReturnToDefault();
            }
            _character = null;
            MenuStateMachine.GoToInitialMenu();
        }

        public static List<Region> GetDiscoveredRegions()
        {
            return DiscoveredRegions;
        }
    }
}