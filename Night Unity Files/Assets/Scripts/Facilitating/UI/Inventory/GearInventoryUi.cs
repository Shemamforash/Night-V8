using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Facilitating.UI.Inventory
{
    public class GearInventoryUi : InventoryItemUi
    {
        public GearInventoryUi(BasicInventoryItem inventoryItem, Transform parent, bool equippable) : base(inventoryItem, parent)
        {
            SummaryText.text = ((EquippableItem)inventoryItem).GetSummary();
            if (!equippable) return;
            ButtonText.text = "Equip";
            ActionButton.AddOnClick(() => ((EquippableItem)inventoryItem).Equip());
        }
    }
}