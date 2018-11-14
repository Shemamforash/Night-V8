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
        private EnhancedText _exploreText, _craftText, _consumeText, _meditateText, _sleepText;
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
            _buttons.ForEach(b => b.AddOnSelectEvent(SelectCharacter));

            gameObject.FindChildWithName<TextMeshProUGUI>("Character Name").text = _player.Name;

            WeaponController = gameObject.FindChildWithName<UIPlayerWeaponController>("Weapon");
            WeaponController.SetWeapon(SelectCharacter, _player);

            AccessoryController = gameObject.FindChildWithName<UIPlayerAccessoryController>("Accessory");
            AccessoryController.SetAccessory(SelectCharacter, _player);

            ArmourController = gameObject.FindChildWithName<UIPlayerArmourController>("Armour");
            ArmourController.SetArmour(SelectCharacter, _player);

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

        private void SetButtonEnabled(EnhancedButton button, EnhancedText text, bool enableButton)
        {
            if (button.IsEnabled() == enableButton) return;
            button.SetEnabled(enableButton);
            text.SetColor(enableButton ? Color.white : UiAppearanceController.FadedColour);
        }

        public void RefreshNavigation()
        {
            bool atHome = _player.TravelAction.AtHome();
            bool resting = _player.States.GetCurrentState() is Rest;
            SetButtonEnabled(_exploreButton, _exploreText, atHome && resting);
            SetButtonEnabled(_craftButton, _craftText, Recipe.RecipesAvailable() && atHome && resting);
            SetButtonEnabled(_consumeButton, _consumeText, Inventory.Consumables().Count > 0);
            SetButtonEnabled(_meditateButton, _meditateText, _player.CanMeditate() && atHome && resting);
            SetButtonEnabled(_sleepButton, _sleepText, _player.CanSleep() && atHome && resting);
        }

        private void UpdateCurrentAction()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            _actionProgress.UpdateCurrentAction(currentState);
            RefreshNavigation();
        }
    }
}