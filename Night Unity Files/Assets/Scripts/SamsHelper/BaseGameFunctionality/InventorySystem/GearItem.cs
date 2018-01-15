using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.World;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem, IPersistenceTemplate
    {
        private readonly GearSubtype _gearType;
        private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();
        public bool Equipped;

        protected GearItem(string name, float weight, GearSubtype gearSubtype) : base(name, GameObjectType.Gear, weight)
        {
            _gearType = gearSubtype;
        }
        
        public void Equip(DesolationInventory p)
        {
            //if in inventory, auto equip and replace
            //if not in inventory open equip window
//            if (!Inventory.InventoryHasSpace(Weight()) && !Inventory.ContainsItem(this)) return;
//            c.AddItemToInventory(this);
//            _equipped = true;
//            c.ReplaceGearInSlot(_gearslot, this);
            ParentInventory = p;
            _modifiers.ForEach(a => a.Apply());
            Equipped = true;
        }
        
        public void Unequip()
        {
            _modifiers.ForEach(a => a.Remove());
            Equipped = false;
        }

        public abstract string GetSummary();

        public GearSubtype GetGearType()
        {
            return _gearType;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = (InventoryUi) base.CreateUi(parent);
            if (ui == null) return ui;
            ui.SetRightTextCallback(GetSummary);
            ui.SetLeftTextCallback(() => GetGearType().ToString());
            ui.SetCentralTextCallback(() => Name);
            return ui;
        }
        
        public void MoveTo(Inventory targetInventory)
        {
            ParentInventory?.Move(this, targetInventory, 1);
        }

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public virtual XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            SaveController.CreateNodeAndAppend("GearType", root, _gearType);
            return root;
        }
    }
}