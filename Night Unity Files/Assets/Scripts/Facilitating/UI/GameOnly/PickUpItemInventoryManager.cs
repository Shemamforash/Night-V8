using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine.UI;

namespace Facilitating.UI.GameOnly
{
    public class PickUpItemInventoryManager : InventoryManager
    {
        private Text _characterInventoryText, _locationInventoryText;
        private Text _characterCarryCapacityText;

        public override void Awake()
        {
            base.Awake();
            _characterCarryCapacityText = Helper.FindChildWithName<Text>(gameObject, "Character Carrying Capacity");
            _characterInventoryText = Helper.FindChildWithName<Text>(gameObject, "Character Title");
            _locationInventoryText = Helper.FindChildWithName<Text>(gameObject, "Environment Title");
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