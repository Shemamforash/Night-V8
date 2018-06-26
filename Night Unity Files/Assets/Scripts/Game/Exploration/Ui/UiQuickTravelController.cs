using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Exploration.Ui
{
    public class UiQuickTravelController : MonoBehaviour, IInputListener
    {
        private const int Centre = 4;
        private readonly List<RegionUi> _regionUiList = new List<RegionUi>();
        private List<Region> _regions;
        private int _selectedRegion;
        public static UiQuickTravelController Instance;

        private TextMeshProUGUI _regionName, _regionDescription;

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
                case InputAxis.Cover:
                    ReturnToGame();
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
            _regionDescription = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Description");
            Transform listObject = Helper.FindChildWithName<Transform>(gameObject, "List");
            List<Transform> regions = Helper.FindAllChildren(listObject).FindAll(r => r.name == "Region");
            for (int i = 0; i < regions.Count; ++i)
            {
                RegionUi regionUi = new RegionUi(regions[i].gameObject, Math.Abs(i - Centre));
                _regionUiList.Add(regionUi);
                regionUi.SetNoRegion();
            }

            InputHandler.SetCurrentListener(this);
            _regions = MapGenerator.DiscoveredNodes();
            _currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();
            int curIndex = _regions.IndexOf(_currentRegion);
            Helper.Swap(curIndex, 0, _regions);

            _selectedRegion = 0;
            SelectRegion();
        }

        private void DrawNeighbors()
        {
            List<Region> visited = new List<Region>();
            Queue<Region> unvisited = new Queue<Region>();
            unvisited.Enqueue(_currentRegion);
            while (visited.Count != _regions.Count)
            {
                Region r = unvisited.Dequeue();
                visited.Add(r);
                r.Neighbors().ForEach(n =>
                {
                    if (!visited.Contains(n) && !unvisited.Contains(n))
                    {
                        unvisited.Enqueue(n);
                    }

                    Debug.DrawLine(r.Position, n.Position, Color.white, 0.05f);
                });
            }
        }

        private static Region _currentRegion;

        public void Start()
        {
            Camera.main.GetComponent<FitScreenToRoute>().Recenter();
        }

        private void ReturnToGame()
        {
            InputHandler.UnregisterInputListener(this);
            InputHandler.SetCurrentListener(null);
            SceneManager.LoadScene("Game");
        }

        private void TravelToRegion()
        {
            if (CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode() != _targetRegion)
            {
                Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
                float distance = Vector2.Distance(travelAction.GetCurrentNode().Position, _targetRegion.Position);
                int duration = MapGenerator.NodeDistanceToTime(distance);
                travelAction.TravelTo(_regions[_selectedRegion], duration);
            }

            ReturnToGame();
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
                    MapGenerator.SetRoute(_currentRegion, region);
                    UpdateCurrentRegionInfo(region);
                    _targetRegion = region;
                }

                _regionUiList[i].SetRegion(region);
            }
        }

        private void UpdateCurrentRegionInfo(Region region)
        {
            _regionName.text = region.Name;
            _regionDescription.text = region.Description();
        }

        private class RegionUi
        {
            private readonly Color _activeColour;
            private readonly EnhancedText _nameText, _distanceText;

            public RegionUi(GameObject gameObject, int offset)
            {
                _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
                _distanceText = Helper.FindChildWithName<EnhancedText>(gameObject, "Distance");
                _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
            }

            private void SetColor(Color c)
            {
                _nameText.SetColor(c);
                _distanceText.SetColor(c);
            }

            public void SetNoRegion()
            {
                SetColor(UiAppearanceController.InvisibleColour);
            }

            public void SetRegion(Region region)
            {
                _nameText.Text(region.Name);
                float duration = RoutePlotter.DistanceBetween(region, _currentRegion);
                duration /= MapGenerator.MinRadius;
                _distanceText.Text(WorldState.TimeToHours((int) duration * WorldState.MinutesPerHour));
                SetColor(_activeColour);
            }
        }
    }
}