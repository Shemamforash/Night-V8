using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Armour : GearItem
    {
        public readonly int ArmourRating, IntelligenceModifier, StabilityModifier, StrengthModifier, EnduranceModifier;
        public readonly string Description;
        
        public Armour(string name, int weight, string description, int armourRating, int intelligenceModifier, int stabilityModifier, int strengthModifier, int enduranceModifier) : base(name, weight, GearSubtype.Armour)
        {
            Description = description;
            ArmourRating = armourRating;
            IntelligenceModifier = intelligenceModifier;
            StabilityModifier = stabilityModifier;
            StrengthModifier = strengthModifier;
            EnduranceModifier = enduranceModifier;
        }

        public override string GetSummary()
        {
            return "Armour";
        }
        
        public override InventoryUi CreateUi(Transform parent)
        {
            return new ArmourUi(this, parent);
        }
    }
}