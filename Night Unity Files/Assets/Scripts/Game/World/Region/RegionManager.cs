using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class RegionManager : Menu
    {
        private static readonly List<Region> UnexploredRegions = new List<Region>();
        private static readonly List<InventoryUi> DiscoveredRegions = new List<InventoryUi>();
        private static readonly Dictionary<string, RegionTemplate> Templates = new Dictionary<string, RegionTemplate>();
        private static readonly int NoRegionsToGenerate = 40;
        private static GameObject _backButton;
        private static InventoryUi _exploreButton;
        private static TextMeshProUGUI _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;
        private static Character _character;
        private static RegionManager _instance;
        private static MenuList _menuList;
        private static readonly List<string> NamePool = new List<string>();

        public static RegionManager Instance()
        {
            return _instance ?? FindObjectOfType<RegionManager>();
        }

        private static void LoadNames()
        {
            List<string> lines = Helper.ReadLinesFromFile("RegionNames");
            List<string> prefixList = new List<string>();
            List<string> suffixList = new List<string>();
            for (int i = 0; i < 3; ++i)
            {
                prefixList.AddRange(Helper.SplitAndRemoveWhiteSpace(lines[i]));
                suffixList.AddRange(Helper.SplitAndRemoveWhiteSpace(lines[i + 3]));
            }
            List<string> combinations = new List<string>();
            foreach (string prefix in prefixList)
            {
                foreach (string suffix in suffixList)
                {
                    if (prefix != suffix)
                    {
                        combinations.Add(prefix +"'s " + suffix);
                    }
                }
            }
            Helper.Shuffle(ref combinations);
            for (int i = 0; i < 500; ++i)
            {
                NamePool.Add(combinations[i]);
            }
        }

        private static string GenerateName()
        {
            int pos = Random.Range(0, NamePool.Count);
            string chosenName = NamePool[pos];
            NamePool.RemoveAt(pos);
            return chosenName;
        }

        protected void Awake()
        {
            _instance = this;
            LoadNames();
            _menuList = gameObject.AddComponent<MenuList>();
            _backButton = Helper.FindChildWithName(gameObject, "Back");
            _backButton.GetComponent<Button>().onClick.AddListener(delegate { ExitManager(false); });
            _regionInfoNameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            _exploreButton = new InventoryUi(null, _menuList.ContentTransform());
            _exploreButton.SetCentralTextCallback(() => "Explore...");
            Helper.SetReciprocalNavigation(_exploreButton.GetNavigationButton(), _backButton);
            _exploreButton.OnPress(delegate
            {
                Region targetRegion = UnexploredRegions[Random.Range(0, UnexploredRegions.Count)];
                StartExploration(delegate { DiscoverRegion(targetRegion); }, targetRegion);
            });
            _menuList.AddPlainButton(_exploreButton);
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
        }

        public static void GenerateNewRegions()
        {
            DiscoveredRegions.ForEach(r => r.Destroy());
            UnexploredRegions.Clear();
            DiscoveredRegions.Clear();
            for (int i = 0; i < NoRegionsToGenerate; ++i)
            {
                RegionTemplate template = Templates[Templates.Keys.ToList()[Random.Range(0, Templates.Keys.Count)]];
                string regionName = GenerateName(); //template.DisplayName == "" ? template.InternalName : template.DisplayName;
                Region region = new Region(regionName, template);
                UnexploredRegions.Add(region);
#if UNITY_EDITOR
                DiscoverRegion(region);
#endif
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
            _menuList.AddItem(region);
            UnexploredRegions.Remove(region);
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

        public static void EnterManager(Character character)
        {
            _character = character;
            MenuStateMachine.States.NavigateToState("Region Menu");
        }

        private static void StartExploration(Action a, Region target)
        {
            Travel state = (Travel) _character.ActionStates.NavigateToState("Travel");
            state.AddOnExit(a);
            state.SetTargetRegion(target);
            ExitManager(true);
        }

        private static void ExitManager(bool characterIsExploring)
        {
            if (!characterIsExploring)
            {
                _character.ActionStates.ReturnToDefault();
            }
            _character = null;
            MenuStateMachine.GoToInitialMenu();
        }
    }
}