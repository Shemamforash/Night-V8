using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Weapons
{
    public class Inscription : MyGameObject
    {
        private AttributeModifier _modifier;

        public Inscription(string name, Inventory parentInventory = null) : base(name, GameObjectType.Inscription, 0.25f, parentInventory)
        {
        }
        
        
    }
}