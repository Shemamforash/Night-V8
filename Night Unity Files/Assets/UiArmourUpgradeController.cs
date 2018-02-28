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
    private static bool _upgradingAllowed;

    private static GameObject _armourList;

    private static RectTransform _selectorTransform;

    private static EnhancedButton _closeButton;
    private static EnhancedButton _centreButton;

    private static Player _currentPlayer;
    private static ArmourPlate _currentSelectedPlate;

    private static readonly List<ArmourUi> _plateUis = new List<ArmourUi>();
    private static int _selectedArmour;

    private static ArmourPlateUi _plateOneUi;
    private static ArmourPlateUi _plateTwoUi;
    private static ArmourPlateUi _selectedPlateUi;

    private static UiArmourUpgradeController _instance;
    private const int centre = 3;

    private class ArmourPlateUi
    {
        private readonly EnhancedText _nameText;
        private readonly EnhancedText _currentArmourText, _armourDetailText;
        private readonly EnhancedText _inscriptionText;
        private readonly EnhancedButton _inscribeButton, _repairButton;
        private readonly Image _selectedImage;
        public readonly EnhancedButton Button;

        public ArmourPlateUi(GameObject gameObject)
        {
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");
            _currentArmourText = Helper.FindChildWithName<EnhancedText>(gameObject, "Current Armour");
            _armourDetailText = Helper.FindChildWithName<EnhancedText>(gameObject, "Armour Detail");
            _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
            _repairButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Repair");
            Button = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            Button.AddOnClick(() =>
            {
                if (PlatesAreAvailable()) EnableInput();
            });
        }

        public void SetPlate(ArmourPlate plate)
        {
            if (plate == null)
            {
                _nameText.Text("");
                _currentArmourText.Text("No Plate");
                _armourDetailText.Text("");
                _inscriptionText.Text("");
                return;
            }

            _nameText.Text(plate.ExtendedName());
            _currentArmourText.Text(plate.Weight + " Armour");
            _armourDetailText.Text("TODO");
        }

        public void SetSelected(bool selected)
        {
            if (selected) Button.Button().Select();
        }

        public void SetNoPlateInserted()
        {
            _inscribeButton.gameObject.SetActive(false);
            _repairButton.gameObject.SetActive(false);
        }

        public void SetPlateInserted(ArmourPlate plate)
        {
            if (plate.GetRepairCost() > 0) _repairButton.gameObject.SetActive(true);
            _inscribeButton.gameObject.SetActive(plate.Inscribable);
        }
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
            _selectedPlateUi.SetNoPlateInserted();
        }
        else
        {
            _selectedPlateUi.SetPlateInserted(plate);
        }
    }

    public void Awake()
    {
        _instance = this;
        _plateOneUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 1"));
        _plateOneUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateOneUi));
        _plateTwoUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 2"));
        _plateTwoUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateTwoUi));
        _armourList = Helper.FindChildWithName(gameObject, "Armour List");
        _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close");
        _closeButton.AddOnClick(MenuStateMachine.GoToInitialMenu);

        for (int i = 0; i < 7; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(_armourList, "Item " + i);
            ArmourUi armourUi = new ArmourUi(uiObject, Math.Abs(i - centre));
            if (i == centre)
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
    }

    private static void UpdatePlates()
    {
        _plateOneUi.SetPlate(_currentPlayer.ArmourController.GetPlateOne());
        _plateTwoUi.SetPlate(_currentPlayer.ArmourController.GetPlateTwo());
    }

    public static void Show(Player player)
    {
        MenuStateMachine.ShowMenu("Armour Upgrade Menu");
        _currentPlayer = player;
        SelectPlateUi(_plateOneUi);
        SelectArmour();
        UpdatePlates();
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

    private static bool PlatesAreAvailable()
    {
        return WorldState.HomeInventory().Armour().Count != 0;
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

    private static void SelectArmour()
    {
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

    private static void TrySelectArmourBelow()
    {
        if (_selectedArmour == WorldState.HomeInventory().Armour().Count - 1) return;
        ++_selectedArmour;
        SelectArmour();
    }

    private static void TrySelectArmourAbove()
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
            _dpsText.SetColor(c);
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