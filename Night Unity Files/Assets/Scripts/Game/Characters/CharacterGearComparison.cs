using System;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterGearComparison : MyGameObject
    {
        private readonly Character _character;
        private readonly GearItem _gearItem;

        public CharacterGearComparison(Character character, GearItem gearItem) : base(gearItem.Name, GameObjectType.Gear)
        {
            _character = character;
            _gearItem = gearItem;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            ViewParent uiParent = base.CreateUi(parent);
            InventoryUi ui = uiParent as InventoryUi;
            if (ui == null) return uiParent;
            ui.SetLeftTextCallback(() => _gearItem.GetGearType().ToString());
            ui.SetLeftTextCallback(() => _character.Name);
            ui.SetCentralTextCallback(() =>
            {
                GearItem equippedGear;
                switch (_gearItem.GetGearType())
                {
                    case GearSubtype.Weapon:
                        equippedGear = _character.Weapon;
                        break;
                    case GearSubtype.Armour:
                        equippedGear = null;
                        break;
                    case GearSubtype.Accessory:
                        equippedGear = _character.Accessory;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return equippedGear == null ? "Nothing equipped" : equippedGear.Name;
            });
            ui.SetRightTextCallback(() => "Equip");
            switch (_gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    ui.PrimaryButton.AddOnClick(() => _character.EquipWeapon((Weapon) _gearItem));
                    break;
                case GearSubtype.Armour:
                    ui.PrimaryButton.AddOnClick(() => _character.ArmourController.SetPlateOne((ArmourPlate) _gearItem));
                    break;
                case GearSubtype.Accessory:
                    ui.PrimaryButton.AddOnClick(() => _character.EquipAccessory((Accessory) _gearItem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ui;
        }
    }
}