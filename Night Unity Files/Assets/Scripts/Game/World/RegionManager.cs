using System.Collections.Generic;
using System.Linq;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World
{
    public class RegionManager : MonoBehaviour
    {
        private static List<EnvironmentRegion> _regions = new List<EnvironmentRegion>();
        private static Dictionary<string, RegionTemplate> _templates = new Dictionary<string, RegionTemplate>();
        private static int _noRegionsToGenerate = 10;
        private static GameObject _regionContainer, _regionPrefab, _backButton, _exploreButton;
        private static Text _regionInfoNameText, _regionInfoTypeText, _regionInfoDescriptionText;

        public void Awake()
        {
            _backButton = Helper.FindChildWithName(gameObject, "Back");
            _backButton.GetComponent<Button>().onClick.AddListener(() => WorldState.MenuNavigator.SwitchToMenu("Game Menu", false));
            _regionContainer = Helper.FindChildWithName(gameObject, "Content");
            _regionInfoNameText = Helper.FindChildWithName<Text>(gameObject, "Name");
            _regionInfoTypeText = Helper.FindChildWithName<Text>(gameObject, "Type");
            _regionInfoDescriptionText = Helper.FindChildWithName<Text>(gameObject, "Description");
            _regionPrefab = Resources.Load("Prefabs/Region") as GameObject;
        }

        public void Start()
        {
            LoadRegionTemplates();
        }

        private void LoadRegionTemplates()
        {
            Helper.ConstructObjectsFromCsv("RegionData", delegate(string[] attributes)
            {
                RegionTemplate newTemplate = new RegionTemplate();
                newTemplate.InternalName = attributes[0];
                newTemplate.DisplayName = attributes[1];
                newTemplate.Type = attributes[2];
                newTemplate.WaterAvailable = float.Parse(attributes[3]);
                newTemplate.FoodAvailable = float.Parse(attributes[4]);
                newTemplate.FuelAvailable = float.Parse(attributes[5]);
                newTemplate.ScrapAvailable = float.Parse(attributes[6]);
                newTemplate.AmmoAvailable = float.Parse(attributes[7]);
                newTemplate.Encounters = attributes[8];
                newTemplate.Items = attributes[9];
                _templates[newTemplate.InternalName] = newTemplate;
            });
        }
        
        public static void GenerateNewRegions()
        {
            _regions.ForEach(r => r.DestroyGameObject());
            _regions.Clear();
            RefreshExploreButton();
        }

        public static void AddRegion()
        {
            int regionNo = _regions.Count - 1;
            if (_regions.Count < _noRegionsToGenerate)
            {
                GameObject newRegionObject = AddNewButton();
                RegionTemplate template = _templates[_templates.Keys.ToList()[Random.Range(0, _templates.Keys.Count)]];
                EnvironmentRegion region = new EnvironmentRegion(template, newRegionObject);
                newRegionObject.GetComponent<Button>().onClick.AddListener(delegate { UpdateRegionInfo(region); });
                newRegionObject.transform.Find("Text").GetComponent<Text>().text = region.Name();
                _regions.Add(region);
                if (regionNo >= 0)
                {
                    Helper.SetNavigation(newRegionObject, _regions[regionNo].GetObject(), Helper.NavigationDirections.Up);
                    Helper.SetNavigation(_regions[regionNo].GetObject(), newRegionObject, Helper.NavigationDirections.Down);
                }
                if (regionNo == _noRegionsToGenerate - 1)
                {
                    Helper.SetNavigation(newRegionObject, _backButton, Helper.NavigationDirections.Down);
                    Helper.SetNavigation(_backButton, newRegionObject, Helper.NavigationDirections.Up);
                }   
            }
            RefreshExploreButton();
        }

        private static void RefreshExploreButton()
        {
            Destroy(_exploreButton);
            _exploreButton = AddNewButton();
            _exploreButton.GetComponent<Button>().onClick.AddListener(AddRegion);
            _exploreButton.transform.Find("Text").GetComponent<Text>().text = "Explore...";
            if (_regions.Count != 0)
            {
                GameObject lastRegionInList = _regions[_regions.Count - 1].GetObject();
                Helper.SetNavigation(lastRegionInList, _exploreButton, Helper.NavigationDirections.Down);
                Helper.SetNavigation(_exploreButton, lastRegionInList, Helper.NavigationDirections.Up);
            }
            Helper.SetNavigation(_exploreButton, _backButton, Helper.NavigationDirections.Down);
            Helper.SetNavigation(_backButton, _exploreButton, Helper.NavigationDirections.Up);
            _exploreButton.GetComponent<Button>().Select();
        }

        private static GameObject AddNewButton()
        {
            GameObject newRegionObject = Instantiate(_regionPrefab);
            newRegionObject.transform.SetParent(_regionContainer.transform);
            newRegionObject.transform.localScale = new Vector3(1, 1, 1);
            return newRegionObject;
        }

        private static void UpdateRegionInfo(EnvironmentRegion region)
        {
            _regionInfoNameText.text = region.Name();
            _regionInfoTypeText.text = region.Type();
            _regionInfoDescriptionText.text = region.Description();
        }
    }
}