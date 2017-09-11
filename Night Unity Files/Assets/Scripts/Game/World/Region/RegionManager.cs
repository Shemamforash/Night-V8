using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Game.Characters.CharacterActions;
using SamsHelper;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;
using Character = Game.Characters.Character;
using Random = UnityEngine.Random;

namespace Game.World
{
    public class RegionManager : MonoBehaviour
    {
        private static List<Region.Region> _unexploredRegions = new List<Region.Region>();
        private static List<Region.Region> _discoveredRegions = new List<Region.Region>();
        private static Dictionary<string, RegionTemplate> _templates = new Dictionary<string, RegionTemplate>();
        private static int _noRegionsToGenerate = 10;
        private static GameObject _regionContainer, _regionPrefab, _backButton, _exploreButton;
        private static Text _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Character _character;

        public void Awake()
        {
            _backButton = Helper.FindChildWithName(gameObject, "Back");
            _backButton.GetComponent<Button>().onClick.AddListener(delegate { ExitManager(false); });
            _regionContainer = Helper.FindChildWithName(gameObject, "Content");
            _regionInfoNameText = Helper.FindChildWithName<Text>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<Text>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<Text>(gameObject, "Description");
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
                _templates[newTemplate.InternalName] = newTemplate;
            });
        }

        public void OnEnable()
        {
            RefreshExploreButton();
            _exploreButton.GetComponent<Button>().Select();
        }
        
        public static void GenerateNewRegions()
        {
            _unexploredRegions.ForEach(r => r.DestroyGameObject());
            _discoveredRegions.ForEach(r => r.DestroyGameObject());
            _unexploredRegions.Clear();
            _discoveredRegions.Clear();
            for (int i = 0; i < _noRegionsToGenerate; ++i)
            {
                GameObject newRegionObject = AddNewButton();
                RegionTemplate template = _templates[_templates.Keys.ToList()[Random.Range(0, _templates.Keys.Count)]];
                Region.Region region = new Region.Region(template, newRegionObject);
                newRegionObject.GetComponent<Button>().onClick.AddListener(delegate { UpdateRegionInfo(region); });
                newRegionObject.transform.Find("Text").GetComponent<Text>().text = region.Name();
                _unexploredRegions.Add(region);
                newRegionObject.SetActive(false);
            }
            RefreshExploreButton();
        }

        private static void DiscoverRandomRegion(int distance)
        {
            foreach (Region.Region region in _unexploredRegions)
            {
                if (region.Distance() <= distance)
                {
                    DiscoverRegion(region);
                    break;
                }
            }
        }

        private static void DiscoverRegion(Region.Region region)
        {
            _discoveredRegions.Add(region);
            _unexploredRegions.Remove(region);
            region.GetObject().SetActive(true);
            for (int i = 0; i < _discoveredRegions.Count; ++i)
            {
                if (i > 0)
                {
                    Helper.SetNavigation(_discoveredRegions[i].GetObject(), _discoveredRegions[i - 1].GetObject(),
                        Helper.NavigationDirections.Up);
                    Helper.SetNavigation(_discoveredRegions[i - 1].GetObject(), _discoveredRegions[i].GetObject(),
                        Helper.NavigationDirections.Down);
                }
            }
            RefreshExploreButton();
        }

        private static void RefreshExploreButton()
        {
            Destroy(_exploreButton);
            _exploreButton = AddNewButton();
            _exploreButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                Region.Region targetRegion = _unexploredRegions[Random.Range(0, _unexploredRegions.Count)];
                StartExploration(delegate { DiscoverRegion(targetRegion); }, targetRegion);
            });
            _exploreButton.transform.Find("Text").GetComponent<Text>().text = "Explore...";
            if (_discoveredRegions.Count != 0)
            {
                GameObject lastRegionInList = _discoveredRegions[_discoveredRegions.Count - 1].GetObject();
                Helper.SetNavigation(lastRegionInList, _exploreButton, Helper.NavigationDirections.Down);
                Helper.SetNavigation(_exploreButton, lastRegionInList, Helper.NavigationDirections.Up);
            }
            Helper.SetNavigation(_exploreButton, _backButton, Helper.NavigationDirections.Down);
            Helper.SetNavigation(_backButton, _exploreButton, Helper.NavigationDirections.Up);
        }

        private static GameObject AddNewButton()
        {
            GameObject newRegionObject = Instantiate(_regionPrefab);
            newRegionObject.transform.SetParent(_regionContainer.transform);
            newRegionObject.transform.localScale = new Vector3(1, 1, 1);
            return newRegionObject;
        }

        private static void UpdateRegionInfo(Region.Region region)
        {
            _regionInfoNameText.text = region.Name();
            _regionInfoTypeText.text = region.Type();
            _regionInfoDescriptionText.text = region.Description();
        }

        public static void EnterManager(Character character)
        {
            _character = character;
            MenuStateMachine.Instance.NavigateToState("Region Menu");
        }

        public static void StartExploration(Action a, Region.Region target)
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
            MenuStateMachine.Instance.GoToInitialMenu();
        }
    }
}