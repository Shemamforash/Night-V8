using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class InventoryItem : MyGameObject
    {
        protected InventoryItem(string name, GameObjectType type, float weight, Inventory inventory = null) : base(name, type, null, weight, inventory)
        {
        }

        public virtual int Quantity()
        {
            return 1;
        }

        public float TotalWeight()
        {
            return Helper.Round(Weight * Quantity(), 1);
        }
        
        public override BaseInventoryUi CreateUi(Transform parent)
        {
            return new InventoryItemUi(this, parent);
        }
    }
}