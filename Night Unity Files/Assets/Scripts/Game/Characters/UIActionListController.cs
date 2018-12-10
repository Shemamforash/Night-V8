using System.Collections.Generic;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UIActionListController : MonoBehaviour
{
    private EnhancedButton _exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton;
    private EnhancedText _exploreText, _craftText, _consumeText, _meditateText, _sleepText;
    private List<EnhancedButton> _buttons;
    private Player _player;
    private bool _atHome;
    private bool _resting;

    public void Awake()
    {
        _exploreButton = gameObject.FindChildWithName<EnhancedButton>("Explore");
        _exploreText = _exploreButton.gameObject.FindChildWithName<EnhancedText>("Text");
        _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
        _craftText = _craftButton.gameObject.FindChildWithName<EnhancedText>("Text");
        _consumeButton = gameObject.FindChildWithName<EnhancedButton>("Consume");
        _consumeText = _consumeButton.gameObject.FindChildWithName<EnhancedText>("Text");
        _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
        _meditateText = _meditateButton.gameObject.FindChildWithName<EnhancedText>("Text");
        _sleepButton = gameObject.FindChildWithName<EnhancedButton>("Sleep");
        _sleepText = _sleepButton.gameObject.FindChildWithName<EnhancedText>("Text");
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
    }

    public List<EnhancedButton> Buttons()
    {
        return _buttons;
    }

    public void SelectInitial() => _exploreButton.Select();

    private void UpdateExploreButton()
    {
        bool exploreEnabled = _atHome && _resting && _player.Attributes.Val(AttributeType.Grit) > 0;
        SetButtonEnabled(_exploreButton, _exploreText, exploreEnabled);
    }

    private void UpdateCraftButton()
    {
        bool craftEnabled = Recipe.RecipesAvailable() && _atHome && _resting;
        SetButtonEnabled(_craftButton, _craftText, craftEnabled);
    }

    private void UpdateConsumeButton()
    {
        bool consumeEnabled = Inventory.Consumables().Count > 0 && !(_player.States.GetCurrentState() is Sleep);
        SetButtonEnabled(_consumeButton, _consumeText, consumeEnabled);
    }

    private void UpdateMeditateButton()
    {
        bool meditateEnabled = _resting
                               && _player.Attributes.Val(AttributeType.Will) > 0
                               && !_player.Attributes.Get(AttributeType.Fettle).ReachedMax()
                               && !_player.Attributes.Get(AttributeType.Grit).ReachedMax()
                               && !_player.Attributes.Get(AttributeType.Focus).ReachedMax();
        SetButtonEnabled(_meditateButton, _meditateText, meditateEnabled);
    }

    private void UpdateSleepButton()
    {
        bool sleepEnabled = false;
        sleepEnabled |= _player.CanSleep() && _atHome && _resting;
        sleepEnabled |= _player.States.GetCurrentState() == _player.SleepAction;
        SetButtonEnabled(_sleepButton, _sleepText, sleepEnabled);
    }

    public void UpdateList()
    {
        _atHome = _player.TravelAction.AtHome();
        _resting = _player.States.GetCurrentState() is Rest;
        UpdateExploreButton();
        UpdateCraftButton();
        UpdateConsumeButton();
        UpdateMeditateButton();
        UpdateSleepButton();
    }

    private void SetButtonEnabled(EnhancedButton button, EnhancedText text, bool enableButton)
    {
        button.gameObject.SetActive(enableButton);
        return;

        if (button.IsEnabled() == enableButton) return;
        button.SetEnabled(enableButton);
        text.SetColor(enableButton ? Color.white : UiAppearanceController.FadedColour);
    }
}