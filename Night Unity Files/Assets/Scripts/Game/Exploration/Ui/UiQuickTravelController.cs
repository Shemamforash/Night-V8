using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class UiQuickTravelController : MonoBehaviour, IInputListener
    {
        private const int Centre = 6;
        private readonly List<RegionUi> _regionUiList = new List<RegionUi>();
        private List<Region> _regions;
        private int _selectedRegion;
        public static UiQuickTravelController Instance;

        private TextMeshProUGUI _regionName, _regionType, _regionDescription;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.Vertical:
                    if (direction < 0)
                        TrySelectRegionBelow();
                    else
                        TrySelectRegionAbove();
                    break;
                case InputAxis.Fire:
                    TravelToRegion();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void Awake()
        {
            Instance = this;
            _regionName = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Region Name");
            _regionType = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Region Type");
            _regionDescription = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            Transform listObject = Helper.FindChildWithName<Transform>(gameObject, "List");
            List<Transform> regions = Helper.FindAllChildren(listObject).FindAll(r => r.name == "Region");
            for (int i = 0; i < 13; ++i)
            {
                RegionUi regionUi = new RegionUi(regions[i].gameObject, Math.Abs(i - Centre));
                _regionUiList.Add(regionUi);
                regionUi.SetNoRegion();
            }
            Disable();
        }
        
        private void TravelToRegion()
        {
            Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
            float distance = Vector2.Distance(travelAction.GetCurrentNode().Position, _targetRegion.Position);
            int duration = MapGenerator.NodeDistanceToTime(distance);
            travelAction.TravelTo(travelAction.GetCurrentNode(), travelAction.GetCurrentNode().Position, duration);
            SceneChanger.ChangeScene("Game");
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            InputHandler.SetCurrentListener(this);
            _regions = MapGenerator.DiscoveredNodes();
            _regions.Remove(CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode());
            _selectedRegion = 0;
            SelectRegion();
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            InputHandler.SetCurrentListener(null);
        }

        private void TrySelectRegionBelow()
        {
            if (_selectedRegion == _regions.Count - 1) return;
            ++_selectedRegion;
            SelectRegion();
        }

        private void TrySelectRegionAbove()
        {
            if (_selectedRegion == 0) return;
            --_selectedRegion;
            SelectRegion();
        }

        private Region _targetRegion;
        
        private void SelectRegion()
        {
            Region currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();
            Assert.NotNull(currentRegion);

            for (int i = 0; i < _regionUiList.Count; ++i)
            {
                int offset = i - Centre;
                int regionIndex = _selectedRegion + offset;
                Region region = null;
                if (regionIndex >= 0 && regionIndex < _regions.Count) region = _regions[regionIndex];
                if (region == null)
                {
                    _regionUiList[i].SetNoRegion();
                    continue;
                }

                if (i == Centre)
                {
                    MapGenerator.SetRoute(currentRegion, region);
                    UpdateCurrentRegionInfo(region);
                    _targetRegion = region;
                }
                _regionUiList[i].SetRegion(region.Name, RoutePlotter.DistanceBetween(region, currentRegion));
            }
        }

        private void UpdateCurrentRegionInfo(Region region)
        {
            _regionName.text = region.Name;
            _regionType.text = region.GetRegionType().ToString();
            string regionDescription = "";
            //todo region description
            _regionDescription.text = regionDescription;
        }

        private class RegionUi
        {
            private readonly Color _activeColour;
            private readonly EnhancedText _nameText, _typeText, _distanceText;

            public RegionUi(GameObject gameObject, int offset)
            {
                _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
                _typeText = Helper.FindChildWithName<EnhancedText>(gameObject, "Type");
                _distanceText = Helper.FindChildWithName<EnhancedText>(gameObject, "Distance");
                _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
            }

            private void SetColor(Color c)
            {
                _nameText.SetColor(c);
                _typeText.SetColor(c);
                _distanceText.SetColor(c);
            }

            public void SetNoRegion()
            {
                SetColor(UiAppearanceController.InvisibleColour);
            }

            public void SetRegion(string name, float duration)
            {
                _nameText.Text(name);
                _distanceText.Text(WorldState.TimeToHours((int) (duration / MapGenerator.MinRadius * WorldState.MinutesPerHour)));
                SetColor(_activeColour);
            }
        }
    }
}