using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryDisplay : MenuList, IInputListener
    {
        private readonly List<Action> _tabActions = new List<Action>();
        private int _currentTab;
        private Inventory _inventory;
        private InventoryDisplay _moveToInventory;
        private bool _selected;
        private TextMeshProUGUI _titleText, _capacityText;
        public EnhancedButton AllButton, WeaponButton, ArmourButton, AccesoryButton, ResourceButton, CharacterButton;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis != InputAxis.SwitchTab || isHeld || !gameObject.activeInHierarchy || !_selected) return;
            if (direction < 0)
            {
                --_currentTab;
                if (_currentTab < 0)
                {
                    _currentTab = 0;
                    return;
                }

                _tabActions[_currentTab]();
            }
            else if (direction > 0)
            {
                ++_currentTab;
                if (_currentTab == _tabActions.Count)
                {
                    _currentTab = _tabActions.Count - 1;
                    return;
                }

                _tabActions[_currentTab]();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public override void Awake()
        {
            base.Awake();
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Inventory Title");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Weight");
        }

        public void SetTitleText(string titleText)
        {
            _titleText.text = titleText;
        }

        public void Start()
        {
            _tabActions.Add(() => SelectTab(AllButton, ""));
            _tabActions.Add(() => SelectTab(CharacterButton, typeof(Character).ToString()));
            _tabActions.Add(() => SelectTab(ResourceButton, typeof(InventoryResource).ToString()));
            _tabActions.Add(() => SelectTab(WeaponButton, typeof(Weapon).ToString()));
            _tabActions.Add(() => SelectTab(ArmourButton, typeof(ArmourPlate).ToString()));
            _tabActions.Add(() => SelectTab(AccesoryButton, typeof(Accessory).ToString()));
            _tabActions[_currentTab]();
        }

        private void SelectTab(EnhancedButton button, string itemType)
        {
            button.GetComponent<CanvasGroup>().alpha = 1;
            if (AllButton != button) AllButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            if (WeaponButton != button) WeaponButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            if (ArmourButton != button) ArmourButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            if (AccesoryButton != button) AccesoryButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            if (ResourceButton != button) ResourceButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            if (CharacterButton != button) CharacterButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            List<MyGameObject> filteredItems = _inventory.Contents();
            if (itemType != "") filteredItems = _inventory.Contents().Where(item => item.GetType().ToString() == itemType).ToList();
            SetItems(filteredItems);
        }

        public void SetInventory(Inventory inventory, InventoryDisplay moveToInventory, EnhancedButton closeButton = null)
        {
            InputHandler.SetCurrentListener(this);
            if (closeButton != null) CloseButton = closeButton;
            _moveToInventory = moveToInventory;
            if (moveToInventory == null) _selected = true;
            _titleText.text = inventory.Name;
            _inventory = inventory;
            SelectTab(AllButton, "");
            Helper.SetReciprocalNavigation(AllButton, Items[0].PrimaryButton);
            UpdateInventoryWeight();
        }

        private void UpdateInventoryWeight()
        {
            string capacityString = "";
            if (!_inventory.IsBottomless()) capacityString = Helper.Round(_inventory.Weight, 1) + "/" + Helper.Round(_inventory.MaxWeight, 1) + " kg";
            _capacityText.text = capacityString;
        }

        public override ViewParent AddItem(MyGameObject item)
        {
            if (!(item is InventoryItem)) return null;
            if (((InventoryItem) item).Quantity() == 0) return null;
            ViewParent existingUi = FindItem(item);
            if (existingUi != null)
            {
                UpdateItem(item);
                return existingUi;
            }

            InventoryUi inventoryItemUi = (InventoryUi) base.AddItem(item);
            if (_moveToInventory != null && inventoryItemUi != null)
            {
                inventoryItemUi.PrimaryButton.AddOnClick(() => GetMoveAction(item, 1)());
                inventoryItemUi.PrimaryButton.AddOnHold(GetMoveAction(item, 5), 0.5f);
                inventoryItemUi.PrimaryButton.AddOnSelectEvent(() =>
                {
                    _selected = true;
                    _moveToInventory.Deselect();
                });
            }

            UpdateInventoryWeight();
            return inventoryItemUi;
        }

        private Action GetMoveAction(MyGameObject inventoryItem, int quantity)
        {
            return () =>
            {
                MyGameObject transferredItem = _inventory.Move(inventoryItem, _moveToInventory._inventory, quantity);
                if (transferredItem == null) return;
                _moveToInventory.AddItem(transferredItem);
                UpdateItem(inventoryItem);
            };
        }

        protected override void UpdateItem(MyGameObject inventoryItem)
        {
            base.UpdateItem(inventoryItem);
            UpdateInventoryWeight();
            ViewParent itemUi = FindItem(inventoryItem);
            InventoryItem foundItem = itemUi.GetLinkedObject() as InventoryItem;
            if (foundItem == null || _inventory.ContainsItem(inventoryItem)) return;
            Remove(itemUi);
            itemUi.Destroy();
        }

        private void Deselect()
        {
            _selected = false;
        }
    }
}