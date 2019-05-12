using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Global;

using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIActionListController : MonoBehaviour
{
	private bool                 _atHome;
	private List<EnhancedButton> _buttons;
	private EnhancedButton       _exploreButton,  _craftButton,  _inventoryButton, _enterGateButton;
	private bool                 _exploreEnabled, _craftEnabled, _sleepEnabled,    _enterGateEnabled;
	private TextMeshProUGUI      _exploreText,    _craftText;
	private Player               _player;
	private bool                 _resting;

	public void Awake()
	{
		_exploreButton   = gameObject.FindChildWithName<EnhancedButton>("Explore");
		_exploreText     = _exploreButton.FindChildWithName<TextMeshProUGUI>("Text");
		_craftButton     = gameObject.FindChildWithName<EnhancedButton>("Craft");
		_craftText       = _craftButton.FindChildWithName<TextMeshProUGUI>("Text");
		_inventoryButton = gameObject.FindChildWithName<EnhancedButton>("Inventory");
		_enterGateButton = gameObject.FindChildWithName<EnhancedButton>("Enter Gate");
		_buttons         = new List<EnhancedButton> {_exploreButton, _craftButton, _inventoryButton};
	}

	public void SetPlayer(Player player)
	{
		_player = player;
		_player.TravelAction.SetButton(_exploreButton);
		_player.CraftAction.SetButton(_craftButton);
		_player.ConsumeAction.SetButton(_inventoryButton);
		_enterGateButton.AddOnClick(GateTransitController.StartTransit);
	}

	public List<EnhancedButton> Buttons() => _buttons;

	public void SelectInitial() => _exploreButton.Select();

	private void UpdateExploreButton()
	{
		bool canStartExploring    = _atHome && _resting;
		bool arrivedAtDestination = _player.TravelAction.AtDestination;
		_exploreEnabled                      = canStartExploring || arrivedAtDestination;
		_exploreButton.Button().interactable = _exploreEnabled;
		_exploreText.alpha                   = _exploreEnabled ? 1f : 0.4f;
		_exploreText.text                    = arrivedAtDestination ? "Enter" : "Explore";
	}

	private void UpdateCraftButton()
	{
		_craftEnabled                      = Recipe.RecipesAvailable() && _atHome && _resting;
		_craftButton.Button().interactable = _craftEnabled;
		_craftText.alpha                   = _craftEnabled ? 1f : 0.4f;
	}

	private void UpdateInventoryButton()
	{
		_inventoryButton.gameObject.SetActive(true);
	}

	private void UpdateEnterGateAction()
	{
		_enterGateEnabled = _resting
		                 && _atHome
		                 && _player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer
		                 && WorldState.AllTemplesActive();
		_enterGateButton.gameObject.SetActive(_enterGateEnabled);
	}

	public bool UpdateList()
	{
		_atHome  = _player.TravelAction.AtHome();
		_resting = _player.States.GetCurrentState() is Rest;
		UpdateExploreButton();
		UpdateCraftButton();
		UpdateInventoryButton();
		UpdateEnterGateAction();
		return CheckIfButtonNeedsSelecting();
	}

	private bool CheckIfButtonNeedsSelecting()
	{
		if (TutorialManager.Instance.IsTutorialVisible()) return false;
		if (_player != CharacterManager.SelectedCharacter) return false;
		GameObject currentObject = EventSystem.current.currentSelectedGameObject;
		if (currentObject != null && currentObject.activeInHierarchy && currentObject.GetComponent<Selectable>().interactable) return false;
		EnhancedButton firstButton = _buttons.FirstOrDefault(b => b.gameObject.activeInHierarchy && b.Button().interactable);
		if (firstButton == null) return true;
		firstButton.Select();
		return false;
	}
}