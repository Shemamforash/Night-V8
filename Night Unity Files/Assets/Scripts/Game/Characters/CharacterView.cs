using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        private CanvasGroup _viewCanvas;
        private EnhancedText _exploreText, _craftText;
        private List<EnhancedButton> _buttons;

        public void SetPlayer(Player player)
        {
            _player = player;
            CacheElements();
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
            UpdateCurrentAction();
        }

        private void UpdateAttributes()
        {
            _attributeController.UpdateAttributes(_player);
            _thirstController.UpdateThirst(_player);
            _hungerController.UpdateHunger(_player);
        }

        private void CacheElements()
        {
            _viewCanvas = gameObject.FindChildWithName<CanvasGroup>("Vertical Group");

            _actionProgress = gameObject.FindChildWithName<ActionProgressController>("Current Action");
            _exploreButton = gameObject.FindChildWithName<EnhancedButton>("Explore");
            _exploreText = _exploreButton.GetComponent<EnhancedText>();
            _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
            _craftText = _craftButton.GetComponent<EnhancedText>();
            _consumeButton = gameObject.FindChildWithName<EnhancedButton>("Consume");
            _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
            _sleepButton = gameObject.FindChildWithName<EnhancedButton>("Sleep");
            _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton};
            _buttons.ForEach(b => b.AddOnSelectEvent(SelectCharacter));

            gameObject.FindChildWithName<TextMeshProUGUI>("Character Name").text = _player.Name;

            WeaponController = gameObject.FindChildWithName<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(UiGearMenuController.ShowWeaponMenu);
            WeaponController.EnhancedButton.AddOnSelectEvent(SelectCharacter);
            WeaponController.SetWeapon(_player.EquippedWeapon);

            AccessoryController = gameObject.FindChildWithName<UIPlayerAccessoryController>("Accessory");
            AccessoryController.EnhancedButton.AddOnSelectEvent(SelectCharacter);
            AccessoryController.SetAccessory(_player.EquippedAccessory);
            AccessoryController.EnhancedButton.AddOnClick(UiGearMenuController.ShowAccessoryMenu);

            ArmourController = gameObject.FindChildWithName<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnSelectEvent(SelectCharacter);
            ArmourController.EnhancedButton.AddOnClick(UiGearMenuController.ShowArmourMenu);
            _player.ArmourController.SetOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));

            _brandUi = gameObject.FindChildWithName<CharacterBrandUIController>("Brands");
        }

        private void SelectCharacter()
        {
            CharacterManager.SelectCharacter(_player);
            _viewCanvas.DOFade(1f, 0.3f);
            CharacterManager.Characters.ForEach(c =>
            {
                if (c != _player) c.CharacterView().DeselectCharacter();
            });
        }

        private void DeselectCharacter()
        {
            _viewCanvas.DOFade(0.4f, 0.3f);
        }

        public void SelectInitial()
        {
            _exploreButton.Select();
        }

        public void RefreshNavigation()
        {
            bool atHome = _player.TravelAction.AtHome();
            bool resting = _player.States.GetCurrentState() is Rest;
            _exploreButton.SetEnabled(atHome && resting);
            _craftButton.SetEnabled(Recipe.RecipesAvailable() && atHome && resting);
            _consumeButton.SetEnabled(Inventory.Consumables().Count > 0);
            _meditateButton.SetEnabled(_player.CanMeditate() && atHome && resting);
            _sleepButton.SetEnabled(_player.CanSleep() && atHome && resting);
            _buttons.ForEach(b =>
            {
                TextMeshProUGUI textObject = b.GetComponent<TextMeshProUGUI>();
                textObject.color = b.IsEnabled() ? Color.white : UiAppearanceController.FadedColour;
            });
        }

        private void UpdateCurrentAction()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            _actionProgress.UpdateCurrentAction(currentState);
            RefreshNavigation();
        }
    }
}