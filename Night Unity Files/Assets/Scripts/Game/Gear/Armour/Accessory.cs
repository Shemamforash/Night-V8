using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        public readonly string Effect, Description;
        
        public Accessory(string name, float weight, string description, string effect) : base(name, weight, GearSubtype.Accessory)
        {
            Description = description;
            Effect = effect;
        }

        public override string GetSummary()
        {
            return Effect;
        }

//        public override ViewParent CreateUi(Transform parent)
//        {
//            return new AccessoryUi(this, parent);
//        }
    }
}