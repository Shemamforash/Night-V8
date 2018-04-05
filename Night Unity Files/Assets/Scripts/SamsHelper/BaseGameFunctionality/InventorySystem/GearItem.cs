using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem, IPersistenceTemplate
    {
        private readonly GearSubtype _gearType;
        public bool Equipped;
        private ItemQuality _itemItemQuality;

        protected GearItem(string name, float weight, GearSubtype gearSubtype, ItemQuality itemQuality) : base(name, GameObjectType.Gear, weight)
        {
            SetQuality(itemQuality);
            _gearType = gearSubtype;
        }

        public ItemQuality Quality()
        {
            return _itemItemQuality;
        }

        public void SetQuality(ItemQuality quality)
        {
            _itemItemQuality = quality;
        }

        public void Equip(DesolationInventory p)
        {
            //if in inventory, auto equip and replace
            //if not in inventory open equip window
//            if (!Inventory.InventoryHasSpace(Weight()) && !Inventory.ContainsItem(this)) return;
//            c.AddItemToInventory(this);
//            _equipped = true;
//            c.ReplaceGearInSlot(_gearslot, this);
            MoveTo(p);
            Equipped = true;
        }

        public void Unequip()
        {
            MoveTo(WorldState.HomeInventory());
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

        private void MoveTo(Inventory targetInventory)
        {
            ParentInventory?.Move(this, targetInventory, 1);
            ParentInventory = targetInventory;
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