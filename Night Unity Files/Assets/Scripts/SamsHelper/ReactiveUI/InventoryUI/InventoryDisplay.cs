using System;
using System.Collections.Generic;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using System.Linq;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    //this class should ONLY be for items that extend from InventoryItem
    public class InventoryDisplay : MenuList
    {
        private Inventory _inventory;
        private TextMeshProUGUI _titleText, _capacityText;
        private InventoryDisplay _moveToInventory;
        public EnhancedButton AllButton, WeaponButton, ArmourButton, AccesoryButton, ResourceButton, CharacterButton, CloseButton;

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
            AllButton.AddOnClick(() =>
            {
                SelectTab(AllButton, "");
                SetItems(_inventory.Contents());
            });
            if(WeaponButton != null) WeaponButton.AddOnClick(() => SetItems(SelectTab(WeaponButton, typeof(Weapon).ToString())));
            if(ArmourButton != null) ArmourButton.AddOnClick(() => SetItems(SelectTab(ArmourButton, typeof(Armour).ToString())));
            if(AccesoryButton != null) AccesoryButton.AddOnClick(() => SetItems(SelectTab(AccesoryButton, typeof(Armour).ToString())));
            if(ResourceButton != null) ResourceButton.AddOnClick(() => SetItems(SelectTab(ResourceButton, typeof(Armour).ToString())));
            if(CharacterButton != null) CharacterButton.AddOnClick(() => SetItems(SelectTab(CharacterButton, typeof(Armour).ToString())));
            if(CloseButton != null)CloseButton.AddOnClick(MenuStateMachine.GoToInitialMenu);
        }

        private List<MyGameObject> SelectTab(EnhancedButton button, string itemType)
        {
            button.SetColor(Color.white);
            if(AllButton != button) AllButton.SetColor(new Color(1,1,1,0.4f));
            if(WeaponButton != button) WeaponButton.SetColor(new Color(1,1,1,0.4f));
            if(ArmourButton != button) ArmourButton.SetColor(new Color(1,1,1,0.4f));
            if(AccesoryButton != button) AccesoryButton.SetColor(new Color(1,1,1,0.4f));
            if(ResourceButton != button) ResourceButton.SetColor(new Color(1,1,1,0.4f));
            if(CharacterButton != button) CharacterButton.SetColor(new Color(1,1,1,0.4f));
            return _inventory.Contents().Where(item => item.GetType().ToString() == itemType).ToList();
        }

        public void SetInventory(Inventory inventory, InventoryDisplay moveToInventory)
        {
            _moveToInventory = moveToInventory;
            _titleText.text = inventory.Name;
            _inventory = inventory;
            SetItems(inventory.SortByType());
            AllButton.Button().Select();
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
            if (inventoryItemUi != null) //item is already added
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
            if (last != null)
            {
                Helper.SetReciprocalNavigation(last.GetNavigationButton(), InventoryTransferManager.CloseButton());
            }
            return last;
        }
    }
}