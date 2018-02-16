using System;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMinigameController : Menu
{
    private GameObject _classList;
    private ClassSkill _skillOne, _skillTwo, _skillThree, _skillFour;
    private Player _chosenCharacter;
    private Weapon _chosenWeapon;
    private AttributePreview _strength, _endurance, _willpower, _perception;
    private EnhancedButton _lastClassButton;
    private readonly List<EnhancedButton> _weaponButtons = new List<EnhancedButton>();
    private readonly List<EnhancedButton> _classButtons = new List<EnhancedButton>();
    private EnhancedButton _lastWeaponButton;
    public Button StartButton;

    private void SetAttributes()
    {
        _strength = new AttributePreview(Helper.FindChildWithName(gameObject, "Strength"));
        _endurance = new AttributePreview(Helper.FindChildWithName(gameObject, "Endurance"));
        _willpower = new AttributePreview(Helper.FindChildWithName(gameObject, "Willpower"));
        _perception = new AttributePreview(Helper.FindChildWithName(gameObject, "Perception"));
    }

    private void UpdateAttributes()
    {
        int strength = (int) _chosenCharacter.Attributes.Strength.CurrentValue();
        _strength.Set(strength, strength * 50 + " health (+50 per strength)");

        int endurance = (int) _chosenCharacter.Attributes.Endurance.CurrentValue();
        _endurance.Set(endurance, _chosenCharacter.CalculateDashCooldown() + " second dash cooldown, " + _chosenCharacter.CalculateSpeed() + "m/s speed");

        int willpower = (int) _chosenCharacter.Attributes.Willpower.CurrentValue();
        float skillRechargeModifier = _chosenCharacter.Attributes.GetSkillRechargeModifier();
        int rechargePercentage = (int) ((1 - skillRechargeModifier) * 100f);
        _willpower.Set(willpower, "-" + rechargePercentage + "% skill recharge time");

        int perception = (int) _chosenCharacter.Attributes.Perception.CurrentValue();
        float gunDamageModifier = (int) ((_chosenCharacter.Attributes.GetGunDamageModifier() - 1) * 100f);
        _perception.Set(perception, "+" + gunDamageModifier + "% weapon damage");
    }

    public void Awake()
    {
        _classList = Helper.FindChildWithName(gameObject, "Classes");
        _skillOne = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 1"));
        _skillTwo = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 2"));
        _skillThree = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 3"));
        _skillFour = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 4"));
        StartButton.onClick.AddListener(() =>
        {
            if (_chosenWeapon != null && _chosenCharacter != null)
            {
                RetryMenu.StartCombat(_chosenCharacter, _chosenWeapon);
            }
        });
        SetAttributes();
        InitialiseClassButtons();
        InitialiseWeaponButtons();
    }

    private void SetWeaponButton(EnhancedButton button)
    {
        if (_lastWeaponButton != null)
        {
            _lastWeaponButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            Helper.FindAllComponentsInChildren<EnhancedText>(_lastWeaponButton.transform).ForEach(text => text.SetColor(Color.white));
        }

        _lastWeaponButton = button;
        _lastWeaponButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
        Helper.FindAllComponentsInChildren<EnhancedText>(_lastWeaponButton.transform).ForEach(text => text.SetColor(Color.black));
    }

    private void InitialiseWeaponButtons()
    {
        foreach (WeaponType t in Enum.GetValues(typeof(WeaponType)))
        {
            EnhancedButton button = Helper.FindChildWithName<EnhancedButton>(gameObject, t.ToString());
            if (t == WeaponType.SMG)
            {
                _classButtons.ForEach(b => b.SetDownNavigation(button));
            }

            Navigation navigation;
            button.AddOnSelectEvent(() =>
            {
                _classButtons.ForEach(b =>
                {
                    navigation = b.Button().navigation;
                    navigation.selectOnDown = button.Button();
                    b.Button().navigation = navigation;
                });
                navigation = StartButton.navigation;
                navigation.selectOnUp = button.Button();
                StartButton.navigation = navigation;
                
                _chosenWeapon = WeaponGenerator.GenerateWeapon(t, 1);
                _skillThree.SetSkill(_chosenWeapon.WeaponSkillOne);
                _skillFour.SetSkill(_chosenWeapon.WeaponSkillTwo);
                SetWeaponButton(button);
            });
            button.SetUpNavigation(_classButtons[0]);
            _weaponButtons.Add(button);
        }
    }

    private void SetClassButton(EnhancedButton button)
    {
        if (_lastClassButton != null)
        {
            _lastClassButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            Helper.FindAllComponentsInChildren<EnhancedText>(_lastClassButton.transform).ForEach(text => text.SetColor(Color.white));
        }

        _lastClassButton = button;
        _lastClassButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
        Helper.FindAllComponentsInChildren<EnhancedText>(_lastClassButton.transform).ForEach(text => text.SetColor(Color.black));
    }
    
    private void InitialiseClassButtons()
    {
        foreach (CharacterClass c in Enum.GetValues(typeof(CharacterClass)))
        {
            GameObject buttonObject = Helper.InstantiateUiObject("Prefabs/Button Small Border", _classList.transform);
            buttonObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            Helper.FindChildWithName<TextMeshProUGUI>(buttonObject, "Text").text = "The " + c;
            EnhancedButton button = buttonObject.GetComponent<EnhancedButton>();
            _classButtons.Add(button);
            button.AddOnSelectEvent(() =>
            {
                _chosenCharacter = PlayerGenerator.GenerateCharacter(c);
                UpdateAttributes();
                _skillOne.SetSkill(_chosenCharacter.CharacterSkillOne);
                _skillTwo.SetSkill(_chosenCharacter.CharacterSkillTwo);
                _weaponButtons.ForEach(wb =>
                {
                    Navigation navigation = wb.Button().navigation;
                    navigation.selectOnUp = button.Button();
                    wb.Button().navigation = navigation;
                });
                SetClassButton(button);
            });
        }

        for (int i = 1; i < _classButtons.Count; ++i)
        {
            _classButtons[i].SetLeftNavigation(_classButtons[i - 1]);
            _classButtons[i - 1].SetRightNavigation(_classButtons[i]);
        }

        _classButtons[0].Button().Select();
    }
    
    private class ClassSkill
    {
        private readonly TextMeshProUGUI _name, _description;

        public ClassSkill(GameObject skillObject)
        {
            _name = Helper.FindChildWithName<TextMeshProUGUI>(skillObject, "Name");
            _description = Helper.FindChildWithName<TextMeshProUGUI>(skillObject, "Description");
        }

        public void SetSkill(Skill skill)
        {
            _name.text = skill.Name;
            _description.text = skill.Description;
        }
    }

    private class AttributePreview
    {
        private readonly TextMeshProUGUI _bonus;
        private UIAttributeMarkerController _markerController;

        public AttributePreview(GameObject attributeObject)
        {
            _bonus = Helper.FindChildWithName<TextMeshProUGUI>(attributeObject, "Text");
            _markerController = Helper.FindChildWithName<UIAttributeMarkerController>(attributeObject, "Marker Container");
        }

        public void Set(int value, string bonusString)
        {
            _markerController.SetValue(value, value);
            _bonus.text = bonusString;
        }
    }

}