using System.Collections.Generic;
using Game.Characters;
using Game.World.Region;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

public class UIExploreMenuController : Menu
{
	private TextMeshProUGUI _titleText;
	private GameObject _regionContainer;
	private readonly List<GameObject> _regionUIList = new List<GameObject>();
	private GameObject _lookAroundButton;
	private Player _player;
	private Region _currentRegion;
	private static UIExploreMenuController _instance;

	public void Awake()
	{
		_titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Title");
		_regionContainer = Helper.FindChildWithName(gameObject, "Region Container");
		_lookAroundButton = Helper.FindChildWithName(gameObject, "Look");
		_instance = this;
	}

	public void SetRegion(Region region, Player player)
	{
		_player = player;
		_currentRegion = region;
		bool isInitialRegion = _currentRegion.Origin == null;
		_lookAroundButton.SetActive(!isInitialRegion);
		_titleText.text = isInitialRegion ? "Setting Out" : _currentRegion.Name;
		foreach (Region connection in region.Connections)
		{
			GameObject regionUi = Helper.InstantiateUiObject("Prefabs/Region Explore UI", _regionContainer.transform);
			UIRegionItem regionUiController = regionUi.GetComponent<UIRegionItem>();
			regionUiController.SetText("left text", connection.Name, "right text");
			regionUiController.SetRegion(connection, _player, ReturnToCamp);
			_regionUIList.Add(regionUi);
		}
		_regionUIList[0].GetComponent<Button>().Select();
		MenuStateMachine.States.NavigateToState("Region Explore Menu");
	}

	public static UIExploreMenuController Instance()
	{
		return _instance;
	}
	
	public void LookAround()
	{
		_currentRegion?.Enter(_player);
	}

	public void ReturnToCamp()
	{
		_regionUIList.ForEach(Destroy);
		_regionUIList.Clear();
		MenuStateMachine.GoToInitialMenu();
		_player.States.NavigateToState("Return");
	}
}
