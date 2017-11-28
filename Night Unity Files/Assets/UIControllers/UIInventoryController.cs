using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.UI.Elements;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UIControllers
{
	public class UIInventoryController : Menu
	{
		private static EnhancedText _inventoryOwnerText;
		private static ScrollingMenuList _menuList;
		private static Button _closeButton;

		public void Awake()
		{
			_inventoryOwnerText = Helper.FindChildWithName<EnhancedText>(gameObject, "Title");
			_menuList = Helper.FindChildWithName<ScrollingMenuList>(gameObject, "Inventory Contents");
			_closeButton = Helper.FindChildWithName<Button>(gameObject, "Close Menu");
		}
	
		public void Start ()
		{
			_closeButton.onClick.AddListener(MenuStateMachine.GoToInitialMenu);
		}

		public static void ShowInventory(string inventoryOwner, List<MyGameObject> contents, Action<MyGameObject> callback = null)
		{
			MenuStateMachine.States.NavigateToState("Inventory Menu");
			_inventoryOwnerText.Text(inventoryOwner);
			_menuList.SetItems(contents);
			_menuList.Items.ForEach(item =>
			{
				item.PrimaryButton.AddOnClick(() =>
				{
					callback?.Invoke(item.GetLinkedObject());
				});
			});
			Helper.SetReciprocalNavigation(_menuList.Items.Last().PrimaryButton.Button(), _closeButton);
		}

		public static void CloseMenu()
		{
			MenuStateMachine.GoToInitialMenu();
		}
	}
}
