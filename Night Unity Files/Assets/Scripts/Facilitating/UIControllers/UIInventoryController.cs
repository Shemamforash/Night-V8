using System;
using System.Linq;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine.UI;

namespace UIControllers
{
	public class UIInventoryController : Menu
	{
		private static InventoryDisplay _inventoryDisplay;
		private static Button _closeButton;

		public void Awake()
		{
			_closeButton = Helper.FindChildWithName<Button>(gameObject, "Close Menu");
			_inventoryDisplay = GetComponent<InventoryDisplay>();
		}
	
		public void Start ()
		{
			_closeButton.onClick.AddListener(MenuStateMachine.GoToInitialMenu);
		}

		public static void SetInventoryTitle(string title)
		{
			_inventoryDisplay.SetTitleText(title);
		}
		
		public static void ShowInventory(DesolationInventory inventory, Action<MyGameObject> callback = null)
		{
			MenuStateMachine.States.NavigateToState("Inventory Menu");
			_inventoryDisplay.SetInventory(inventory, null);
			_inventoryDisplay.Items.ForEach(item =>
			{
				item.PrimaryButton.AddOnClick(() =>
				{
					callback?.Invoke(item.GetLinkedObject());
				});
			});
			Helper.SetReciprocalNavigation(_inventoryDisplay.Items.Last().PrimaryButton.Button(), _closeButton);
		}
	}
}