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
    private EnhancedText _titleText, _benefitText, _effectText;
    private static UiBrandMenu _instance;
    private CloseButtonController _closeButton;
    private Menu _lastMenu;

    public override void Awake()
    {
        base.Awake();
        _titleText = gameObject.FindChildWithName<EnhancedText>("Title");
        _benefitText = gameObject.FindChildWithName<EnhancedText>("Benefit");
        _effectText = gameObject.FindChildWithName<EnhancedText>("Effect");
        _instance = this;
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetCallback(Hide);
        _closeButton.SetOnClick(Hide);
        _closeButton.UseFireInput();
    }

    private void Hide()
    {
        _closeButton.Disable();
        MenuStateMachine.ShowMenu(_lastMenu.name);
        CombatManager.Resume();
    }

    private void Show()
    {
        _closeButton.Enable();
        _lastMenu = MenuStateMachine.CurrentMenu();
        MenuStateMachine.ShowMenu("Brand Menu");
        CombatManager.Pause();
    }

    public static void ShowBrand(Brand brand)
    {
        string titleString = "";
        string benefitString = "";
        switch (brand.Status)
        {
            case BrandStatus.Failed:
                Debug.Log("failed " + brand.GetFailName());
                titleString = "Failed";
                benefitString = "A curse of " + brand.GetFailName() + " has been cast upon " + CharacterManager.SelectedCharacter.Name;
                break;
            case BrandStatus.Succeeded:
                Debug.Log("passed " + brand.GetSuccessName());
                titleString = "Passed";
                benefitString = "A boon of " + brand.GetSuccessName() + " has been granted upon " + CharacterManager.SelectedCharacter.Name;
                break;
        }

        titleString = titleString + " " + brand.GetName();
        _instance._titleText.SetText(titleString);
        string descriptionString = "\n<i><size=20>" + brand.Description() + "</size></i>";
        _instance._benefitText.SetText(benefitString + descriptionString);
        _instance._effectText.SetText(brand.GetEffectString());
        _instance.Show();
    }

    public static void ShowWeaponSkillUnlock(WeaponType weaponType, Skill weaponSkill)
    {
        _instance.ShowWeaponSkill(weaponType, weaponSkill);
    }

    public static void ShowCharacterSkillUnlock(Skill characterSkill)
    {
        _instance.ShowCharacterSkill(characterSkill);
    }

    private void ShowCharacterSkill(Skill characterSkill)
    {
        string titleString = CharacterManager.SelectedCharacter.Name + " has grown with the passing of time";
        ShowSkill(titleString, characterSkill);
    }

    private void ShowWeaponSkill(WeaponType weaponType, Skill weaponSkill)
    {
        string effectString = CharacterManager.SelectedCharacter.Name + "'s proficiency with " + weaponType + "s grows";
        ShowSkill(effectString, weaponSkill);
    }

    private void ShowSkill(string effectString, Skill skill)
    {
        string titleString = skill.Name + " has been unlocked";
        _titleText.SetText(titleString);
        _benefitText.SetText(skill.Description());
        _effectText.SetText(effectString);
        gameObject.SetActive(true);
        _instance.Show();
    }
}