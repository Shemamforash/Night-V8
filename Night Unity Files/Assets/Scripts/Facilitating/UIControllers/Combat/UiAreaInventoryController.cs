using Facilitating.UIControllers;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiAreaInventoryController : IInputListener
{
    public const float MaxShowInventoryDistance = 0.5f;

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        switch (axis)
        {
            case InputAxis.Inventory:
                MenuStateMachine.ShowMenu("HUD");
                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    public static void OpenInventory()
    {
        UiGearMenuController.ShowConsumableMenu();
        MenuStateMachine.ShowMenu("Inventories");
    }
}