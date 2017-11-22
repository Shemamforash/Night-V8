using Game.Characters;
using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Armour : GearItem
    {
        public readonly AttributeModifier ArmourRating, IntelligenceModifier, StabilityModifier, StrengthModifier, EnduranceModifier;
        public readonly string Description;
        
        public Armour(string name, int weight, string description, int armourRating, int intelligenceModifier, int stabilityModifier, int strengthModifier, int enduranceModifier) : base(name, weight, GearSubtype.Armour)
        {
            Description = description;
            //TODO fill me in
//            ArmourRating = armourRating;
//            IntelligenceModifier = intelligenceModifier;
//            StabilityModifier = stabilityModifier;
//            StrengthModifier = strengthModifier;
//            EnduranceModifier = enduranceModifier;
//            Modifiers.AddModifier(AttributeType.Intelligence, intelligenceModifier, true);
//            Modifiers.AddModifier(AttributeType.Stability, stabilityModifier, true);
//            Modifiers.AddModifier(AttributeType.Strength, strengthModifier, true);
//            Modifiers.AddModifier(AttributeType.Endurance, enduranceModifier, true);
        }

        public override string GetSummary()
        {
            return ArmourRating + " Armour";
        }
        
        public override ViewParent CreateUi(Transform parent)
        {
            return new ArmourUi(this, parent);
        }
    }
}