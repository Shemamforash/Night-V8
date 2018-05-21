using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class InventoryItem : MyGameObject
    {
        protected InventoryItem(string name, GameObjectType type, float weight, Inventory parentInventory = null) : base(name, type, weight, parentInventory)
        {
        }

        public virtual float Quantity()
        {
            return 1;
        }

        public float TotalWeight()
        {
            return Helper.Round(Weight * Quantity(), 1);
        }

        public virtual bool IsStackable()
        {
            return true;
        }
    }
}