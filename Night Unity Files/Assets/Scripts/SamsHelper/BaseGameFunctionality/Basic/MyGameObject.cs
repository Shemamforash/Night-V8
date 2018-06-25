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
        public readonly GameObjectType Type;

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

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new System.NotImplementedException();
        }

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode itemNode = SaveController.CreateNodeAndAppend(Type.ToString(), doc);
            SaveController.CreateNodeAndAppend("Id", itemNode, Id);
            SaveController.CreateNodeAndAppend("Name", itemNode, Name);
            SaveController.CreateNodeAndAppend("ParentInventory", itemNode, ParentInventory?.Id ?? -1);
            SaveController.CreateNodeAndAppend("Type", itemNode, Type);
            return itemNode;
        }
    }
}