using Facilitating.UIControllers;
using Game.Combat.Player;
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

    private static ContainerController NearestContainer()
    {
        ContainerController nearestContainer = null;
        float nearestContainerDistance = MaxShowInventoryDistance;
        ContainerController.Containers.ForEach(c =>
        {
            float distance = Vector2.Distance(c.transform.position, PlayerCombat.Instance.transform.position);
            if (distance > nearestContainerDistance) return;
            nearestContainerDistance = distance;
            nearestContainer = c.ContainerController;
        });
        return nearestContainer;
    }

    public static void OpenInventory()
    {
        UiGearMenuController.ShowConsumableMenu(PlayerCombat.Instance.Player);
        MenuStateMachine.ShowMenu("Inventories");
    }

//    private void UseItem()
//    {
//        Consumable consumable = _inventory.Contents()[_selectedItem] as Consumable;
//        if (consumable != null)
//        {
//            consumable.Consume(PlayerCombat.Instance.Player);
//            SelectItem();
//        }
//
//        Weapon weapon = _inventory.Contents()[_selectedItem] as Weapon;
//        if (weapon != null)
//        {
//            PlayerCombat.Equip(weapon);
//        }
//    }

    public static void TakeItem()
    {
        NearestContainer()?.Take();
    }
}