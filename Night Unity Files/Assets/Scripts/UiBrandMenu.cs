using System;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiBrandMenu : Menu
{
    private EnhancedText _titleText, _benefitText, _quoteText, _descriptionText, _overviewText;
    private CanvasGroup _detailCanvas, _overviewCanvas;
    private static UiBrandMenu _instance;
    private CloseButtonController _closeButton;
    private Menu _lastMenu;
    private static string _titleString, _benefitString, _quoteString, _overviewString, _descriptionString;

    protected override void Awake()
    {
        base.Awake();
        _detailCanvas = gameObject.FindChildWithName<CanvasGroup>("Detail");
        _overviewCanvas = gameObject.FindChildWithName<CanvasGroup>("Overview");
        _overviewText = _overviewCanvas.GetComponent<EnhancedText>();
        _titleText = gameObject.FindChildWithName<EnhancedText>("Title");
        _benefitText = gameObject.FindChildWithName<EnhancedText>("Benefit");
        _quoteText = gameObject.FindChildWithName<EnhancedText>("Quote");
        _descriptionText = gameObject.FindChildWithName<EnhancedText>("Description");
        _instance = this;
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
    }

    private void Hide()
    {
        _closeButton.Disable();
        MenuStateMachine.ShowMenu(_lastMenu.name);
        WorldState.Resume();
    }

    private void Show()
    {
        _lastMenu = MenuStateMachine.CurrentMenu();
        ScreenFaderController.FlashWhite(1f, new Color(1, 1, 1, 0f));
        MenuStateMachine.ShowMenu("Brand Menu");
        WorldState.Pause();
        ShowOverview();
    }

    private void ShowOverview()
    {
        _overviewText.SetText(_overviewString);
        _overviewCanvas.alpha = 1;
        _detailCanvas.alpha = 0;
        _closeButton.UseAcceptInput();
        _closeButton.Enable();
        _closeButton.SetOnClick(ShowDetail);
    }

    private void ShowDetail()
    {
        _titleText.SetText(_titleString);
        _benefitText.SetText(_benefitString);
        _quoteText.SetText(_quoteString);
        _descriptionText.SetText(_descriptionString);
        _overviewCanvas.alpha = 0;
        _detailCanvas.alpha = 1;
        _closeButton.SetOnClick(Hide);
    }

    public static void ShowBrand(Brand brand)
    {
        _overviewString = "Rite Complete";
        _titleString = "";
        _benefitString = "";
        switch (brand.Status)
        {
            case BrandStatus.Failed:
                _titleString = "Failed";
                _descriptionString = "You feel your body go weak";
                _benefitString = "All attributes reduced to 1";
                break;
            case BrandStatus.Succeeded:
                _titleString = "Passed";
                _descriptionString = "You feel a warmth bloom inside you";
                _benefitString = brand.GetEffectText();
                break;
        }

        _titleString = _titleString + " The " + brand.GetDisplayName();
        _quoteString = brand.Description();
        _instance.Show();
    }

    public static void ShowWeaponSkillUnlock(WeaponType weaponType, Skill weaponSkill, int skillNum) => _instance.ShowWeaponSkill(weaponType, weaponSkill, skillNum);

    public static void ShowCharacterSkillUnlock(Skill characterSkill, int skillNum) => _instance.ShowCharacterSkill(characterSkill, skillNum);

    private void ShowCharacterSkill(Skill characterSkill, int skillNum)
    {
        _overviewString = "Character Skill Unlocked";
        _descriptionString = "The " + CharacterManager.SelectedCharacter.Name + "'s wisdom has grown with the passing of time";
        ShowSkill(characterSkill, skillNum);
    }

    private string GetSkillUsageString(Skill skill, int skillNum)
    {
        string binding = GetBindingForSkill(skillNum);
        string requirements = "Requires " + skill.Cost() + " Adrenaline - " + skill.Cost() + " second".Pluralise(skill.Cost()) + " Cooldown";
        return binding + " - " + requirements;
    }

    private string GetBindingForSkill(int skillNum)
    {
        string keyName;
        switch (skillNum)
        {
            case 1:
                keyName = InputHandler.GetBindingForKey(InputAxis.SkillOne);
                break;
            case 2:
                keyName = InputHandler.GetBindingForKey(InputAxis.SkillTwo);
                break;
            case 3:
                keyName = InputHandler.GetBindingForKey(InputAxis.SkillThree);
                break;
            case 4:
                keyName = InputHandler.GetBindingForKey(InputAxis.SkillFour);
                break;
            default:
                throw new ArgumentOutOfRangeException("Invalid skill number passed " + skillNum);
        }

        return "Press [" + keyName + "] to use";
    }

    private void ShowWeaponSkill(WeaponType weaponType, Skill weaponSkill, int skillNum)
    {
        _overviewString = "Weapon Skill Unlocked";
        _descriptionString = CharacterManager.SelectedCharacter.Name + "'s proficiency with " + weaponType + "s grows";
        ShowSkill(weaponSkill, skillNum);
    }

    private void ShowSkill(Skill skill, int skillNum)
    {
        _titleString = skill.Name;
        _benefitString = skill.Description();
        _quoteString = GetSkillUsageString(skill, skillNum);
        _instance.Show();
    }
}