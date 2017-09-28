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

        public override InventoryUi CreateUi(Transform parent)
        {
            InventoryUi ui = base.CreateUi(parent);
            ui.DisableBorder();
            ui.SetLeftButtonTextCallback(() => GearItem.GetGearType().ToString());
            ui.SetLeftButtonActive(false);
            ui.SetLeftTextCallback(() => Character.Name);
            ui.SetCentralTextCallback(() => GearItem.Name);
            ui.SetRightTextCallback(() => "^");
            ui.SetRightButtonTextCallback(() => "Equip");
            ui.OnRightButtonPress(() => Character.Equip(GearItem));
            return ui;
        }
    }
}