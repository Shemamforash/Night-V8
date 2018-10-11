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

        protected MyGameObject(string name, GameObjectType type)
        {
            Name = name;
            Type = type;
            _id = _idCounter;
            ++_idCounter;
        }

        public virtual void Load(XmlNode root)
        {
            _id = root.IntFromNode("Id");
            if (_id > _idCounter) _idCounter = _id + 1;
            Name = root.StringFromNode("Name");
        }

        public int ID()
        {
            return _id;
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            XmlNode itemNode = doc.CreateChild(Type.ToString());
            itemNode.CreateChild("Id", _id);
            itemNode.CreateChild("Name", Name);
            return itemNode;
        }
    }
}