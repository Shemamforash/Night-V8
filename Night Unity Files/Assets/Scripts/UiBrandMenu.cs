using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;

public class UiBrandMenu : Menu
{
    private EnhancedText _titleText, _benefitText, _effectText;
    private static UiBrandMenu _instance;
    private CloseButtonController _closeButton;

    public override void Awake()
    {
        base.Awake();
        _titleText = gameObject.FindChildWithName<EnhancedText>("Title");
        _benefitText = gameObject.FindChildWithName<EnhancedText>("Benefit");
        _effectText = gameObject.FindChildWithName<EnhancedText>("Effect");
        _instance = this;
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetInputAxis(InputAxis.Fire);
        _closeButton.SetCallback(Hide);
    }

    private void Hide()
    {
        _closeButton.Disable();
        gameObject.SetActive(false);
    }

    private void Show(Brand brand)
    {
        string titleString = "";
        string benefitString = "";
        switch (brand.Status)
        {
            case BrandStatus.Failed:
                titleString = "failed";
                benefitString = "A curse of " + brand.GetFailName() + " has been cast upon " + CharacterManager.SelectedCharacter.Name;
                break;
            case BrandStatus.Succeeded:
                titleString = "passed";
                benefitString = "A boon of " + brand.GetSuccessName() + " has been granted upon " + CharacterManager.SelectedCharacter.Name;
                break;
        }

        titleString = brand.GetName() + "has been " + titleString;
        _titleText.SetText(titleString);
        _benefitText.SetText(benefitString);
        _effectText.SetText(brand.GetEffectString());
        gameObject.SetActive(true);
        _closeButton.Enable();
    }

    public static void ShowBrand(Brand brand)
    {
        _instance.Show(brand);
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
        string titleString = CharacterManager.SelectedCharacter.Name + "'s proficiency with " + weaponType + "s grows";
        ShowSkill(titleString, weaponSkill);
    }

    private void ShowSkill(string titleString, Skill skill)
    {
        string benefitString = skill.Name + " has been unlocked";
        _titleText.SetText(titleString);
        _benefitText.SetText(benefitString);
        _effectText.SetText(skill.Description());
        gameObject.SetActive(true);
    }
}