using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterGearComparison : MyGameObject
    {
        public readonly Character Character;
        public readonly GearItem GearItem;

        public CharacterGearComparison(Character character, GearItem gearItem) : base(gearItem.Name, GameObjectType.Gear)
        {
            Character = character;
            GearItem = gearItem;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            ViewParent uiParent = base.CreateUi(parent);
            InventoryUi ui = uiParent as InventoryUi;
            if (ui == null) return uiParent;
            ui.SetLeftTextCallback(() => GearItem.GetGearType().ToString());
            ui.SetLeftTextCallback(() => Character.Name);
            ui.SetCentralTextCallback(() =>
            {
                GearItem equippedGear = Character.EquipmentController.GetGearItem(GearItem.GetGearType());
                return equippedGear == null ? "Nothing equipped" : equippedGear.Name;
            });
            ui.SetRightTextCallback(() => "Equip");
            ui.PrimaryButton.AddOnClick(() => Character.Equip(GearItem));
            return ui;
        }
    }
}