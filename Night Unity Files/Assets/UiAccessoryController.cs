using System;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.Characters.Player;
using Game.Gear.Armour;
using Game.World;
using SamsHelper;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiAccessoryController : Menu, IInputListener
{
    private static bool _upgradingAllowed;

    private static GameObject _accessoryList;

    private static RectTransform _selectorTransform;

    private static Player _currentPlayer;

    private static readonly List<AccessoryUi> _accessoryUis = new List<AccessoryUi>();
    private static int _selectedAccessory;


    private static UiAccessoryController _instance;
    private const int centre = 3;
    private static EnhancedText _nameText, _descriptionText, _compareText, _inscriptionText;
    private static EnhancedButton _inscribeButton, _closeButton;
    private static EnhancedButton _centreButton;

    private static void ShowAccessoryInfo()
    {
        if (_currentPlayer.Accessory != null)
        {
            _nameText.Text(_currentPlayer.Accessory.Name);
            _descriptionText.Text(_currentPlayer.Accessory.GetSummary());
        }
        else
        {
            _nameText.Text("");
            _descriptionText.Text("No Accessory Equipped");
            _inscriptionText.Text("");
        }
        _compareText.Text("");
    }
    
    public void Awake()
    {
        _instance = this;
        _accessoryList = Helper.FindChildWithName(gameObject, "Accessory List");
        _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close");
        _closeButton.AddOnClick(MenuStateMachine.GoToInitialMenu);
        _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
        _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");
        _descriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Description");
        _compareText = Helper.FindChildWithName<EnhancedText>(gameObject, "Compare");
        
        _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
        
        Helper.FindChildWithName(gameObject, "Info").GetComponent<EnhancedButton>().AddOnClick(() =>
        {
            if (AccessoriesAreAvailable()) EnableInput();
        });

        for (int i = 0; i < 7; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(_accessoryList, "Item " + i);
            AccessoryUi accessoryUi = new AccessoryUi(uiObject, Math.Abs(i - centre));
            if (i == centre)
            {
                _centreButton = uiObject.GetComponent<EnhancedButton>();
            }

            _accessoryUis.Add(accessoryUi);
            accessoryUi.SetAccessory(null);
        }

        _centreButton.AddOnClick(() =>
        {
            Equip(WorldState.HomeInventory().Accessories()[_selectedAccessory]);
            DisableInput();
        });
    }

    public static void Show(Player player)
    {
        MenuStateMachine.ShowMenu("Accessory Menu");
        _currentPlayer = player;
        ShowAccessoryInfo();
        SelectAccessory();
    }

    private void Equip(Accessory accessory)
    {
        _currentPlayer.EquipAccessory(accessory);
        Show(_currentPlayer);
    }

    private static bool AccessoriesAreAvailable()
    {
        return WorldState.HomeInventory().Accessories().Count != 0;
    }

    private static void EnableInput()
    {
        InputHandler.RegisterInputListener(_instance);
        _centreButton.Button().Select();
    }

    private void DisableInput()
    {
        InputHandler.UnregisterInputListener(this);
    }
    

    private static void SelectAccessory()
    {
        for (int i = 0; i < _accessoryUis.Count; ++i)
        {
            int offset = i - centre;
            int targetPlate = _selectedAccessory + offset;
            Accessory accessory = null;
            if (targetPlate >= 0 && targetPlate < WorldState.HomeInventory().Accessories().Count)
            {
                accessory = WorldState.HomeInventory().Accessories()[targetPlate];
            }

            if (i == centre && accessory != null)
            {
                _compareText.Text(_currentPlayer.Accessory != null ? accessory.GetSummary() : "");
            }

            _accessoryUis[i].SetAccessory(accessory);
        }
    }

    private static void TrySelectAccessoryBelow()
    {
        if (_selectedAccessory == WorldState.HomeInventory().Accessories().Count - 1) return;
        ++_selectedAccessory;
        SelectAccessory();
    }

    private static void TrySelectAccessoryAbove()
    {
        if (_selectedAccessory == 0) return;
        --_selectedAccessory;
        SelectAccessory();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        if (axis == InputAxis.Cover)
        {
            DisableInput();
            Equip(null);
        }
        else if (axis != InputAxis.Vertical)
            return;

        if (direction < 0)
        {
            TrySelectAccessoryBelow();
        }
        else
        {
            TrySelectAccessoryAbove();
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    private class AccessoryUi
    {
        private readonly EnhancedText _accessoryNameText;
        private readonly Color _activeColour;

        public AccessoryUi(GameObject uiObject, int offset)
        {
            _accessoryNameText = uiObject.GetComponent<EnhancedText>();
            _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
        }

        public void SetAccessory(Accessory accessory)
        {
            if (accessory == null)
            {
                _accessoryNameText.SetColor(new Color(1, 1, 1, 0f));
                return;
            }

            _accessoryNameText.SetColor(_activeColour);
            _accessoryNameText.Text("");
        }
    }
}