using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using SamsHelper;
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
        private Transform _actionList;
        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;
        private GameObject _detailedView;
        private GameObject SimpleView;
        public UIPlayerAccessoryController AccessoryController;
        public UIPlayerArmourController ArmourController;
        public UIPlayerWeaponController WeaponController;
        private UIAttributeController _attributeControllerSimple;
        private UIAttributeController _attributeControllerDetailed;
        private UIConditionController _thirstControllerSimple, _hungerControllerSimple;
        private UIConditionController _thirstControllerDetailed, _hungerControllerDetailed;

        public void SetPlayer(Player player)
        {
            _player = player;
            _player.CharacterView = this;
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            SwitchToSimpleView();
            BindUi();
            FillActionList();
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
        }

        public void Update()
        {
            _attributeControllerSimple.UpdateAttributes(_player);
            _attributeControllerDetailed.UpdateAttributes(_player);
            _thirstControllerDetailed.UpdateDehydration(_player);
            _hungerControllerDetailed.UpdateHunger(_player);
            _thirstControllerSimple.UpdateDehydration(_player);
            _hungerControllerSimple.UpdateHunger(_player);
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            FindInSimpleView<TextMeshProUGUI>("Name").text = _player.Name;
            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = transform.Find("Detailed").gameObject;
            _detailedView.SetActive(true);
            _detailedView.SetActive(false);

            _actionList = FindInDetailedView<Transform>("Action List");
            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = FindInDetailedView<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowWeaponMenu(_player));
            WeaponController.SetWeapon(_player.Weapon);

            ArmourController = FindInDetailedView<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowArmourMenu(_player));
            ArmourController.SetArmour(_player.ArmourController);
            _player.ArmourController.AddOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));

            AccessoryController = FindInDetailedView<UIPlayerAccessoryController>("Accessory");
            AccessoryController.SetAccessory(_player.Accessory);
            AccessoryController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowAccessoryMenu(_player));
        }

        private EnhancedButton _exploreButton, _craftButton;
        
        private void FillActionList()
        {
            _exploreButton = Helper.FindChildWithName<EnhancedButton>(_actionList.gameObject, "Explore");
            _craftButton = Helper.FindChildWithName<EnhancedButton>(_actionList.gameObject, "Craft");
            _player.TravelAction.SetButton(_exploreButton);
            _player.CraftAction.SetButton(_craftButton);
            Helper.SetNavigation(_exploreButton, WeaponController.EnhancedButton, Direction.Left);
            Helper.SetNavigation(_craftButton, ArmourController.EnhancedButton, Direction.Left);
            Helper.SetNavigation(WeaponController.EnhancedButton, _exploreButton, Direction.Right);
            Helper.SetNavigation(ArmourController.EnhancedButton.Button(), _exploreButton, Direction.Right);
            Helper.SetNavigation(AccessoryController.EnhancedButton.Button(), _exploreButton, Direction.Right);
        }

        public void SelectInitial()
        {
            CharacterManager.SelectCharacter(_player);
            if (_actionListActive) _exploreButton.Button().Select();
            else WeaponController.EnhancedButton.Button().Select();
        }

        //actions[0], weaponui, & consumption toggle navigate to actions[last], accessoryui, & consumption buttons respectively
        //inverse is true to navigate to character below
        //if no character above, all navigate to inventory button
        //if no character below do nothing
        public void RefreshNavigation()
        {
            CharacterView _previousCharacterView = CharacterManager.PreviousCharacter(_player)?.CharacterView;

            if (_previousCharacterView == null) return;

            _previousCharacterView._craftButton.SetOnDownAction(() =>
            {
                CharacterManager.ExitCharacter(_previousCharacterView._player);
                CharacterManager.SelectCharacter(_player);
                if (_actionListActive) _exploreButton.Button().Select();
                else WeaponController.EnhancedButton.Button().Select();
            });
            _exploreButton.SetOnUpAction(() =>
            {
                CharacterManager.ExitCharacter(_player);
                CharacterManager.SelectCharacter(_previousCharacterView._player);
                if (_previousCharacterView._actionListActive) _previousCharacterView._craftButton.Button().Select();
                else _previousCharacterView.ArmourController.EnhancedButton.Button().Select();
            });
            WeaponController.EnhancedButton.SetOnUpAction(() =>
            {
                CharacterManager.ExitCharacter(_player);
                CharacterManager.SelectCharacter(_previousCharacterView._player);
                _previousCharacterView.ArmourController.EnhancedButton.Button().Select();
            });
            _previousCharacterView.ArmourController.EnhancedButton.SetOnDownAction(() =>
            {
                CharacterManager.ExitCharacter(_previousCharacterView._player);
                CharacterManager.SelectCharacter(_player);
                WeaponController.EnhancedButton.Button().Select();
            });
        }

        public void UpdateCurrentActionText(string currentActionString)
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            if (currentState == null) return;
            _currentActionText.text = currentActionString;
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

        private bool _actionListActive = true;

        private void SetActionListActive(bool active)
        {
            RefreshNavigation();
            _actionListActive = active;
            _actionList.gameObject.SetActive(active);
            _detailedCurrentActionText.gameObject.SetActive(!active);
        }

        private T FindInSimpleView<T>(string elementName) where T : class
        {
            return Helper.FindChildWithName<T>(SimpleView, elementName);
        }

        private T FindInDetailedView<T>(string elementName) where T : class
        {
            return Helper.FindChildWithName<T>(_detailedView, elementName);
        }

        public void SwitchToDetailedView()
        {
            GetComponent<LayoutElement>().preferredHeight = 200;
            _detailedView.SetActive(true);
            SimpleView.SetActive(false);
            _actionList.GetChild(0).GetComponent<EnhancedButton>().Button().Select();
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