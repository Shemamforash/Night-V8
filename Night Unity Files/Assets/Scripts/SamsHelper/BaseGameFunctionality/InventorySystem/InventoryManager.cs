using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryManager : MonoBehaviour
    {
        public Transform InventoryAParent, InventoryBParent;
        private static InventoryContainer _originInventory, _targetInventory;

        public class InventoryContainer
        {
            private Inventory _inventory;
            private readonly GameObject _itemPrefab;
            private readonly Transform _parent;
            private Dictionary<GameObject, InventoryItem> _items = new Dictionary<GameObject, InventoryItem>();

            public InventoryContainer(GameObject itemPrefab, Transform parent)
            {
                _itemPrefab = itemPrefab;
                _parent = parent;
            }

            private void AddItem(InventoryItem item)
            {
                GameObject newItemObject = Instantiate(_itemPrefab);
                newItemObject.transform.SetParent(_parent);
                newItemObject.transform.localScale = new Vector3(1, 1, 1);
                Button b = Helper.FindChildWithName<Button>(newItemObject, "Move");
                b.onClick.AddListener(delegate
                {
                    MoveItem(this, item);
                    UpdateItemObject(newItemObject);
                });
                _items[newItemObject] = item;
            }

            private void UpdateItemObject(GameObject itemObject)
            {
                InventoryItem item = _items[itemObject];
                if (item.Quantity() == 0)
                {
                    
                }
                Helper.FindChildWithName<Text>(itemObject, "Weight").text = item.GetTotalWeight() + " W";
                Helper.FindChildWithName<Text>(itemObject, "Name").text = item.Name;
                Helper.FindChildWithName<Text>(itemObject, "Amount").text = item.Quantity().ToString();
            }

            public void SetInventory(Inventory inventory)
            {
                Clear();
                _inventory = inventory;
                _inventory.Items().ForEach(AddItem);
            }

            public Inventory GetInventory()
            {
                return _inventory;
            }

            private void Clear()
            {
                foreach (GameObject g in _items.Keys)
                {
                    Destroy(g);
                }
                _items = new Dictionary<GameObject, InventoryItem>();
            }
        }

        public static void MoveItem(InventoryContainer container, InventoryItem item)
        {
            if (container == _originInventory)
            {
                _originInventory.GetInventory().MoveItem(item, _targetInventory.GetInventory());
            }
            else
            {
                _targetInventory.GetInventory().MoveItem(item, _originInventory.GetInventory());
            }
        }

        public void Awake()
        {
            _originInventory = new InventoryContainer(Resources.Load<GameObject>("Prefabs/RightItem"), InventoryAParent);
            _targetInventory = new InventoryContainer(Resources.Load<GameObject>("Prefabs/LeftItem"), InventoryBParent);
        }

        public void SetInventorys(Inventory inventoryOrigin, Inventory inventoryTarget)
        {
            _originInventory.SetInventory(inventoryOrigin);
            _targetInventory.SetInventory(inventoryTarget);
        }
    }
}