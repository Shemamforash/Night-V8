using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        private readonly bool _inscribable;
        public readonly ArmourPlateType PlateType;

        public ArmourPlate(ArmourPlateType plateType, float weight) : base(plateType.ToString(), weight, GearSubtype.Armour)
        {
            PlateType = plateType;
            if (weight == 5)
            {
                _inscribable = true;
            }
        }

        public override string GetSummary()
        {
            return PlateType + " Armour Plate";
        }

        public static ArmourPlate CreatePlate(ArmourPlateType plateType)
        {
            int weight = (int)plateType + 1;
            return new ArmourPlate(plateType, weight);
        }
    }
}