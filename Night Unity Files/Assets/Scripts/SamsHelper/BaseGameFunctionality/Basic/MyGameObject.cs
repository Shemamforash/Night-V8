using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class MyGameObject : IPersistenceTemplate
    {
        private GameObject _gameObject;
        public string Name { get; set; }
        private string _extendedName;
        public float Weight { get; set; }
        public Inventory ParentInventory { get; set; }
        public readonly GameObjectType Type;
        private static int _idCounter;
        public readonly int Id;

        protected MyGameObject(string name, GameObjectType type, float weight = 0, Inventory parentInventory = null)
        {
            Name = name;
            Weight = weight;
            ParentInventory = parentInventory;
            Type = type;
            Id = _idCounter;
            ++_idCounter;
        }

        public virtual void SetGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public string ExtendedName() => _extendedName ?? Name;
        public void SetExtendedName(string name) => _extendedName = name;

        public virtual ViewParent CreateUi(Transform parent)
        {
            return new SimpleView(this, parent);
        }

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new System.NotImplementedException();
        }

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode itemNode = SaveController.CreateNodeAndAppend(Type.ToString(), doc);
            SaveController.CreateNodeAndAppend("Id", itemNode, Id);
            SaveController.CreateNodeAndAppend("Name", itemNode, Name);
            SaveController.CreateNodeAndAppend("ExtendedName", itemNode, _extendedName);
            SaveController.CreateNodeAndAppend("Weight", itemNode, Weight);
            SaveController.CreateNodeAndAppend("ParentInventory", itemNode, ParentInventory.Id);
            SaveController.CreateNodeAndAppend("Type", itemNode, Type);
            return itemNode;
        }
    }
}