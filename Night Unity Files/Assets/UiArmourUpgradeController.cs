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
using UnityEngine.UI;

public class UiArmourUpgradeController : Menu, IInputListener
{
    private static string _previousMenu;
    private static bool _upgradingAllowed;

    private static GameObject _armourList;

    private static RectTransform _selectorTransform;

    private static EnhancedButton _inscribeButton, _repairButton, _replaceButton, _insertButton;
    private static EnhancedButton _centreButton;

    private static Player _currentPlayer;
    private static ArmourPlate _currentSelectedPlate;

    private static readonly List<ArmourUi> _plateUis = new List<ArmourUi>();
    private static int _selectedArmour;

    private static ArmourPlateUi _plateOneUi;
    private static ArmourPlateUi _plateTwoUi;
    private static ArmourPlateUi _selectedPlateUi;

    private class ArmourPlateUi
    {
        private readonly EnhancedText _nameText;
        private readonly EnhancedText _currentArmourText, _armourDetailText;
        private readonly EnhancedText _inscriptionText;
        private readonly Image _selectedImage;
        public readonly EnhancedButton Button;

        public ArmourPlateUi(GameObject gameObject)
        {
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");
            _currentArmourText = Helper.FindChildWithName<EnhancedText>(gameObject, "Current Armour");
            _armourDetailText = Helper.FindChildWithName<EnhancedText>(gameObject, "Armour Detail");
            _selectedImage = gameObject.GetComponent<Image>();
            Button = gameObject.GetComponent<EnhancedButton>();
        }

        public void SetPlate(ArmourPlate plate)
        {
            _nameText.Text(plate.ExtendedName());
            _currentArmourText.Text(plate.Weight + " Armour");
            _armourDetailText.Text("TODO");
        }

        public void SetSelected(bool selected)
        {
            _selectedImage.color = selected ? new Color(1f, 1f, 1f, 0.4f) : new Color(1f, 1f, 1f, 0f);
            Button.Button().Select();
        }
    }

    private void SetNavigation()
    {
    }

    private static void SetNoPlateInserted()
    {
        _insertButton.gameObject.SetActive(true);
        _replaceButton.gameObject.SetActive(false);
        _inscribeButton.gameObject.SetActive(false);
        _repairButton.gameObject.SetActive(false);
    }

    private static void SetPlateInserted(ArmourPlate plate)
    {
        _insertButton.gameObject.SetActive(false);
        _replaceButton.gameObject.SetActive(true);
        //todo only show repair button if needs repair
        _repairButton.gameObject.SetActive(true);
        _inscribeButton.gameObject.SetActive(plate._inscribable);
    }

    private static void SelectPlateUi(ArmourPlateUi plateUi)
    {
        _selectedPlateUi = plateUi;
        bool isPlateOne = plateUi == _plateOneUi;
        _plateOneUi.SetSelected(isPlateOne);
        _plateTwoUi.SetSelected(!isPlateOne);
        ArmourPlate plate = isPlateOne ? _currentPlayer.ArmourController.GetPlateOne() : _currentPlayer.ArmourController.GetPlateTwo();
        if (plate == null)
        {
            SetNoPlateInserted();
        }
        else
        {
            SetPlateInserted(plate);
        }
    }

    public void Awake()
    {
        _plateOneUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 1"));
        _plateOneUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateOneUi));
        _plateTwoUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 2"));
        _plateTwoUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateTwoUi));
        _armourList = Helper.FindChildWithName(gameObject, "Armour List");

        _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
        _repairButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Repair");
        _replaceButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Replace");
        _insertButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Insert");

        for (int i = 0; i < 7; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(_armourList, "Item " + i);
            ArmourUi armourUi = new ArmourUi(uiObject, Math.Abs(i - 3));
            if (i == 3)
            {
                _centreButton = uiObject.GetComponent<EnhancedButton>();
            }

            _plateUis.Add(armourUi);
            armourUi.SetPlate(null);
        }

        _centreButton.AddOnClick(() =>
        {
            Equip(WorldState.HomeInventory().Armour()[_selectedArmour]);
            DisableInput();
        });

        _replaceButton.AddOnClick(EnableInput);
    }

    private void ReturnToPreviousMenu()
    {
        MenuStateMachine.ShowMenu(_previousMenu);
    }

    public static void Show(Player player)
    {
        _previousMenu = MenuStateMachine.States.GetCurrentState().Name;
        MenuStateMachine.ShowMenu("Armour Upgrade Menu");
        _currentPlayer = player;
        SelectPlateUi(_plateOneUi);
        SelectArmour();
    }

    private void Equip(ArmourPlate plate)
    {
        if (_selectedPlateUi == _plateOneUi)
        {
            _currentPlayer.EquipArmourSlotOne(plate);
        }
        else
        {
            _currentPlayer.EquipArmourSlotTwo(plate);
        }

        Show(_currentPlayer);
    }

    public static bool PlatesAreAvailable()
    {
        return WorldState.HomeInventory().Armour().Count != 0;
    }

    public void EnableInput()
    {
        InputHandler.RegisterInputListener(this);
        _centreButton.Button().Select();
    }

    private void DisableInput()
    {
        InputHandler.UnregisterInputListener(this);
    }

    private static void SelectArmour()
    {
        int centre = 3;
        for (int i = 0; i < _plateUis.Count; ++i)
        {
            int offset = i - centre;
            int targetPlate = _selectedArmour + offset;
            ArmourPlate plate = null;
            if (targetPlate >= 0 && targetPlate < WorldState.HomeInventory().Armour().Count)
            {
                plate = WorldState.HomeInventory().Armour()[targetPlate];
            }

            _plateUis[i].SetPlate(plate);
        }
    }

    private void TrySelectArmourBelow()
    {
        if (_selectedArmour == WorldState.HomeInventory().Weapons().Count - 1) return;
        ++_selectedArmour;
        SelectArmour();
    }

    private void TrySelectArmourAbove()
    {
        if (_selectedArmour == 0) return;
        --_selectedArmour;
        SelectArmour();
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
            TrySelectArmourBelow();
        }
        else
        {
            TrySelectArmourAbove();
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    private class ArmourUi
    {
        private readonly EnhancedText _typeText, _nameText, _dpsText;
        private readonly Color _activeColour;

        public ArmourUi(GameObject uiObject, int offset)
        {
            _typeText = Helper.FindChildWithName<EnhancedText>(uiObject, "Type");
            _nameText = Helper.FindChildWithName<EnhancedText>(uiObject, "Name");
            _dpsText = Helper.FindChildWithName<EnhancedText>(uiObject, "Dps");
            _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
        }

        private void SetColour(Color c)
        {
            _typeText.SetColor(c);
            _nameText.SetColor(c);
        }

        public void SetPlate(ArmourPlate plate)
        {
            if (plate == null)
            {
                SetColour(new Color(1, 1, 1, 0f));
                return;
            }

            SetColour(_activeColour);
            _typeText.Text(plate.Weight + "Armour");
            _nameText.Text(plate.ExtendedName());
            _dpsText.Text("");
        }
    }
}