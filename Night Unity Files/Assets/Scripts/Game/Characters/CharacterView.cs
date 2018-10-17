using System.Collections.Generic;
using System.Linq;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterView : MonoBehaviour
    {
        private Player _player;
        private ActionProgressController _actionProgress;
        private CharacterBrandUIController _brandUi;
        [HideInInspector] public UIPlayerAccessoryController AccessoryController;
        [HideInInspector] public UIPlayerArmourController ArmourController;
        [HideInInspector] public UIPlayerWeaponController WeaponController;
        private UIAttributeController _attributeController;
        private UIConditionController _thirstController, _hungerController;
        private EnhancedButton _exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton;
        private EnhancedText _exploreText, _craftText;
        private List<EnhancedButton> _buttons;
        private Transform _actionList;
        private bool _actionListActive = true;

        public void SetPlayer(Player player)
        {
            _player = player;
            CacheDetailedViewElements();
            BindUi();
            _player.SetCharacterView(this);
        }

        private void BindUi()
        {
            _attributeController = gameObject.FindChildWithName<UIAttributeController>("Attributes");

            _thirstController = gameObject.FindChildWithName<UIConditionController>("Thirst");
            _hungerController = gameObject.FindChildWithName<UIConditionController>("Hunger");

            _player.TravelAction.SetButton(_exploreButton);
            _player.CraftAction.SetButton(_craftButton);
            _player.ConsumeAction.SetButton(_consumeButton);
            _player.MeditateAction.SetButton(_meditateButton);
            _player.SleepAction.SetButton(_sleepButton);
        }

        public void Update()
        {
            UpdateAttributes();
            _brandUi.UpdateBrands(_player.BrandManager);
            UpdateActionButtons();
            UpdateCurrentAction();
        }

        private void UpdateAttributes()
        {
            _attributeController.UpdateAttributes(_player);
            _thirstController.UpdateThirst(_player);
            _hungerController.UpdateHunger(_player);
        }

        private void UpdateActionButtons()
        {
        }

        private void CacheDetailedViewElements()
        {
            _actionList = gameObject.FindChildWithName<Transform>("Action List");
            _actionProgress = gameObject.FindChildWithName<ActionProgressController>("Current Action");
            _exploreButton = gameObject.FindChildWithName<EnhancedButton>("Explore");
            _exploreText = _exploreButton.GetComponent<EnhancedText>();
            _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
            _craftText = _craftButton.GetComponent<EnhancedText>();
            _consumeButton = gameObject.FindChildWithName<EnhancedButton>("Consume");
            _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
            _sleepButton = gameObject.FindChildWithName<EnhancedButton>("Sleep");
            _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton};
            _exploreButton.SetOnUpAction(() => CharacterManager.SelectPreviousCharacter(!_actionListActive));

            gameObject.FindChildWithName<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = gameObject.FindChildWithName<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(UiGearMenuController.ShowWeaponMenu);
            WeaponController.EnhancedButton.SetOnUpAction(() => CharacterManager.SelectPreviousCharacter(true));
            WeaponController.SetWeapon(_player.EquippedWeapon);

            AccessoryController = gameObject.FindChildWithName<UIPlayerAccessoryController>("Accessory");
            AccessoryController.SetAccessory(_player.EquippedAccessory);
            AccessoryController.EnhancedButton.AddOnClick(UiGearMenuController.ShowAccessoryMenu);

            ArmourController = gameObject.FindChildWithName<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnClick(UiGearMenuController.ShowArmourMenu);
            ArmourController.EnhancedButton.SetOnDownAction(() => CharacterManager.SelectNextCharacter(true));
            _player.ArmourController.SetOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));

            _brandUi = gameObject.FindChildWithName<CharacterBrandUIController>("Brands");
        }

        public void SelectInitial()
        {
            CharacterManager.SelectCharacter(_player);
            if (!_actionListActive) WeaponController.EnhancedButton.Select();
            else _exploreButton.Select();
        }

        public void SelectLast()
        {
            CharacterManager.SelectCharacter(_player);
            if (!_actionListActive) ArmourController.EnhancedButton.Select();
            else _buttons.Last(b => b.enabled).Select();
        }

        //actions[0], weaponui, & consumption toggle navigate to actions[last], accessoryui, & consumption buttons respectively
        //inverse is true to navigate to character below
        //if no character above, all navigate to inventory button
        //if no character below do nothing
        public void RefreshNavigation()
        {
            _craftButton.enabled = Recipe.RecipesAvailable();
            _consumeButton.enabled = Inventory.Consumables().Count > 0;
            _meditateButton.enabled = _player.CanMeditate();
            _sleepButton.enabled = _player.CanSleep();
            List<EnhancedButton> activeButtons = _buttons.FindAll(b => b.enabled);
            for (int i = 0; i < activeButtons.Count; ++i)
            {
                EnhancedButton activeButton = activeButtons[i];
                activeButton.SetOnDownAction(null);
                TextMeshProUGUI textObject = activeButton.GetComponent<TextMeshProUGUI>();
                string text = textObject.text;
                text = text.Replace("<s>", "");
                text = text.Replace("</s>", "");
                textObject.text = text;
                textObject.color = Color.white;
                activeButton.SetDownNavigation(i + 1 < activeButtons.Count ? activeButtons[i + 1] : null);
                if (i != activeButtons.Count - 1) continue;
                activeButton.SetOnDownAction(() => CharacterManager.SelectNextCharacter(!_actionListActive));
            }

            List<EnhancedButton> inactiveButtons = _buttons.FindAll(b => !b.enabled);
            inactiveButtons.ForEach(b =>
            {
                TextMeshProUGUI textObject = b.gameObject.GetComponent<TextMeshProUGUI>();
                string text = textObject.text;
                text = "<s>" + text + "</s>";
                textObject.text = text;
                textObject.color = UiAppearanceController.FadedColour;
            });
        }

        private void UpdateCurrentAction()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            _actionProgress.UpdateCurrentAction(currentState);
            SetActionListActive(currentState is Rest);
        }

        private void SetActionListActive(bool active)
        {
            RefreshNavigation();
            _actionListActive = active;
            _actionList.gameObject.SetActive(active);
        }
    }
}