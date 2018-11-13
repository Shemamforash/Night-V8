using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceItem : NamedItem
    {
        private readonly Number _quantity = new Number();
        public ResourceTemplate Template;

        public ResourceItem(ResourceTemplate template) : this(template.Name)
        {
            Template = template;
        }

        private ResourceItem(string name) : base(name)
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

        public void Save(XmlNode root)
        {
            root = root.CreateChild("Resource");
            root.CreateChild("Quantity", _quantity.CurrentValue());
            root.CreateChild("Template", Template == null ? "" : Template.Name);
        }
    }
}