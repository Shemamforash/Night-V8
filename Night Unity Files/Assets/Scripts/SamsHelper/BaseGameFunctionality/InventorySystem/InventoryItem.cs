using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryItem : MyGameObject
    {
        private readonly Number _quantity = new Number();
        private bool _stackable;
        public ResourceTemplate Template;

        public InventoryItem(ResourceTemplate template, GameObjectType type, Inventory parentInventory = null) : base(template.Name, type, parentInventory)
        {
            Template = template;
        }

        protected InventoryItem(string name, GameObjectType type, Inventory parentInventory = null) : base(name, type, parentInventory)
        {
            _quantity.Increment();
        }

        public int Quantity()
        {
            return Mathf.FloorToInt(_quantity.CurrentValue());
        }

        public void Increment(int amount)
        {
            _quantity.SetCurrentValue(_quantity.CurrentValue() + amount);
        }

        public void Decrement(int amount)
        {
            _quantity.SetCurrentValue(_quantity.CurrentValue() - amount);
        }

        public override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            root.CreateChild("Quantity", _quantity.CurrentValue());
            root.CreateChild("Stackable", _stackable);
            root.CreateChild("Template", Template == null ? "" : Template.Name);
            return root;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
            _quantity.SetCurrentValue(root.FloatFromNode("Quantity"));
            _stackable = root.BoolFromNode("Stackable");
            string templateString = root.StringFromNode("Template");
            if (templateString == "") return;
            Template = ResourceTemplate.StringToTemplate(templateString);
        }

        public virtual bool IsStackable()
        {
            return _stackable;
        }

        public void SetStackable(bool stackable)
        {
            _stackable = stackable;
        }
    }
}