using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using NUnit.Framework.Internal.Filters;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class UIMinigameController : MonoBehaviour
{
    private GameObject _classList;
    private ClassSkill _skillOne, _skillTwo, _skillThree, _skillFour;
    private Player _chosenCharacter;
    private Weapon _chosenWeapon;

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


    private AttributePreview _strength, _endurance, _willpower, _perception;
    
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
        _endurance.Set(endurance, _chosenCharacter.MovementController.GetDashCooldown() + " second dash cooldown, " + _chosenCharacter.MovementController.GetSpeed() + "m/s speed");
        
        int willpower = (int) _chosenCharacter.Attributes.Willpower.CurrentValue();
        float skillRechargeModifier = _chosenCharacter.Attributes.GetSkillRechargeModifier();
        int rechargePercentage = (int)((1 - skillRechargeModifier) * 100f);
        _willpower.Set(willpower, "-" + rechargePercentage + "% skill recharge time");
        
        int perception = (int) _chosenCharacter.Attributes.Perception.CurrentValue();
        float gunDamageModifier = (int)((_chosenCharacter.Attributes.GetGunDamageModifier() - 1) * 100f);
        _perception.Set(perception, "+" +gunDamageModifier + "% weapon damage");
    }

    public void Awake()
    {
        _classList = Helper.FindChildWithName(gameObject, "Classes");
        _skillOne = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 1"));
        _skillTwo = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 2"));
        _skillThree = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 3"));
        _skillFour = new ClassSkill(Helper.FindChildWithName(gameObject, "Skill 4"));
        SetClassButtons();
        SetAttributes();
        SetWeaponButtons();
    }

    private void SetWeaponButtons()
    {
        foreach (WeaponType t in Enum.GetValues(typeof(WeaponType)))
        {
            EnhancedButton button = Helper.FindChildWithName<EnhancedButton>(gameObject, t.ToString());
            if (t == WeaponType.SMG)
            {
                _classButtons.ForEach(b => b.SetDownNavigation(button));
            }
            button.AddOnSelectEvent(() =>
            {
                _classButtons.ForEach(b => b.SetDownNavigation(button));
                _chosenWeapon = WeaponGenerator.GenerateWeapon(t, false, 1);
                _skillThree.SetSkill(_chosenWeapon.WeaponSkillOne);
                _skillFour.SetSkill(_chosenWeapon.WeaponSkillTwo);
            });
            Debug.Log(_classButtons[0].Button()  +" "  + button.Button());
            button.SetUpNavigation(_classButtons[0]);
        }
    }
    
    private readonly List<EnhancedButton> _classButtons = new List<EnhancedButton>();
    
    private void SetClassButtons()
    {
        foreach (CharacterClass c in Enum.GetValues(typeof(CharacterClass)))
        {
            GameObject buttonObject = Helper.InstantiateUiObject("Prefabs/Button Small Border", _classList.transform);
            Helper.FindChildWithName<TextMeshProUGUI>(buttonObject, "Text").text = "The " + c;
            EnhancedButton button = buttonObject.GetComponent<EnhancedButton>();
            _classButtons.Add(button);
            button.AddOnSelectEvent(() =>
            {
                _chosenCharacter = PlayerGenerator.GenerateCharacter(c);
                UpdateAttributes();
                _skillOne.SetSkill(_chosenCharacter.CharacterSkillOne);
                _skillTwo.SetSkill(_chosenCharacter.CharacterSkillTwo);
            });
        }

        for (int i = 1; i < _classButtons.Count; ++i)
        {
            _classButtons[i].SetLeftNavigation(_classButtons[i - 1]);
            _classButtons[i - 1].SetRightNavigation(_classButtons[i]);
        }

        _classButtons[0].Button().Select();
    }
}