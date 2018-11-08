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
using UnityEngine.EventSystems;
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
            _craftButton = gameObject.FindChildWithName<EnhancedButton>("Craft");
            _consumeButton = gameObject.FindChildWithName<EnhancedButton>("Consume");
            _meditateButton = gameObject.FindChildWithName<EnhancedButton>("Meditate");
            _sleepButton = gameObject.FindChildWithName<EnhancedButton>("Sleep");
            _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton};
            _buttons.ForEach(b => b.AddOnSelectEvent(SelectCharacter));

            gameObject.FindChildWithName<TextMeshProUGUI>("Character Name").text = _player.Name;

            WeaponController = gameObject.FindChildWithName<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnSelectEvent(SelectCharacter);
            WeaponController.SetWeapon(_player.EquippedWeapon);

            AccessoryController = gameObject.FindChildWithName<UIPlayerAccessoryController>("Accessory");
            AccessoryController.EnhancedButton.AddOnSelectEvent(SelectCharacter);
            AccessoryController.SetAccessory(_player.EquippedAccessory);

            ArmourController = gameObject.FindChildWithName<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnSelectEvent(SelectCharacter);

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

        private void SetButtonEnabled(EnhancedButton button, bool enableButton)
        {
            if (button.IsEnabled() == enableButton) return;
            button.SetEnabled(enableButton);
            TextMeshProUGUI textObject = button.GetComponent<TextMeshProUGUI>();
            textObject.color = enableButton ? Color.white : UiAppearanceController.FadedColour;
        }

        public void RefreshNavigation()
        {
            bool atHome = _player.TravelAction.AtHome();
            bool resting = _player.States.GetCurrentState() is Rest;
            SetButtonEnabled(_exploreButton, atHome && resting);
            SetButtonEnabled(_craftButton, Recipe.RecipesAvailable() && atHome && resting);
            SetButtonEnabled(_consumeButton, Inventory.Consumables().Count > 0);
            SetButtonEnabled(_meditateButton, _player.CanMeditate() && atHome && resting);
            SetButtonEnabled(_sleepButton, _player.CanSleep() && atHome && resting);
        }

        private void UpdateCurrentAction()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            _actionProgress.UpdateCurrentAction(currentState);
            RefreshNavigation();
        }
    }
}