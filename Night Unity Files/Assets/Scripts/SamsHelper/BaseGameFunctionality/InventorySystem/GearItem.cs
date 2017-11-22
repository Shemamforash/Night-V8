using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem
    {
        private readonly GearSubtype _gearType;
        public bool Equipped { get; set; }
        protected readonly List<AttributeModifier> Modifiers = new List<AttributeModifier>();

        protected GearItem(string name, float weight, GearSubtype gearSubtype) : base(name, GameObjectType.Gear, weight)
        {
            _gearType = gearSubtype;
        }

        public void Equip()
        {
            //if in inventory, auto equip and replace
            //if not in inventory open equip window
//            if (!Inventory.InventoryHasSpace(Weight()) && !Inventory.ContainsItem(this)) return;
//            c.AddItemToInventory(this);
//            _equipped = true;
//            c.ReplaceGearInSlot(_gearslot, this);
            Modifiers.ForEach(a => a.Apply());
            Equipped = true;
        }
        
        public void Unequip()
        {
            Modifiers.ForEach(a => a.Remove());
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
            if (ui != null)
            {
                ui.SetRightTextCallback(GetSummary);
                ui.SetLeftTextCallback(() => GetGearType().ToString());
                ui.SetCentralTextCallback(() => Name);
            }
            return ui;
        }
        
        public void MoveTo(Inventory targetInventory)
        {
            ParentInventory?.Move(this, targetInventory, 1);
        }   
    }
}