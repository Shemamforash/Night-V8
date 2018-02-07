using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Characters;
using UnityEngine;
using UnityEngine.UI;
using Game.Characters.CharacterActions;
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
    public class RegionManager : Menu, IPersistenceTemplate
    {
        private static readonly Dictionary<string, RegionTemplate> Templates = new Dictionary<string, RegionTemplate>();
        private static Button _backButton;
        private static InventoryUi _exploreButton;
        private static TextMeshProUGUI _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Player _character;
        private static MenuList _menuList;
        private static Region _initialRegion;
        private const int MaxGenerationDistance = 5;
        private static string _graphString;
        private const int TargetNodeNumber = 50;
        private static readonly List<Region> NonFullNodes = new List<Region>();
        private static readonly List<Region> _regionList = new List<Region>();

        private static Region Generate()
        {
            int totalNodes = 0;
            Region initialRegionNode = GenerateNewRegion(null, totalNodes);
            for(int i = 0; i < Random.Range(3, 5); ++i) NonFullNodes.Add(initialRegionNode);
            while (totalNodes < TargetNodeNumber)
            {
                Region originRegion = NonFullNodes[Random.Range(0, NonFullNodes.Count)];
                ++totalNodes;
                Region newRegion = GenerateNewRegion(originRegion, totalNodes);
                originRegion.AddConnection(newRegion);
                float maxConnections = newRegion.Distance > MaxGenerationDistance ? 0 : 4;
                for (int i = 0; i < maxConnections; ++i) NonFullNodes.Add(newRegion);
                NonFullNodes.Remove(originRegion);
                _graphString += GetNodeName(originRegion) + "->" + GetNodeName(newRegion) + "\n";
            }
//            Debug.Log(_graphString);
            return initialRegionNode;
        }

        private static string GetNodeName(Region n)
        {
            return "\"Id:" + n.RegionNumber + " PReq:" + n.PerceptionRequirement + "\"";
//                return "\"Id:" + n.NodeNumber + " Type:" + n.NodeType + " PReq:" + n.PerceptionRequirement+"\"";
        }
        
        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            XmlNode regionNode = SaveController.CreateNodeAndAppend("Regions", doc);
            foreach (Region region in _regionList)
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
            _menuList = gameObject.AddComponent<MenuList>();
            _backButton = Helper.FindChildWithName<Button>(gameObject, "Back");
            _backButton.onClick.AddListener(delegate { ExitManager(false); });
            _regionInfoNameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            _exploreButton = new InventoryUi(null, _menuList.InventoryContent);
            _exploreButton.SetCentralTextCallback(() => "Explore...");
            Helper.SetReciprocalNavigation(_exploreButton.PrimaryButton, _backButton);
            _exploreButton.PrimaryButton.AddOnClick(delegate
            {
                Player currentCharacter = Character();
                ExitManager(true);
                AllocateTravelResources(currentCharacter, 6);
                InventoryTransferManager.Instance().ShowInventories(WorldState.HomeInventory(), currentCharacter.Inventory(), () =>
                {
                    UIExploreMenuController.Instance().SetRegion(_initialRegion, currentCharacter);
                });
            });
            _menuList.AddPlainButton(_exploreButton);
            LoadRegionTemplates();
        }

        private static void AllocateTravelResources(Player currentCharacter, int duration)
        {
            int foodRequired = (int) (currentCharacter.Attributes.Hunger.Max / 12f * duration);
            int waterRequired = (int) (currentCharacter.Attributes.Thirst.Max / 12f * duration);
            currentCharacter.Inventory().IncrementResource(InventoryResourceType.Food, foodRequired);
            currentCharacter.Inventory().IncrementResource(InventoryResourceType.Water, waterRequired);
        }

        private void LoadRegionTemplates()
        {
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
        }

        private RegionType StringToRegionType(string type)
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

        public void OnEnable()
        {
            RefreshExploreButton();
        }

        public static void GenerateNewRegions()
        {
            _initialRegion = Generate();
            _menuList.Items.ForEach(i =>
            {
                if (i == _exploreButton) return;
                i.Destroy();
            });
            RefreshExploreButton();
        }

        private static Region GenerateNewRegion(Region origin, int regionNumber)
        {
            RegionTemplate template = Templates[Templates.Keys.ToList()[Random.Range(0, Templates.Keys.Count)]];
            string regionName = template.GenerateName();
            Region region = new Region(regionName, origin, regionNumber, template);
            _regionList.Add(region);
            return region;
        }

        public static void DiscoverRegion(Region region)
        {
            ViewParent regionUi = _menuList.AddItem(region);
            regionUi.PrimaryButton.AddOnClick(() =>
            {
                //Travel to particular region
                Player currentCharacter = Character();
                ExitManager(true);
                AllocateTravelResources(currentCharacter, region.Distance);
                InventoryTransferManager.Instance().ShowInventories(WorldState.HomeInventory(), currentCharacter.Inventory(), () =>
                {
                    currentCharacter.TravelAction.TravelTo(region);
                });
            });
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
                typeText = region.GetRegionType().ToString();
                descriptionText = region.Description();
            }
            _regionInfoNameText.text = nameText;
            _regionInfoTypeText.text = typeText;
            _regionInfoDescriptionText.text = descriptionText;
        }

        public static void EnterManager(Player character)
        {
            _character = character;
            MenuStateMachine.ShowMenu("Region Menu");
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
            return _regionList.FindAll(r => r.Discovered());
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }
    }
}