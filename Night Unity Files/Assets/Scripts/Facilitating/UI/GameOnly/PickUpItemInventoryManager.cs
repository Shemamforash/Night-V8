using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine.UI;

namespace Facilitating.UI.GameOnly
{
    public class PickUpItemInventoryManager : InventoryManager
    {
        private TextMeshProUGUI _characterInventoryText, _locationInventoryText;
        private TextMeshProUGUI _characterCarryCapacityText;

        public override void Awake()
        {
            base.Awake();
            _characterCarryCapacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Character Carrying Capacity");
            _characterInventoryText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Character Title");
            _locationInventoryText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Environment Title");
            OriginInventoryContainer.SetOnInventoryMoveAction(delegate
            {
                Inventory inventory = OriginInventoryContainer.GetInventory();
                _characterCarryCapacityText.text = Helper.Round(inventory.GetInventoryWeight(), 1) + "/" + inventory.MaxWeight + "kgs";
            });
            OnInventorySetAction(delegate
            {
                _characterInventoryText.text = CharacterManager.SelectedCharacter.CharacterName;
                _locationInventoryText.text = CharacterManager.SelectedCharacter.CurrentRegion.Name();
            });
        }
    }
}