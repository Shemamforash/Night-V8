using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Exploration.Environment;
using NUnit.Framework;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class UiQuickTravelController : MonoBehaviour, IInputListener
    {
        private const int Centre = 7;
        private readonly List<RegionUi> _regionUiList = new List<RegionUi>();
        private List<MapNode> _regions;
        private int _selectedRegion;
        public static UiQuickTravelController Instance;

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
            Transform listObject = Helper.FindChildWithName<Transform>(gameObject, "List");
            List<Transform> regions = Helper.FindAllChildren(listObject).FindAll(r => r.name == "Region");
            for (int i = 0; i < 15; ++i)
            {
                RegionUi regionUi = new RegionUi(regions[i].gameObject, Math.Abs(i - Centre));
                _regionUiList.Add(regionUi);
                regionUi.SetRegion();
            }
            Disable();
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

        private void SelectRegion()
        {
            MapNode currentNode = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();
            Assert.NotNull(currentNode);

            for (int i = 0; i < _regionUiList.Count; ++i)
            {
                int offset = i - Centre;
                int targetRegion = _selectedRegion + offset;
                MapNode node = null;
                if (targetRegion >= 0 && targetRegion < _regions.Count) node = _regions[targetRegion];
                if (node == null) continue;
                if (i == Centre) MapGenerator.SetRoute(currentNode, node);
                _regionUiList[i].SetRegion(node.GetRegionName(), RoutePlotter.DistanceBetween(node, currentNode));
            }
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

            public void SetRegion()
            {
                SetColor(UiAppearanceController.InvisibleColour);
            }

            public void SetRegion(string name, float distance)
            {
                _nameText.Text(name);
                _distanceText.Text(Helper.Round(distance / (MapGenerator.MinRadius * 2f), 1) + "hrs");
                SetColor(_activeColour);
            }
        }
    }
}