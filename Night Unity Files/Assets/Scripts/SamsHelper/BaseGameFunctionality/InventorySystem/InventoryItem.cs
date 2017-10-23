using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class InventoryItem : MyGameObject
    {
        protected InventoryItem(string name, GameObjectType type, float weight, Inventory inventory = null) : base(name, type, weight, inventory)
        {
        }

        public virtual float Quantity()
        {
            return 1;
        }

        private float TotalWeight()
        {
            return Helper.Round(Weight * Quantity(), 1);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = new InventoryUi(this, parent);
            ui.SetCentralTextCallback(() => Name);
            ui.SetLeftTextCallback(() => Type.ToString());
            ui.SetRightTextCallback(() => TotalWeight() + "kg");
            ui.SetDestroyCondition(() => Quantity() == 0);
            return ui;
        }
    }
}