using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper;
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
        private TextMeshProUGUI _currentActionText;
        private Slider _currentActionSliderSimple;
        private GameObject _detailedView;
        private GameObject SimpleView;
        private EnhancedText _brandText;
        [HideInInspector] public UIPlayerAccessoryController AccessoryController;
        [HideInInspector] public UIPlayerArmourController ArmourController;
        [HideInInspector] public UIPlayerWeaponController WeaponController;
        private UIAttributeController _attributeControllerSimple;
        private UIAttributeController _attributeControllerDetailed;
        private UIConditionController _thirstControllerSimple, _hungerControllerSimple;
        private UIConditionController _thirstControllerDetailed, _hungerControllerDetailed;
        private EnhancedButton _exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton;
        private List<EnhancedButton> _buttons;
        private Transform _actionList;
        private TextMeshProUGUI _detailedCurrentActionText;
        private Slider _currentActionSliderDetailed;
        private bool _actionListActive = true;

        public void SetPlayer(Player player)
        {
            _player = player;
            _player.CharacterView = this;
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            SwitchToSimpleView();
            BindUi();
            ((BaseCharacterAction) _player.States.GetCurrentState()).UpdateActionText();
        }

        private void BindUi()
        {
            _attributeControllerSimple = FindInSimpleView<UIAttributeController>("Attributes");
            _attributeControllerDetailed = FindInDetailedView<UIAttributeController>("Attributes");

            _thirstControllerDetailed = FindInDetailedView<UIConditionController>("Thirst");
            _hungerControllerDetailed = FindInDetailedView<UIConditionController>("Hunger");
            _thirstControllerSimple = FindInSimpleView<UIConditionController>("Thirst");
            _hungerControllerSimple = FindInSimpleView<UIConditionController>("Hunger");

            _player.TravelAction.SetButton(_exploreButton);
            _player.CraftAction.SetButton(_craftButton);
            _player.ConsumeAction.SetButton(_consumeButton);
            _player.MeditateAction.SetButton(_meditateButton);
            _player.SleepAction.SetButton(_sleepButton);
        }

        public void Update()
        {
            _attributeControllerSimple.UpdateAttributes(_player);
            _attributeControllerDetailed.UpdateAttributes(_player);
            _thirstControllerDetailed.UpdateThirst(_player);
            _hungerControllerDetailed.UpdateHunger(_player);
            _thirstControllerSimple.UpdateThirst(_player);
            _hungerControllerSimple.UpdateHunger(_player);
            UpdateBrands();
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            FindInSimpleView<TextMeshProUGUI>("Name").text = _player.Name;
            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
            _currentActionSliderSimple = FindInSimpleView<Slider>("Slider");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = transform.Find("Detailed").gameObject;
            _detailedView.SetActive(true);
            _detailedView.SetActive(false);

            _actionList = gameObject.FindChildWithName<Transform>("Action List");
            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("Current Action");
            _currentActionSliderDetailed = FindInDetailedView<Slider>("Slider");
            _exploreButton = FindInDetailedView<EnhancedButton>("Explore");
            _craftButton = FindInDetailedView<EnhancedButton>("Craft");
            _consumeButton = FindInDetailedView<EnhancedButton>("Consume");
            _meditateButton = FindInDetailedView<EnhancedButton>("Meditate");
            _sleepButton = FindInDetailedView<EnhancedButton>("Sleep");
            _buttons = new List<EnhancedButton> {_exploreButton, _craftButton, _consumeButton, _meditateButton, _sleepButton};
            _exploreButton.SetOnUpAction(() => CharacterManager.SelectPreviousCharacter(!_actionListActive));

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = FindInDetailedView<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(() => UiGearMenuController.ShowWeaponMenu(_player));
            WeaponController.EnhancedButton.SetOnUpAction(() => CharacterManager.SelectPreviousCharacter(true));
            WeaponController.SetWeapon(_player.EquippedWeapon);

            AccessoryController = FindInDetailedView<UIPlayerAccessoryController>("Accessory");
            AccessoryController.SetAccessory(_player.EquippedAccessory);
            AccessoryController.EnhancedButton.AddOnClick(() => UiGearMenuController.ShowAccessoryMenu(_player));

            ArmourController = FindInDetailedView<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnClick(() => UiGearMenuController.ShowArmourMenu(_player));
            ArmourController.EnhancedButton.SetOnDownAction(() => CharacterManager.SelectNextCharacter(true));
            ArmourController.SetArmour(_player.ArmourController);
            _player.ArmourController.AddOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));

            _brandText = _detailedView.FindChildWithName<EnhancedText>("Brands");
        }

        private void UpdateBrands()
        {
            string brandString = "";
            List<BrandManager.Brand> brands = _player.BrandManager.GetActiveBrands();
            for (int i = 0; i < brands.Count; i++)
            {
                BrandManager.Brand brand = brands[i];
                brandString += brand.GetProgressString();
                if (i < brands.Count - 1) brandString += "\n";
            }

            _brandText.SetText(brandString);
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
            _consumeButton.enabled = WorldState.HomeInventory().Consumables().Count > 0;
            _meditateButton.enabled = _player.CanMeditate();
            _sleepButton.enabled = _player.CanSleep();
            List<EnhancedButton> activeButtons = _buttons.FindAll(b => b.enabled);
            for (int i = 0; i < activeButtons.Count; ++i)
            {
                EnhancedButton activeButton = activeButtons[i];
                activeButton.SetOnDownAction(null);
                TextMeshProUGUI textObject = activeButton.gameObject.FindChildWithName<TextMeshProUGUI>("Text");
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
                TextMeshProUGUI textObject = b.gameObject.FindChildWithName<TextMeshProUGUI>("Text");
                string text = textObject.text;
                text = "<s>" + text + "</s>";
                textObject.text = text;
                textObject.color = UiAppearanceController.FadedColour;
            });
        }

        public void UpdateCurrentActionText(string currentActionString, float progress)
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            if (currentState == null) return;
            _currentActionText.text = currentActionString;
            if (progress < 0)
            {
                _currentActionSliderSimple.gameObject.SetActive(false);
                _currentActionSliderDetailed.gameObject.SetActive(false);
            }
            else
            {
                _currentActionSliderSimple.gameObject.SetActive(true);
                _currentActionSliderDetailed.gameObject.SetActive(true);
                if (progress == 1)
                {
                    _currentActionSliderDetailed.value = 1;
                    _currentActionSliderDetailed.value = 1;
                }
                else
                {
                    DOTween.To(() => _currentActionSliderSimple.value, f => _currentActionSliderSimple.value = f, progress, WorldState.MinuteInSeconds).SetEase(Ease.Linear);
                    DOTween.To(() => _currentActionSliderDetailed.value, f => _currentActionSliderDetailed.value = f, progress, WorldState.MinuteInSeconds).SetEase(Ease.Linear);
                }
            }

            if (_player.States.IsDefaultState(currentState))
            {
                SetActionListActive(true);
            }
            else
            {
                SetActionListActive(false);
                _detailedCurrentActionText.text = currentActionString;
            }
        }


        private void SetActionListActive(bool active)
        {
            RefreshNavigation();
            _actionListActive = active;
            _actionList.gameObject.SetActive(active);
            _detailedCurrentActionText.gameObject.SetActive(!active);
        }

        private T FindInSimpleView<T>(string elementName) where T : class
        {
            return SimpleView.FindChildWithName<T>(elementName);
        }

        private T FindInDetailedView<T>(string elementName) where T : class
        {
            return _detailedView.FindChildWithName<T>(elementName);
        }

        public void SwitchToDetailedView()
        {
            GetComponent<LayoutElement>().preferredHeight = 200;
            _detailedView.SetActive(true);
            SimpleView.SetActive(false);
            _exploreButton.Select();
        }

        public void SwitchToSimpleView()
        {
            GetComponent<LayoutElement>().preferredHeight = 60;
            _detailedView.SetActive(false);
            SimpleView.SetActive(true);
//            SimpleView.GetComponent<Button>().Select();
        }
    }
}