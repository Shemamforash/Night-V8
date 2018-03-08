using System;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiQuickTravelController : MonoBehaviour, IInputListener
{
    private List<RegionUi> _regionUiList = new List<RegionUi>();
    private int centre = 7;
    private List<MapNode> _regions;
    private int _selectedRegion;

    public void Awake()
    {
        Transform listObject = Helper.FindChildWithName<Transform>(gameObject, "List");
        List<Transform> regions = Helper.FindAllChildren(listObject).FindAll(r => r.name == "Region");
        for (int i = 0; i < 15; ++i)
        {
            RegionUi regionUi = new RegionUi(regions[i].gameObject, Math.Abs(i - centre));
            _regionUiList.Add(regionUi);
            regionUi.SetRegion();
        }
    }

    private void OnEnable()
    {
        InputHandler.RegisterInputListener(this);
        _regions = MapGenerator.DiscoveredNodes();
        _regions.Remove(CharacterVisionController.Instance()?.CurrentNode);
        _selectedRegion = 0;
        SelectRegion();
    }

    public void OnDisable()
    {
        InputHandler.UnregisterInputListener(this);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                {
                    TrySelectRegionBelow();
                }
                else
                {
                    TrySelectRegionAbove();
                }

                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    private void TrySelectRegionBelow()
    {
        if (_selectedRegion == _regions.Count) return;
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
        if(CharacterVisionController.Instance()?.CurrentNode == null) return;

        for (int i = 0; i < _regionUiList.Count; ++i)
        {
            int offset = i - centre;
            int targetGear = _selectedRegion + offset;
            MapNode region = null;
            if (targetGear >= 0 && targetGear < _regions.Count)
            {
                region = _regions[targetGear];
            }

            if (i == centre)
            {
                //todo show info
                MapGenerator.SetRoute(CharacterVisionController.Instance().CurrentNode, region);
            }
            if(region == null) _regionUiList[i].SetRegion();
            else _regionUiList[i].SetRegion(region.name, "Region", region.DistanceToPoint(CharacterVisionController.Instance().CurrentNode));
        }
    }

    private class RegionUi
    {
        private EnhancedText _nameText, _typeText, _distanceText;
        private Color _activeColour;

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

        public void SetRegion(string name, string type, float distance)
        {
            _nameText.Text(name);
            _typeText.Text(name);
            _distanceText.Text(Helper.Round(distance / (MapGenerator.MinRadius * 2f), 1) + "hrs");
            SetColor(_activeColour);
        }
    }
}