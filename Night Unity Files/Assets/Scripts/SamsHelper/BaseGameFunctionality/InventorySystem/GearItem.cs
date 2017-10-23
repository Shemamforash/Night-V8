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
        public readonly AttributesModifier Modifier = new AttributesModifier();

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
        }

        public abstract string GetSummary();

        public GearSubtype GetGearType()
        {
            return _gearType;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = (InventoryUi) base.CreateUi(parent);
//            ui.SetRightButtonTextCallback(() => "Equip");
//            ui.SetRightButtonActive(false);
            ui.SetRightTextCallback(GetSummary);
            ui.SetLeftTextCallback(() => GetGearType().ToString());
            ui.SetCentralTextCallback(() => Name);
            return ui;
        }
        
        public void MoveTo(Inventory targetInventory)
        {
            Inventory?.Move(this, targetInventory, 1);
        }
    }
}