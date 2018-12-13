using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
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
    private static Camera MapCamera;
    private static Player _player;
    private static MapMovementController _instance;
    private bool _pressed;
    private Vector3 velocity;
    private static bool _visible;
    private static AudioSource _audioSource;

    public void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _availableRegions = MapGenerator.SeenRegions();
        MapCamera = GetComponent<Camera>();
        _instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public static void Enter(Player player)
    {
        _visible = true;
        ResourcesUiController.Hide();
        _player = player;
        UpdateGrit(0);
        _currentRegion = _player.TravelAction.GetCurrentRegion();
        MapCamera.DOOrthoSize(6, 1f);
        InputHandler.SetCurrentListener(_instance);
        Recenter();
        _audioSource.DOFade(1f, 3f);
    }

    public static void Exit()
    {
        ResourcesUiController.Show();
        InputHandler.SetCurrentListener(null);
        _audioSource.DOFade(0f, 3f);
        MapCamera.DOOrthoSize(3, 1f);
        _player = null;
    }

    public static void UpdateGrit(int gritCost)
    {
        CharacterAttribute grit = CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Grit);
        MapMenuController.GritMarker().SetValue(grit.Max, grit.CurrentValue(), -gritCost);
    }

    private static void Recenter()
    {
        Vector3 position = _currentRegion.Position;
        position.z = -10;
        MapCamera.transform.position = position;
    }

    private void LocateToNearestRegion()
    {
        if (Cursor.visible) return;
        float distance = ((Vector2) transform.position).Distance(_nearestRegion.Position);
        if (distance < 0.01f)
        {
            Vector3 snappedPosition = _nearestRegion.Position;
            snappedPosition.z = transform.position.z;
            transform.position = snappedPosition;
            return;
        }

        _direction = _nearestRegion.Position.Direction(transform.position);
        CurrentSpeed = distance * Time.fixedDeltaTime * 400;
    }

    private Region FindNearestRegion(Vector2 point)
    {
        float nearestDistance = 1000;
        Region newNearestRegion = null;
        _availableRegions.ForEach(region =>
        {
            float distance = Vector2.Distance(point, region.Position);
            if (distance > nearestDistance) return;
            nearestDistance = distance;
            newNearestRegion = region;
        });
        if (nearestDistance > 1f) newNearestRegion = null;
        return newNearestRegion;
    }
    
    public void Update()
    {
        if (!_visible || _player == null) return;
        if (Cursor.visible)
        {
            MoveWithMouse();
            Vector2 mouseWorldPosition = MapCamera.ScreenToWorldPoint(Input.mousePosition);
            SetNearestRegion(FindNearestRegion(mouseWorldPosition));
        }
        else
        {
            SetNearestRegion(FindNearestRegion(transform.position));
        }
    }

    public void FixedUpdate()
    {
        if (InputHandler.GetCurrentListener() != this) return;
        if (!_visible) return;
        if (_player == null) return;
        if (!_pressed && _nearestRegion != null) LocateToNearestRegion();
        _direction.Normalize();
        _direction *= CurrentSpeed;
        _rigidBody2D.AddForce(_direction);
        _direction = Vector2.zero;
    }

    private void MoveWithMouse()
    {
        Vector2 mousePosition = Input.mousePosition;
        float screenRadius = Mathf.Min(Screen.height / 2f, Screen.width / 2f);
        mousePosition.x -= Screen.width / 2f;
        mousePosition.x = Mathf.Clamp(mousePosition.x, -screenRadius, screenRadius);
        mousePosition.y -= Screen.height / 2f;
        mousePosition.y = Mathf.Clamp(mousePosition.y, -screenRadius, screenRadius);
        mousePosition.x /= screenRadius;
        mousePosition.y /= screenRadius;
        _direction = mousePosition;
        if (mousePosition.magnitude < 0.25f) CurrentSpeed = 0f;
        else CurrentSpeed = (mousePosition.magnitude - 0.25f) / 0.75f * PanSpeed / 2f;
        ClampDirection();
    }

    private void ClampDirection()
    {
        bool isRegionVisible = false;
        foreach (Region r in _availableRegions)
        {
            Vector2 potentialPosition = r.Position - _direction * 5;
            if (!potentialPosition.InCameraView(MapCamera)) continue;
            isRegionVisible = true;
            break;
        }

        if (!isRegionVisible) _direction = Vector2.zero;
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
            case InputAxis.Menu:
                if (!MapMenuController.IsReturningFromCombat && !TutorialManager.IsTutorialVisible()) ReturnToGame();
                break;
            case InputAxis.Fire:
                TravelToRegion();
                break;
            case InputAxis.Mouse:
                TravelToRegion();
                break;
        }

        ClampDirection();
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
        if (TutorialManager.IsTutorialVisible()) return;
        Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
        travelAction.TravelTo(_nearestRegion, _nearestRegion.MapNode().GetGritCost());
        MapMenuController.IsReturningFromCombat = false;
        ReturnToGame();
    }

    public static void ReturnToGame()
    {
        MapMenuController.FlashCloseButton();
        MenuStateMachine.ShowMenu("Game Menu");
        InputHandler.SetCurrentListener(null);
        _visible = false;
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