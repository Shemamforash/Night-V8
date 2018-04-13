using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Global;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem
    {
        private readonly GearSubtype _gearType;
        private ItemQuality _itemItemQuality;

        protected GearItem(string name, float weight, GearSubtype gearSubtype, ItemQuality itemQuality) : base(name, GameObjectType.Gear, weight)
        {
            SetQuality(itemQuality);
            _gearType = gearSubtype;
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            SaveController.CreateNodeAndAppend("GearType", root, _gearType);
            return root;
        }

        public ItemQuality Quality()
        {
            return _itemItemQuality;
        }

        public void SetQuality(ItemQuality quality)
        {
            _itemItemQuality = quality;
        }

        public void Equip(Character character)
        {
            //if in inventory, auto equip and replace
            //if not in inventory open equip window
//            if (!Inventory.InventoryHasSpace(Weight()) && !Inventory.ContainsItem(this)) return;
//            c.AddItemToInventory(this);
//            _equipped = true;
//            c.ReplaceGearInSlot(_gearslot, this);
            MoveTo(character.Inventory());
        }

        public void Unequip()
        {
            MoveTo(WorldState.HomeInventory());
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
            targetInventory.Move(this, 1);
            ParentInventory = targetInventory;
        }
    }
}