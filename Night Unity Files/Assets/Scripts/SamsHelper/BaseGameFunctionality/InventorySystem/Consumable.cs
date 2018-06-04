using Game.Characters;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        public Consumable(string name, GameObjectType type, float weight, Inventory parentInventory = null) : base(name, type, weight, parentInventory)
        {
        }

        public string Effect1, Effect2;

        public void Consume(Player selectedCharacter)
        {
        }
    }
}