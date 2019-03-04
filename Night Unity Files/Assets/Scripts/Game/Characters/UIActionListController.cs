using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIActionListController : MonoBehaviour
{
    private bool _exploreEnabled, _craftEnabled, _consumeEnabled, _meditateEnabled, _sleepEnabled, _enterGateEnabled;
    private EnhancedButton _exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton, _enterGateButton;
    private List<EnhancedButton> _buttons;
    private Player _player;
    private bool _atHome;
    private bool _resting;

    public void Awake()
    {
        _exploreButton = gameObject.FindChildWithName<EnhancedButton>("Explore");
        _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
        _consumeButton = gameObject.FindChildWithName<EnhancedButton>("Consume");
        _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
        _sleepButton = gameObject.FindChildWithName<EnhancedButton>("Sleep");
        _enterGateButton = gameObject.FindChildWithName<EnhancedButton>("Enter Gate");
        _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton};
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _player.TravelAction.SetButton(_exploreButton);
        _player.CraftAction.SetButton(_craftButton);
        _player.ConsumeAction.SetButton(_consumeButton);
        _player.MeditateAction.SetButton(_meditateButton);
        _player.SleepAction.SetButton(_sleepButton);
        _enterGateButton.AddOnClick(GateTransitController.StartTransit);
    }

    public List<EnhancedButton> Buttons()
    {
        return _buttons;
    }

    public void SelectInitial() => _exploreButton.Select();

    private void UpdateExploreButton()
    {
        Debug.Log(_atHome + " " + _resting + " " + _player.Attributes.Val(AttributeType.Grit));
        _exploreEnabled = _atHome && _resting && _player.Attributes.Val(AttributeType.Grit) > 0;
        _exploreButton.gameObject.SetActive(_exploreEnabled);
    }

    private void UpdateCraftButton()
    {
        _craftEnabled = Recipe.RecipesAvailable() && _atHome && _resting;
        _craftButton.gameObject.SetActive(_craftEnabled);
    }

    private void UpdateConsumeButton()
    {
        _consumeEnabled = Inventory.Consumables().Count > 0 && !(_player.States.GetCurrentState() is Sleep);
        _consumeButton.gameObject.SetActive(_consumeEnabled);
    }

    private void UpdateMeditateButton()
    {
        bool hasWill = _player.Attributes.Val(AttributeType.Will) > 0;
        bool canUseWill = !_player.Attributes.Get(AttributeType.Fettle).ReachedMax()
                          || !_player.Attributes.Get(AttributeType.Grit).ReachedMax()
                          || !_player.Attributes.Get(AttributeType.Focus).ReachedMax();
        _meditateEnabled = _resting && hasWill && canUseWill;
        if (TutorialManager.Active() && MapGenerator.DiscoveredRegions().Count == 1) _meditateEnabled = false;
        _meditateButton.gameObject.SetActive(_meditateEnabled);
    }

    private void UpdateEnterGateAction()
    {
        _enterGateEnabled = _resting
                            && _atHome
                            && _player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer
                            && WorldState.AllTemplesActive();
        _enterGateButton.gameObject.SetActive(_enterGateEnabled);
    }

    private void UpdateSleepButton()
    {
        _sleepEnabled = false;
        _sleepEnabled |= _player.CanSleep() && _atHome && _resting;
        _sleepEnabled |= _player.States.GetCurrentState() == _player.SleepAction;
        if (TutorialManager.Active() && MapGenerator.DiscoveredRegions().Count == 1) _sleepEnabled = false;
        _sleepButton.gameObject.SetActive(_sleepEnabled);
    }

    public bool UpdateList()
    {
        _atHome = _player.TravelAction.AtHome();
        _resting = _player.States.GetCurrentState() is Rest;
        UpdateExploreButton();
        UpdateCraftButton();
        UpdateConsumeButton();
        UpdateMeditateButton();
        UpdateSleepButton();
        UpdateEnterGateAction();
        return CheckIfButtonNeedsSelecting();
    }

    private bool CheckIfButtonNeedsSelecting()
    {
        if (_player != CharacterManager.SelectedCharacter) return false;
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        if (currentObject != null && currentObject.activeInHierarchy) return false;
        EnhancedButton firstButton = _buttons.FirstOrDefault(b => b.gameObject.activeInHierarchy);
        if (firstButton != null)
        {
            firstButton.Select();
            return false;
        }

        return true;
    }

    public RectTransform SleepRect()
    {
        return _sleepButton.GetComponent<RectTransform>();
    }
}