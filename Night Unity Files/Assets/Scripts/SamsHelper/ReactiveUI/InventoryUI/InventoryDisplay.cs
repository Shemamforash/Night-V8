using System;
using System.Collections.Generic;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using System.Linq;
using Game.Characters;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    //this class should ONLY be for items that extend from InventoryItem
    public class InventoryDisplay : MenuList, IInputListener
    {
        private Inventory _inventory;
        private TextMeshProUGUI _titleText, _capacityText;
        private InventoryDisplay _moveToInventory;
        public EnhancedButton AllButton, WeaponButton, ArmourButton, AccesoryButton, ResourceButton, CharacterButton, CloseButton;
        private List<Action> _tabActions = new List<Action>();
        private int _currentTab = 0;

        public override void Awake()
        {
            base.Awake();
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Inventory Title");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Weight");
            InputHandler.RegisterInputListener(this);
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
            _tabActions.Add(() => SelectTab(ArmourButton, typeof(Armour).ToString()));
            _tabActions.Add(() => SelectTab(AccesoryButton, typeof(Accessory).ToString()));
            _tabActions[_currentTab]();
        }

        private void SelectTab(EnhancedButton button, string itemType)
        {
            button.SetColor(Color.white);
            if (AllButton != button) AllButton.SetColor(new Color(1, 1, 1, 0.4f));
            if (WeaponButton != button) WeaponButton.SetColor(new Color(1, 1, 1, 0.4f));
            if (ArmourButton != button) ArmourButton.SetColor(new Color(1, 1, 1, 0.4f));
            if (AccesoryButton != button) AccesoryButton.SetColor(new Color(1, 1, 1, 0.4f));
            if (ResourceButton != button) ResourceButton.SetColor(new Color(1, 1, 1, 0.4f));
            if (CharacterButton != button) CharacterButton.SetColor(new Color(1, 1, 1, 0.4f));
            List<MyGameObject> filteredItems = _inventory.Contents();
            if (itemType != "")
            {
                filteredItems = _inventory.Contents().Where(item => item.GetType().ToString() == itemType).ToList();
            }
            SetItems(filteredItems);
            if (filteredItems.Count != 0)
            {
                Items[0].PrimaryButton.Button().Select();
            }
            else
            {
                CloseButton?.Button().Select();
            }
        }

        public void SetInventory(Inventory inventory, InventoryDisplay moveToInventory)
        {
            _moveToInventory = moveToInventory;
            _titleText.text = inventory.Name;
            _inventory = inventory;
            SetItems(inventory.SortByType());
//            AllButton.Button().Select();
            SelectTab(AllButton, "");
            Helper.SetReciprocalNavigation(AllButton, Items[0].PrimaryButton);
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = Helper.Round(_inventory.Weight, 1) + " kg";
        }

        public override ViewParent AddItem(MyGameObject item)
        {
            if (!(item is InventoryItem))
            {
                return null;
                throw new Exceptions.InvalidInventoryItemException(item, "InventoryItem");
            }
            if (((InventoryItem) item).Quantity() == 0) return null;
            InventoryUi inventoryItemUi = (InventoryUi) base.AddItem(item);
            if (inventoryItemUi != null && _moveToInventory != null) //item is already added
            {
                inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnClick(() => GetMoveAction(item, 1)());
                inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnHold(GetMoveAction(item, 5), 0.5f);
            }
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
//            InventoryItem foundItem = FindItem(inventoryItem).GetLinkedObject() as InventoryItem;
//            if (foundItem != null && foundItem.Quantity() == 0 && !_inventory.ContainsItem(inventoryItem))
//            {
//                Remove(found);
//                found.Destroy();
//            }
        }

        public override ViewParent RefreshNavigation()
        {
            ViewParent last = base.RefreshNavigation();
            if (CloseButton != null)
            {
                if (last != null)
                {
                    Helper.SetReciprocalNavigation(last.GetNavigationButton(), CloseButton);
                }
                else
                {
                    CloseButton.Button().Select();
                }
            }
            return last;
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis != InputAxis.Horizontal || isHeld || !gameObject.activeInHierarchy) return;
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
    }
}