using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Exploration.Regions;
using Game.Global.Tutorial;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

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
        private CanvasGroup _viewCanvas;
        private UIActionListController _actionList;
        private List<TutorialOverlay> _overlays;
        private List<TutorialOverlay> _sleepingOverlays;

        public void SetPlayer(Player player)
        {
            _player = player;
            if (_player == null)
            {
                gameObject.SetActive(false);
                return;
            }

            CacheElements();
            BindUi();
            _player.SetCharacterView(this);
        }

        private void BindUi()
        {
            _attributeController = gameObject.FindChildWithName<UIAttributeController>("Attributes");

            _thirstController = gameObject.FindChildWithName<UIConditionController>("Thirst");
            _hungerController = gameObject.FindChildWithName<UIConditionController>("Hunger");

            _actionList.SetPlayer(_player);

            RectTransform physical = _attributeController.FindChildWithName<RectTransform>("Physical");
            RectTransform mental = _attributeController.FindChildWithName<RectTransform>("Mental");

            _sleepingOverlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_actionList.SleepRect()),
                new TutorialOverlay(gameObject.FindChildWithName("Conditions").GetComponent<RectTransform>())
            };
            
            _overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_attributeController.GetComponent<RectTransform>()),
                new TutorialOverlay(physical),
                new TutorialOverlay(mental),
            };
        }

        public void Update()
        {
            if (_player == null) return;
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
            _actionList = gameObject.FindChildWithName<UIActionListController>("Action List");

            _actionList.Buttons().ForEach(b => b.AddOnSelectEvent(SelectCharacter));

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

        public void SelectInitial() => _actionList.SelectInitial();

        private void UpdateCurrentAction()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            _actionProgress.UpdateCurrentAction(currentState);
            bool selectWeapon = _actionList.UpdateList();
            if (selectWeapon) WeaponController.EnhancedButton.Select();
        }

        public void ShowTutorial()
        {
            TutorialManager.TryOpenTutorial(18, _sleepingOverlays);
            if (Region.InTutorialPeriod()) return;
            TutorialManager.TryOpenTutorial(9, _overlays);
        }
    }
}