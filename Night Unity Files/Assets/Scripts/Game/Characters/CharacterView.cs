using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Global.Tutorial;
using Extensions;
using TMPro;
using UnityEngine;

namespace Game.Characters
{
	public class CharacterView : MonoBehaviour
	{
		private                  UIActionListController      _actionList;
		private                  ActionProgressController    _actionProgress;
		private                  UIAttributeController       _attributeController;
		private                  CharacterBrandUIController  _brandUi;
		private                  Player                      _player;
		private                  bool                        _seenTutorial;
		private                  CanvasGroup                 _viewCanvas;
		[HideInInspector] public UIPlayerAccessoryController AccessoryController;
		[HideInInspector] public UIPlayerArmourController    ArmourController;
		[HideInInspector] public UIPlayerWeaponController    WeaponController;

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
			_actionList.SetPlayer(_player);
		}

		public void Update()
		{
			if (_player == null) return;
			UpdateAttributes();
			_brandUi.UpdateBrands(_player.BrandManager);
			UpdateCurrentAction();
			ArmourController.gameObject.SetActive(UiArmourUpgradeController.Instance().Unlocked());
			AccessoryController.gameObject.SetActive(UiAccessoryController.Instance().Unlocked());
		}

		private void UpdateAttributes()
		{
			_attributeController.UpdateAttributes(_player);
		}

		private void CacheElements()
		{
			_viewCanvas = gameObject.FindChildWithName<CanvasGroup>("Vertical Group");

			_actionProgress = gameObject.FindChildWithName<ActionProgressController>("Current Action");
			_actionList     = gameObject.FindChildWithName<UIActionListController>("Action List");

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
			bool isWanderer            = _player                             == CharacterManager.Wanderer;
			bool hasAlternateCharacter = CharacterManager.AlternateCharacter != null;
			if (isWanderer && hasAlternateCharacter)
			{
				CharacterManager.AlternateCharacter.CharacterView().DeselectCharacter();
			}
			else if (!isWanderer) CharacterManager.Wanderer.CharacterView().DeselectCharacter();
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
			UpdateActionList();
		}

		public void UpdateActionList()
		{
			bool selectWeapon = _actionList.UpdateList();
			if (selectWeapon) WeaponController.EnhancedButton.Select();
		}

		public void ShowAttributeTutorial()
		{
			if (_seenTutorial || !TutorialManager.Active()) return;
			if (MapGenerator.DiscoveredRegions().Count < 2) return;
			RectTransform physical = _attributeController.FindChildWithName<RectTransform>("Life");
			RectTransform mental   = _attributeController.FindChildWithName<RectTransform>("Will");
			List<TutorialOverlay> overlays = new List<TutorialOverlay>
			{
				new TutorialOverlay(_attributeController.GetComponent<RectTransform>()),
				new TutorialOverlay(physical),
				new TutorialOverlay(mental)
			};
			TutorialManager.Instance.TryOpenTutorial(10, overlays);
			_seenTutorial = true;
		}
	}
}