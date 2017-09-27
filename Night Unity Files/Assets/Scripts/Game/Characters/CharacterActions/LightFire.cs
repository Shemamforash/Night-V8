using Game.World;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Characters.CharacterActions
{
    public class LightFire : BaseCharacterAction
    {
        public LightFire(DesolationCharacter character) : base("Tend Fire", character)
        {
            
        }

        public override void Enter()
        {
            if (WorldState.HomeInventory.GetResource(InventoryResourceType.Fuel).Quantity() > 0)
            {
                SetDuration(1);
                MinuteCallback = Campfire.Tend;
                WorldState.HomeInventory.DecrementResource(InventoryResourceType.Fuel, 1);
                Start();
            }
            else
            {
                ParentMachine.ReturnToDefault();
            }
        }
    }
}