using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        public readonly string Effect;
        
        public Accessory(string name, float weight, string effect) : base(name, weight, GearSubtype.Accessory)
        {
            Effect = effect;
        }

        public override string GetSummary()
        {
            return Effect;
        }

        public override InventoryUi CreateUi(Transform parent)
        {
            return new AccessoryUi(this, parent);
        }
    }
}