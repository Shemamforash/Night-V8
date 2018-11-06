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
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class MapMovementController : MonoBehaviour, IInputListener
{
    private Rigidbody2D _rigidBody2D;
    private Vector2 _direction;
    private const float PanSpeed = 40f;
    private float CurrentSpeed;
    private List<Region> _availableRegions;
    private Region _nearestRegion;
    private static Region _currentRegion;
    private static UIAttributeMarkerController _gritMarker;
    public static Camera MapCamera;
    private static Player _player;
    private static MapMovementController _instance;
    private bool _pressed;
    private Vector3 velocity;

    public void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _availableRegions = MapGenerator.DiscoveredRegions();
        _gritMarker = GameObject.Find("Grit").FindChildWithName<UIAttributeMarkerController>("Bar");
        MapCamera = GameObject.Find("Map Camera").GetComponent<Camera>();
        _instance = this;
    }

    public static void Enter(Player player)
    {
        ResourcesUiController.Hide();
        _player = player;
        _gritMarker.SetValue(_player.Attributes.Get(AttributeType.Grit));
        _currentRegion = _player.TravelAction.GetCurrentNode();
        MapCamera.DOOrthoSize(7, 1f);
        InputHandler.SetCurrentListener(_instance);
        Recenter();
    }

    public static void Exit()
    {
        ResourcesUiController.Show();
        InputHandler.SetCurrentListener(null);
        MapCamera.DOOrthoSize(3, 1f);
        _player = null;
    }

    public static void UpdateGrit(int gritCost)
    {
        CharacterAttribute grit = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Grit);
        _gritMarker.SetValue((int) grit.Max, (int) (grit.CurrentValue() - gritCost));
    }

    private static void Recenter()
    {
        Vector3 position = _currentRegion.Position;
        position.z = -10;
        MapCamera.transform.position = position;
    }

    public void FixedUpdate()
    {
        if (_player == null) return;
        if (!_pressed && _nearestRegion != null) LocateToNearestRegion();
        _direction.Normalize();
        _direction *= CurrentSpeed;
        _rigidBody2D.AddForce(_direction);
        _direction = Vector2.zero;
    }

    private void LocateToNearestRegion()
    {
        float distance = ((Vector2) transform.position).Distance(_nearestRegion.Position);
        if (distance < 0.01f)
        {
            Vector3 snappedPosition = _nearestRegion.Position;
            snappedPosition.z = transform.position.z;
            transform.position = snappedPosition;
            return;
        }

        _direction = _nearestRegion.Position.Direction(transform.position);
        CurrentSpeed = distance * Time.fixedDeltaTime * 250;
    }

    public void Update()
    {
        if (_player == null) return;
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
        SetNearestRegion(newNearestRegion);
    }

    private void SetNearestRegion(Region newNearestRegion)
    {
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
                _pressed = true;
                _direction.x += direction;
                break;
            case InputAxis.Vertical:
                _pressed = true;
                _direction.y += direction;
                break;
            case InputAxis.Cover:
                if (!MapMenuController.IsReturningFromCombat) ReturnToGame(_currentRegion);
                break;
            case InputAxis.Fire:
                TravelToRegion();
                break;
        }

        bool isRegionVisible = false;
        foreach (Region r in _availableRegions)
        {
            Vector2 potentialPosition = r.Position - _direction * 5;
            if (!potentialPosition.InCameraView(MapCamera)) continue;
            isRegionVisible = true;
            break;
        }

        if (!isRegionVisible) _direction = Vector2.zero;
        CurrentSpeed = PanSpeed;
    }

    private bool CanAffordToTravel()
    {
        int gritCost = _nearestRegion.MapNode().GetGritCost();
        bool canAfford = CharacterManager.SelectedCharacter.CanAffordTravel(gritCost);
        bool travellingToGate = _nearestRegion.GetRegionType() == RegionType.Gate;
        bool canAffordToTravel = canAfford || travellingToGate;
        return canAffordToTravel;
    }

    private void TravelToRegion()
    {
        if (_nearestRegion == null) return;
        if (!CanAffordToTravel()) return;
        Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
        travelAction.TravelTo(_nearestRegion, _nearestRegion.MapNode().GetGritCost());
        if (_currentRegion == _nearestRegion) return;
        MapMenuController.IsReturningFromCombat = false;
        ReturnToGame(_nearestRegion);
    }

    private static void ReturnToGame(Region region)
    {
        MapMenuController.FlashCloseButton();
        region.MapNode().Enter();
        MenuStateMachine.ShowMenu("Game Menu");
        InputHandler.SetCurrentListener(null);
    }

    public void OnInputUp(InputAxis axis)
    {
        if (axis != InputAxis.Horizontal && axis != InputAxis.Vertical) return;
        _pressed = false;
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}