using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryManager : MonoBehaviour
    {
        public Transform InventoryAParent, InventoryBParent;
        private static InventoryContainer _originInventoryContainer, _targetInventoryContainer;
        private JourneyToLocation _characterLocationState;

        public void ExitManager()
        {
            _characterLocationState.Exit();
        }

        public class InventoryContainer
        {
            private Inventory _inventory;
            private InventoryContainer _otherInventoryContainer;
            private readonly GameObject _itemPrefab;
            private readonly Transform _parent;
            private List<ItemUIObject> _items = new List<ItemUIObject>();

            private class ItemUIObject
            {
                public GameObject GameObject;
                public InventoryItem Item;

                public ItemUIObject(GameObject gameObject, InventoryItem item)
                {
                    GameObject = gameObject;
                    Item = item;
                }
            }

            public InventoryContainer(GameObject itemPrefab, Transform parent)
            {
                _itemPrefab = itemPrefab;
                _parent = parent;
            }

            private ItemUIObject GetItemIfInInventory(InventoryItem item)
            {
                return _items.FirstOrDefault(i => item == i.Item);
            }

            private ItemUIObject CreateNewUiObject(InventoryItem item)
            {
                GameObject newItemObject = Instantiate(_itemPrefab);
                newItemObject.transform.SetParent(_parent);
                newItemObject.transform.localScale = new Vector3(1, 1, 1);
                ItemUIObject newItemUi = new ItemUIObject(newItemObject, item);
                Button b = Helper.FindChildWithName<Button>(newItemObject, "Move");
                b.onClick.AddListener(delegate
                {
                    InventoryItem transferredItem = _inventory.MoveItem(item, _otherInventoryContainer._inventory);
                    if (transferredItem != null)
                    {
                        _otherInventoryContainer.AddItem(transferredItem);
                    }
                    UpdateItemObject(newItemUi);
                });
                return newItemUi;
            }

            private void AddItem(InventoryItem item)
            {
                ItemUIObject found = GetItemIfInInventory(item);
                if (found == null)
                {
                    found = CreateNewUiObject(item);
                    _items.Add(found);
                }
                UpdateItemObject(found);
            }

            private void UpdateItemObject(ItemUIObject itemObject)
            {
                InventoryItem item = itemObject.Item;
                if (item == null || item.Quantity() == 0)
                {
                    Destroy(itemObject.GameObject);
                    _items.Remove(itemObject);
                }
                else
                {
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Weight").text = item.GetTotalWeight() + " W";
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Name").text = item.Name;
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Amount").text = item.Quantity().ToString();
                }
            }

            public void SetInventory(Inventory inventory, InventoryContainer otherInventoryContainer)
            {
                Clear();
                _inventory = inventory;
                _otherInventoryContainer = otherInventoryContainer;
                _inventory.Items().ForEach(AddItem);
            }

            public Inventory GetInventory()
            {
                return _inventory;
            }

            private void Clear()
            {
                _items.ForEach(i => Destroy(i.GameObject));
                _items = new List<ItemUIObject>();
            }
        }

        public void Awake()
        {
            _originInventoryContainer = new InventoryContainer(Resources.Load<GameObject>("Prefabs/RightItem"), InventoryAParent);
            _targetInventoryContainer = new InventoryContainer(Resources.Load<GameObject>("Prefabs/LeftItem"), InventoryBParent);
            Helper.FindChildWithName<Button>(gameObject, "Confirm").onClick.AddListener(ExitManager);
        }

        public void SetInventories(Inventory inventoryOrigin, Inventory inventoryTarget, JourneyToLocation characterLocationState)
        {
            _originInventoryContainer.SetInventory(inventoryOrigin, _targetInventoryContainer);
            _targetInventoryContainer.SetInventory(inventoryTarget, _originInventoryContainer);
            _characterLocationState = characterLocationState;
        }
    }
}