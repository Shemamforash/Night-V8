﻿using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class MyGameObject : IPersistenceTemplate
    {
        private static int _idCounter;
        public readonly int Id;
        private readonly GameObjectType Type;

        protected MyGameObject(string name, GameObjectType type, Inventory parentInventory = null)
        {
            Name = name;
            ParentInventory = parentInventory;
            Type = type;
            Id = _idCounter;
            ++_idCounter;
        }

        public string Name;
        public Inventory ParentInventory;

        public virtual void Load(XmlNode doc)
        {
//            throw new System.NotImplementedException();
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            XmlNode itemNode = doc.CreateChild(Type.ToString());
            itemNode.CreateChild("Id", Id);
            itemNode.CreateChild("Name", Name);
            if(ParentInventory != null) itemNode.CreateChild("ParentInventory", ParentInventory.Id);
            return itemNode;
        }
    }
}