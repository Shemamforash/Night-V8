using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiBrandMenu : Menu
{
    private EnhancedText _titleText, _benefitText, _effectText, _overviewText;
    private CanvasGroup _detailCanvas, _overviewCanvas;
    private static UiBrandMenu _instance;
    private CloseButtonController _closeButton;
    private Menu _lastMenu;
    private static string _titleString, _benefitString, _effectString, _overviewString;

    public override void Awake()
    {
        base.Awake();
        _detailCanvas = gameObject.FindChildWithName<CanvasGroup>("Detail");
        _overviewCanvas = gameObject.FindChildWithName<CanvasGroup>("Overview");
        _overviewText = _overviewCanvas.GetComponent<EnhancedText>();
        _titleText = gameObject.FindChildWithName<EnhancedText>("Title");
        _benefitText = gameObject.FindChildWithName<EnhancedText>("Benefit");
        _effectText = gameObject.FindChildWithName<EnhancedText>("Effect");
        _instance = this;
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
    }

    private void Hide()
    {
        _closeButton.Disable();
        MenuStateMachine.ShowMenu(_lastMenu.name);
        CombatManager.Resume();
    }

    private void Show()
    {
        _lastMenu = MenuStateMachine.CurrentMenu();
        ScreenFaderController.FlashWhite(1f, new Color(1, 1, 1, 0f));
        MenuStateMachine.ShowMenu("Brand Menu");
        CombatManager.Pause();
        ShowOverview();
    }

    private void ShowOverview()
    {
        _overviewText.SetText(_overviewString);
        _overviewCanvas.alpha = 1;
        _detailCanvas.alpha = 0;
        _closeButton.UseFireInput();
        _closeButton.Enable();
        _closeButton.SetOnClick(ShowDetail);
        _closeButton.SetCallback(ShowDetail);
    }

    private void ShowDetail()
    {
        _titleText.SetText(_titleString);
        _benefitText.SetText(_benefitString);
        _effectText.SetText(_effectString);
        _overviewCanvas.alpha = 0;
        _detailCanvas.alpha = 1;
        _closeButton.UseDefaultInput();
        _closeButton.SetOnClick(Hide);
        _closeButton.SetCallback(Hide);
    }

    public static void ShowBrand(Brand brand)
    {
        _overviewString = "Brand Complete";
        _titleString = "";
        _benefitString = "";
        switch (brand.Status)
        {
            case BrandStatus.Failed:
                Debug.Log("failed " + brand.GetFailName());
                _titleString = "Failed";
                _benefitString = "A curse of " + brand.GetFailName() + " has been cast upon " + CharacterManager.SelectedCharacter.Name;
                break;
            case BrandStatus.Succeeded:
                Debug.Log("passed " + brand.GetSuccessName());
                _titleString = "Passed";
                _benefitString = "A boon of " + brand.GetSuccessName() + " has been granted upon " + CharacterManager.SelectedCharacter.Name;
                break;
        }

        _titleString = _titleString + " " + brand.GetName();
        string descriptionString = "\n<i><size=20>" + brand.Description() + "</size></i>";
        _benefitString += descriptionString;
        _instance.Show();
    }

    public static void ShowWeaponSkillUnlock(WeaponType weaponType, Skill weaponSkill) => _instance.ShowWeaponSkill(weaponType, weaponSkill);

    public static void ShowCharacterSkillUnlock(Skill characterSkill) => _instance.ShowCharacterSkill(characterSkill);

    private void ShowCharacterSkill(Skill characterSkill)
    {
        _overviewString = "Skill Unlocked";
        _effectString = CharacterManager.SelectedCharacter.Name + " has grown with the passing of time";
        ShowSkill(characterSkill);
    }

    private void ShowWeaponSkill(WeaponType weaponType, Skill weaponSkill)
    {
        _overviewString = "Skill Unlocked";
        _effectString = CharacterManager.SelectedCharacter.Name + "'s proficiency with " + weaponType + "s grows";
        ShowSkill(weaponSkill);
    }

    private void ShowSkill(Skill skill)
    {
        _titleString = "The Skill '" + skill.Name + "' has been unlocked";
        _benefitString = skill.Description();
        _instance.Show();
    }
}