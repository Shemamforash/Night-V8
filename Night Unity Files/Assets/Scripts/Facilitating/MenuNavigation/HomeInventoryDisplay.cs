using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.MenuNavigation
{
    public class HomeInventoryDisplay : InventoryDisplay
    {
        private GameObject InventoryObject;
        private static HomeInventoryDisplay _instance;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            TitleText.text = "Vehicle Inventory";
        }

        protected override void Close()
        {
            MenuStateMachine.Instance().NavigateToState("Game Menu");
        }

        public static HomeInventoryDisplay Instance()
        {
            return _instance ?? (_instance = FindObjectOfType<HomeInventoryDisplay>());
        }

        public GameObject GetInventoryObject()
        {
            if (InventoryObject != null)
            {
                return InventoryObject;
            }
            InventoryObject = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Inventory");
            InventoryObject.GetComponent<Button>().onClick.AddListener(OpenMenu);
            return InventoryObject;
        }

        private void OpenMenu()
        {
            MenuStateMachine.Instance().NavigateToState("Home Inventory Menu");
            SetInventory(WorldState.Inventory());
            CapacityText.text = WorldState.Inventory().GetInventoryWeight() + " W";
        }

        protected override GameObject AddItem(BasicInventoryContents inventoryItem)
        {
            GameObject itemUi = base.AddItem(inventoryItem);
            Helper.FindChildWithName<TextMeshProUGUI>(itemUi, "Name").text = inventoryItem.Name();
            Helper.FindChildWithName<TextMeshProUGUI>(itemUi, "Weight").text = inventoryItem.Quantity().ToString();
            return itemUi;
        }
    }
}