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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIActionListController : MonoBehaviour
{
    private bool _exploreEnabled, _craftEnabled, _meditateEnabled, _sleepEnabled, _enterGateEnabled;
    private EnhancedButton _exploreButton, _craftButton, _inventoryButton, _meditateButton, _enterGateButton;
    private TextMeshProUGUI _exploreText, _craftText, _meditateText;
    private List<EnhancedButton> _buttons;
    private Player _player;
    private bool _atHome;
    private bool _resting;

    public void Awake()
    {
        _exploreButton = gameObject.FindChildWithName<EnhancedButton>("Explore");
        _exploreText = _exploreButton.FindChildWithName<TextMeshProUGUI>("Text");
        _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
        _craftText = _craftButton.FindChildWithName<TextMeshProUGUI>("Text");
        _inventoryButton = gameObject.FindChildWithName<EnhancedButton>("Inventory");
        _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
        _meditateText = _meditateButton.FindChildWithName<TextMeshProUGUI>("Text");
        _enterGateButton = gameObject.FindChildWithName<EnhancedButton>("Enter Gate");
        _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _inventoryButton, _meditateButton};
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _player.TravelAction.SetButton(_exploreButton);
        _player.CraftAction.SetButton(_craftButton);
        _player.ConsumeAction.SetButton(_inventoryButton);
        _player.MeditateAction.SetButton(_meditateButton);
        _enterGateButton.AddOnClick(GateTransitController.StartTransit);
    }

    public List<EnhancedButton> Buttons()
    {
        return _buttons;
    }

    public void SelectInitial() => _exploreButton.Select();

    private void UpdateExploreButton()
    {
        _exploreEnabled = _atHome && _resting && _player.Attributes.Val(AttributeType.Grit) > 0;
        _exploreButton.Button().interactable = _exploreEnabled;
        _exploreText.alpha = _exploreEnabled ? 1f : 0.4f;
    }

    private void UpdateCraftButton()
    {
        _craftEnabled = Recipe.RecipesAvailable() && _atHome && _resting;
        _craftButton.Button().interactable = _craftEnabled;
        _craftText.alpha = _craftEnabled ? 1f : 0.4f;
    }

    private void UpdateInventoryButton()
    {
        _inventoryButton.gameObject.SetActive(true);
    }

    private void UpdateMeditateButton()
    {
        bool hasWill = _player.Attributes.Val(AttributeType.Will) > 0;
        bool canUseWill = !_player.Attributes.Get(AttributeType.Life).ReachedMax()
                          || !_player.Attributes.Get(AttributeType.Grit).ReachedMax()
                          || !_player.Attributes.Get(AttributeType.Focus).ReachedMax();
        _meditateEnabled = _resting && hasWill && canUseWill;
        if (TutorialManager.Active() && MapGenerator.DiscoveredRegions().Count == 1) _meditateEnabled = false;
        _meditateButton.Button().interactable = _meditateEnabled;
        _meditateText.alpha = _meditateEnabled ? 1f : 0.4f;
    }

    private void UpdateEnterGateAction()
    {
        _enterGateEnabled = _resting
                            && _atHome
                            && _player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer
                            && WorldState.AllTemplesActive();
        _enterGateButton.gameObject.SetActive(_enterGateEnabled);
    }

    public bool UpdateList()
    {
        _atHome = _player.TravelAction.AtHome();
        _resting = _player.States.GetCurrentState() is Rest;
        UpdateExploreButton();
        UpdateCraftButton();
        UpdateInventoryButton();
        UpdateMeditateButton();
        UpdateEnterGateAction();
        return CheckIfButtonNeedsSelecting();
    }

    private bool CheckIfButtonNeedsSelecting()
    {
        if (TutorialManager.Instance.IsTutorialVisible()) return false;
        if (_player != CharacterManager.SelectedCharacter) return false;
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        if (currentObject != null && currentObject.activeInHierarchy) return false;
        EnhancedButton firstButton = _buttons.FirstOrDefault(b => b.gameObject.activeInHierarchy && b.Button().interactable);
        if (firstButton == null) return true;
        firstButton.Select();
        return false;

    }
}