using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class MyGameObject
    {
        private static int _idCounter;
        private int _id;
        private readonly GameObjectType Type;
        public string Name;
        private Inventory _parentInventory;
        private int _parentInventoryId;

        protected MyGameObject(string name, GameObjectType type, Inventory parentInventory = null)
        {
            Name = name;
            SetParentInventory(parentInventory);
            Type = type;
            _id = _idCounter;
            ++_idCounter;
        }

        public virtual void Load(XmlNode root)
        {
            _id = root.IntFromNode("Id");
            if (_id > _idCounter) _idCounter = _id + 1;
            Name = root.StringFromNode("Name");
            _parentInventoryId = root.IntFromNode("ParentInventory");
        }

        public int ID()
        {
            return _id;
        }

        public Inventory ParentInventory()
        {
            if (_parentInventory == null && _parentInventoryId != -1)
            {
                _parentInventory = Inventory.FindInventory(_parentInventoryId);
            }

            return _parentInventory;
        }

        public void SetParentInventory(Inventory parentInventory)
        {
            _parentInventory = parentInventory;
            _parentInventoryId = _parentInventory?._id ?? -1;
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            XmlNode itemNode = doc.CreateChild(Type.ToString());
            itemNode.CreateChild("Id", _id);
            itemNode.CreateChild("Name", Name);
            itemNode.CreateChild("ParentInventory", _parentInventoryId);
            return itemNode;
        }
    }
}