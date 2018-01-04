using Game.World.Region;
using SamsHelper;
using TMPro;
using UnityEngine;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

public class UIExploreMenuController : Menu
{
	private TextMeshProUGUI _titleText;
	private GameObject _regionContainer;

	public void Awake()
	{
		_titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Title");
		_regionContainer = Helper.FindChildWithName(gameObject, "Region Container");
	}

	public void SetRegion(Region region)
	{
		GameObject regionUi = Helper.InstantiateUiObject("Prefabs/RegionExploreUI", _regionContainer.transform);
	}
	
	public void LookAround()
	{
		
	}

	public void ReturnToCamp()
	{
		
	}
}
