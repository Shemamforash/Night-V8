using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Characters.Player;
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
            Player player = (Player) viewParent.GetLinkedObject();
            InventoryUi inventoryUi = (InventoryUi) viewParent;
            inventoryUi.SetLeftTextCallback(() => player.Name);
            string gearName = player.Weapon == null ? "Nothing Equipped" : player.Weapon.Name;
            inventoryUi.SetCentralTextCallback(() => gearName);
            string gearInfo = player.Weapon == null ? "--" : player.Weapon.GetSummary();
            inventoryUi.SetRightTextCallback(() => gearInfo);
            inventoryUi.PrimaryButton.AddOnClick(() =>
            {
                switch (gear.GetGearType())
                {
                    case GearSubtype.Weapon:
                        player.EquipWeapon((Weapon) gear);
                        break;
                    case GearSubtype.Armour:
                        player.EquipArmourSlotOne((ArmourPlate) gear);
                        break;
                    case GearSubtype.Accessory:
                        player.EquipAccessory((Accessory) gear);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                MenuStateMachine.GoToInitialMenu();
            });
        }
    }

    public static void DisplayGear(Player player, List<MyGameObject> availableGear)
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
                        player.EquipWeapon((Weapon) gearItem);
                        break;
                    case GearSubtype.Armour:
                        player.EquipArmourSlotOne((ArmourPlate) gearItem);
                        break;
                    case GearSubtype.Accessory:
                        player.EquipAccessory((Accessory) gearItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                MenuStateMachine.GoToInitialMenu();
            });
        }
    }
}
