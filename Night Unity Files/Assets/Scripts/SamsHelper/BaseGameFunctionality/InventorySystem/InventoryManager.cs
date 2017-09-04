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
        protected InventoryContainer OriginInventoryContainer, TargetInventoryContainer;
        protected JourneyToLocation CharacterLocationState;
        private Action _onInventorySetAction;

        public void ExitManager()
        {
            CharacterLocationState.Exit();
        }

        public class InventoryContainer
        {
            private Inventory _inventory;
            private InventoryContainer _otherInventoryContainer;
            private readonly GameObject _itemPrefab;
            private readonly Transform _parent;
            private List<ItemUIObject> _items = new List<ItemUIObject>();
            private Action _onInventoryMoveAction;

            private class ItemUIObject
            {
                public GameObject GameObject;
                public BasicInventoryContents Item;

                public ItemUIObject(GameObject gameObject, BasicInventoryContents item)
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

            private ItemUIObject GetItemIfInInventory(BasicInventoryContents item)
            {
                return _items.FirstOrDefault(i => item.Name() == i.Item.Name());
            }

            private ItemUIObject CreateNewUiObject(BasicInventoryContents item)
            {
                GameObject newItemObject = Instantiate(_itemPrefab);
                newItemObject.transform.SetParent(_parent);
                newItemObject.transform.localScale = new Vector3(1, 1, 1);
                ItemUIObject newItemUi = new ItemUIObject(newItemObject, item);
                Button b = Helper.FindChildWithName<Button>(newItemObject, "Move");
                b.onClick.AddListener(delegate
                {
                    BasicInventoryContents transferredItem = _inventory.Move(item, _otherInventoryContainer._inventory);
                    if (transferredItem != null)
                    {
                        _otherInventoryContainer.AddItem(transferredItem);
                    }
                    UpdateItemObject(newItemUi);
                });
                return newItemUi;
            }

            public void SetOnInventoryMoveAction(Action inventoryMoveAction)
            {
                _onInventoryMoveAction = inventoryMoveAction;
            }

            private void AddItem(BasicInventoryContents item)
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
                BasicInventoryContents item = itemObject.Item;
                if (item.Quantity() == 0 || !_inventory.ContainsItem(item))
                {
                    Destroy(itemObject.GameObject);
                    _items.Remove(itemObject);
                }
                else
                {
                    InventoryResource resource = item as InventoryResource;
                    float weight = resource != null ? resource.GetTotalWeight() : item.Weight();
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Weight").text = weight + " W";
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Name").text = item.Name();
                    InventoryResource inventoryResource = item as InventoryResource;
                    float amount = inventoryResource != null ? inventoryResource.Quantity() : 1;
                    Helper.FindChildWithName<Text>(itemObject.GameObject, "Amount").text = Helper.Round(amount, 1).ToString();
                }
                if (_onInventoryMoveAction != null)
                {
                    _onInventoryMoveAction();
                }
            }

            public void SetInventory(Inventory inventory, InventoryContainer otherInventoryContainer)
            {
                Clear();
                _inventory = inventory;
                _otherInventoryContainer = otherInventoryContainer;
                _inventory.Contents().ForEach(AddItem);
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

        public virtual void Awake()
        {
            OriginInventoryContainer = new InventoryContainer(Resources.Load<GameObject>("Prefabs/RightItem"), InventoryAParent);
            TargetInventoryContainer = new InventoryContainer(Resources.Load<GameObject>("Prefabs/LeftItem"), InventoryBParent);
            Helper.FindChildWithName<Button>(gameObject, "Confirm").onClick.AddListener(ExitManager);
        }

        public virtual void SetInventories(Inventory inventoryOrigin, Inventory inventoryTarget, JourneyToLocation characterLocationState)
        {
            OriginInventoryContainer.SetInventory(inventoryOrigin, TargetInventoryContainer);
            TargetInventoryContainer.SetInventory(inventoryTarget, OriginInventoryContainer);
            CharacterLocationState = characterLocationState;
            if (_onInventorySetAction != null)
            {
                _onInventorySetAction();
            }
        }

        protected void OnInventorySetAction(Action inventorySetAction)
        {
            _onInventorySetAction = inventorySetAction;
        }
    }
}