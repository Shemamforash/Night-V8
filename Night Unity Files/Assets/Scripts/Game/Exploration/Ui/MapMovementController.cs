using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MapMovementController : MonoBehaviour, IInputListener
{
    private Rigidbody2D _rigidBody2D;
    private Vector2 _direction;
    private const float Speed = 40f;
    private List<Region> _availableRegions;
    private Region _nearestRegion;
    private Region _currentRegion;
    private static UIAttributeMarkerController _enduranceMarker;

    public void Awake()
    {
        Cursor.visible = false;
        Recenter();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _availableRegions = MapGenerator.DiscoveredRegions();
        _currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();
        _enduranceMarker = GameObject.Find("Endurance").FindChildWithName<UIAttributeMarkerController>("Bar");
        InputHandler.SetCurrentListener(this);
    }

    public void Start()
    {
        _enduranceMarker.SetValue(CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Endurance));
    }

    public static void UpdateEndurance(int enduranceCost)
    {
        CharacterAttribute endurance = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Endurance);
        _enduranceMarker.SetValue((int) endurance.Max, (int) (endurance.CurrentValue() - enduranceCost), true);
    }

    private static void Recenter()
    {
        Vector3 position = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Position;
        position.z = -10;
        Camera.main.transform.DOMove(position, 0.5f);
    }

    public void FixedUpdate()
    {
        _rigidBody2D.AddForce(_direction * Speed);
        _direction = Vector2.zero;
    }

    public void Update()
    {
        float nearestDistance = 1000;
        Region newNearestRegion = null;
        _availableRegions.ForEach(region =>
        {
            float distance = Vector2.Distance(transform.position, region.Position);
            if (distance > nearestDistance) return;
            nearestDistance = distance;
            newNearestRegion = region;
        });
        if (nearestDistance > 1.5f) newNearestRegion = null;
        if (_nearestRegion == newNearestRegion) return;
        _nearestRegion?.MapNode().LoseFocus();
        _nearestRegion = newNearestRegion;
        _nearestRegion?.MapNode().GainFocus();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        switch (axis)
        {
            case InputAxis.Horizontal:
                _direction.x += direction;
                break;
            case InputAxis.Vertical:
                _direction.y += direction;
                break;
            case InputAxis.Cover:
                ReturnToGame(_currentRegion);
                break;
            case InputAxis.Fire:
                TravelToRegion();
                break;
        }

        _direction.Normalize();
        bool isRegionVisible = false;
        foreach (Region r in _availableRegions)
        {
            Vector2 potentialPosition = r.Position - _direction * 5;
            if (!potentialPosition.InCameraView()) continue;
            isRegionVisible = true;
            break;
        }

        if (!isRegionVisible) _direction = Vector2.zero;
    }

    private void TravelToRegion()
    {
        if (_nearestRegion == null) return;
        int _enduranceCost = RoutePlotter.RouteBetween(_currentRegion, _nearestRegion).Count - 1;
        bool enoughEndurance = _enduranceCost <= CharacterManager.SelectedCharacter.Attributes.Val(AttributeType.Endurance);
        bool travellingToGate = _nearestRegion.GetRegionType() == RegionType.Gate;
        if (!enoughEndurance && !travellingToGate) return;
        Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
        travelAction.TravelTo(_nearestRegion, _enduranceCost);
        if (_currentRegion != _nearestRegion)
        {
            ReturnToGame(_nearestRegion);
        }
    }

    private static void ReturnToGame(Region region)
    {
        region.MapNode().Enter();
        SceneChanger.GoToGameScene();
        InputHandler.SetCurrentListener(null);
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}