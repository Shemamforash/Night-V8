using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;

public class UIGearEquipController : Menu
{
    private static MenuList _menuList;

    public void Awake()
    {
        _menuList = GetComponent<MenuList>();
    }

    public static void DisplayCharacters(GearItem gear)
    {
        MenuStateMachine.ShowMenu("Equip Menu");
        _menuList.SetItems(CharacterManager.Characters());
        foreach (ViewParent viewParent in _menuList.Items)
        {
            Character c = (Character) viewParent.GetLinkedObject();
            InventoryUi inventoryUi = (InventoryUi) viewParent;
            inventoryUi.SetLeftTextCallback(() => c.Name);
            string gearName = c.Weapon == null ? "Nothing Equipped" : c.Weapon.Name;
            inventoryUi.SetCentralTextCallback(() => gearName);
            string gearInfo = c.Weapon == null ? "--" : c.Weapon.GetSummary();
            inventoryUi.SetRightTextCallback(() => gearInfo);
            inventoryUi.PrimaryButton.AddOnClick(() =>
            {
                switch (gear.GetGearType())
                {
                    case GearSubtype.Weapon:
                        c.EquipWeapon((Weapon) gear);
                        break;
                    case GearSubtype.Armour:
                        c.ArmourController.AddPlate((ArmourPlate) gear);
                        break;
                    case GearSubtype.Accessory:
                        c.EquipAccessory((Accessory) gear);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                MenuStateMachine.GoToInitialMenu();
            });
        }
    }

    public static void DisplayGear(Character character, List<MyGameObject> availableGear)
    {
        MenuStateMachine.ShowMenu("Equip Menu");
        _menuList.SetItems(availableGear);
        foreach (ViewParent viewParent in _menuList.Items)
        {
            InventoryUi inventoryUi = (InventoryUi) viewParent;
            inventoryUi.PrimaryButton.Button().onClick.RemoveAllListeners();
            GearItem gearItem = (GearItem) inventoryUi.GetLinkedObject();
            inventoryUi.PrimaryButton.AddOnClick(() =>
            {
                switch (gearItem.GetGearType())
                {
                    case GearSubtype.Weapon:
                        character.EquipWeapon((Weapon) gearItem);
                        break;
                    case GearSubtype.Armour:
                        character.ArmourController.AddPlate((ArmourPlate) gearItem);
                        break;
                    case GearSubtype.Accessory:
                        character.EquipAccessory((Accessory) gearItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                MenuStateMachine.GoToInitialMenu();
            });
        }
    }
}
