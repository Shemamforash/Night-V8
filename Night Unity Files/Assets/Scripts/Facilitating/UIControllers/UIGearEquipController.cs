using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
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
            string gearName = c.EquipmentController.Weapon() == null ? "Nothing Equipped" : c.EquipmentController.Weapon().Name;
            inventoryUi.SetCentralTextCallback(() => gearName);
            string gearInfo = c.EquipmentController.Weapon() == null ? "--" : c.EquipmentController.Weapon().GetSummary();
            inventoryUi.SetRightTextCallback(() => gearInfo);
            inventoryUi.PrimaryButton.AddOnClick(() =>
            {
                c.Equip(gear);
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
                character.Equip(gearItem);
                MenuStateMachine.GoToInitialMenu();
            });
        }
    }
}
