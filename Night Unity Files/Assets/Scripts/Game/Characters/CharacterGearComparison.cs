using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterGearComparison : MyGameObject
    {
        public readonly DesolationCharacter Character;
        public readonly GearItem GearItem;

        public CharacterGearComparison(DesolationCharacter character, GearItem gearItem) : base(gearItem.Name, GameObjectType.Gear)
        {
            Character = character;
            GearItem = gearItem;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = (InventoryUi) base.CreateUi(parent);
            ui.SetLeftButtonTextCallback(() => GearItem.GetGearType().ToString());
            ui.SetLeftButtonActive(false);
            ui.SetLeftTextCallback(() => Character.Name);
            ui.SetCentralTextCallback(() =>
            {
                GearItem equippedGear = Character.EquippedGear[GearItem.GetGearType()];
                return equippedGear == null ? "Nothing equipped" : equippedGear.Name;
            });
            ui.SetRightTextCallback(() => "^");
            ui.SetRightButtonTextCallback(() => "Equip");
            ui.SetRightButtonActive(false);
            ui.OnPress(() => Character.Equip(GearItem));
            return ui;
        }
    }
}