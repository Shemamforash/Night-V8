using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using SamsHelper;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Character = Game.Characters.Character;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class RegionManager : Menu
    {
        private static readonly List<Region> UnexploredRegions = new List<Region>();
        private static readonly List<Region> DiscoveredRegions = new List<Region>();
        private static readonly Dictionary<string, RegionTemplate> Templates = new Dictionary<string, RegionTemplate>();
        private static readonly int NoRegionsToGenerate = 10;
        private static GameObject _regionContainer, _regionPrefab, _backButton, _exploreButton;
        private static TextMeshProUGUI _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Character _character;
        private static RegionManager _instance;
        
        public static RegionManager Instance()
        {
            return _instance ?? FindObjectOfType<RegionManager>();
        }
        
        protected void Awake()
        {
            _instance = this;
            _backButton = Helper.FindChildWithName(gameObject, "Back");
            _backButton.GetComponent<Button>().onClick.AddListener(delegate { ExitManager(false); });
            _regionContainer = Helper.FindChildWithName(gameObject, "Content");
            _regionInfoNameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            _regionPrefab = Resources.Load("Prefabs/Region") as GameObject;
            LoadRegionTemplates();
        }

        private void LoadRegionTemplates()
        {
            Helper.ConstructObjectsFromCsv("RegionData", delegate(string[] attributes)
            {
                RegionTemplate newTemplate = new RegionTemplate
                {
                    InternalName = attributes[0],
                    DisplayName = attributes[1],
                    Type = attributes[2],
                    WaterAvailable = int.Parse(attributes[3]),
                    FoodAvailable = int.Parse(attributes[4]),
                    FuelAvailable = int.Parse(attributes[5]),
                    ScrapAvailable = int.Parse(attributes[6]),
                    AmmoAvailable = int.Parse(attributes[7]),
                    Encounters = attributes[8],
                    Items = attributes[9]
                };
                Templates[newTemplate.InternalName] = newTemplate;
            });
        }

        public void OnEnable()
        {
            RefreshExploreButton();
            _exploreButton.GetComponent<Button>().Select();
        }
        
        public static void GenerateNewRegions()
        {
            UnexploredRegions.ForEach(r => r.DestroyGameObject());
            DiscoveredRegions.ForEach(r => r.DestroyGameObject());
            UnexploredRegions.Clear();
            DiscoveredRegions.Clear();
            for (int i = 0; i < NoRegionsToGenerate; ++i)
            {
                GameObject newRegionObject = AddNewButton();
                RegionTemplate template = Templates[Templates.Keys.ToList()[Random.Range(0, Templates.Keys.Count)]];
                string regionName = template.DisplayName == "" ? template.InternalName : template.DisplayName;
                Region region = new Region(regionName, template, newRegionObject);
                newRegionObject.GetComponent<Button>().onClick.AddListener(delegate { UpdateRegionInfo(region); });
                newRegionObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = region.Name();
                UnexploredRegions.Add(region);
                newRegionObject.SetActive(false);
            }
            RefreshExploreButton();
        }

        private static void DiscoverRandomRegion(int distance)
        {
            foreach (Region region in UnexploredRegions)
            {
                if (region.Distance() > distance) continue;
                DiscoverRegion(region);
                break;
            }
        }

        private static void DiscoverRegion(Region region)
        {
            DiscoveredRegions.Add(region);
            UnexploredRegions.Remove(region);
            region.GetObject().SetActive(true);
            for (int i = 0; i < DiscoveredRegions.Count; ++i)
            {
                if (i <= 0) continue;
                Helper.SetReciprocalNavigation(DiscoveredRegions[i].GetObject(), DiscoveredRegions[i - 1].GetObject());
            }
            RefreshExploreButton();
        }

        private static void RefreshExploreButton()
        {
            Destroy(_exploreButton);
            _exploreButton = AddNewButton();
            _exploreButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                Region targetRegion = UnexploredRegions[Random.Range(0, UnexploredRegions.Count)];
                StartExploration(delegate { DiscoverRegion(targetRegion); }, targetRegion);
            });
            _exploreButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Explore...";
            if (DiscoveredRegions.Count != 0)
            {
                GameObject lastRegionInList = DiscoveredRegions[DiscoveredRegions.Count - 1].GetObject();
                Helper.SetNavigation(lastRegionInList, _exploreButton, Helper.NavigationDirections.Down);
                Helper.SetNavigation(_exploreButton, lastRegionInList, Helper.NavigationDirections.Up);
            }
            Helper.SetNavigation(_exploreButton, _backButton, Helper.NavigationDirections.Down);
            Helper.SetNavigation(_backButton, _exploreButton, Helper.NavigationDirections.Up);
        }

        private static GameObject AddNewButton()
        {
            return Helper.InstantiateUiObject(_regionPrefab, _regionContainer.transform);
        }

        private static void UpdateRegionInfo(Region region)
        {
            _regionInfoNameText.text = region.Name();
            _regionInfoTypeText.text = region.Type();
            _regionInfoDescriptionText.text = region.Description();
        }

        public static void EnterManager(Character character)
        {
            _character = character;
            MenuStateMachine.Instance().NavigateToState("Region Menu");
        }

        public static void StartExploration(Action a, Region target)
        {
            Travel state = (Travel) _character.NavigateToState("Travel");
            state.AddOnExit(a);
            state.SetTargetRegion(target);
            ExitManager(true);
        }

        public static void ExitManager(bool characterIsExploring)
        {
            if (!characterIsExploring)
            {
                _character.ReturnToDefault();
            }
            _character = null;
            MenuStateMachine.Instance().GoToInitialMenu();
        }
    }
}