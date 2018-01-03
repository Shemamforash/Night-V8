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
        public readonly AttributeModifier ArmourRating, PerceptionModifier, WillpowerModifier, StrengthModifier, EnduranceModifier;
        public readonly string Description;
        
        public Armour(string name, int weight, string description, int armourRating, int perceptionModifier, int willpowerModifier, int strengthModifier, int enduranceModifier) : base(name, weight, GearSubtype.Armour)
        {
            Description = description;
            //TODO fill me in
//            ArmourRating = armourRating;
//            PerceptionModifier = perceptionModifier;
//            WillpowerModifier = willpowerModifier;
//            StrengthModifier = strengthModifier;
//            EnduranceModifier = enduranceModifier;
//            Modifiers.AddModifier(AttributeType.Perception, perceptionModifier, true);
//            Modifiers.AddModifier(AttributeType.Willpower, willpowerModifier, true);
//            Modifiers.AddModifier(AttributeType.Strength, strengthModifier, true);
//            Modifiers.AddModifier(AttributeType.Endurance, enduranceModifier, true);
        }

        public override string GetSummary()
        {
            return ArmourRating + " Armour";
        }
        
//        public override ViewParent CreateUi(Transform parent)
//        {
//            return new ArmourUi(this, parent);
//        }
    }
}