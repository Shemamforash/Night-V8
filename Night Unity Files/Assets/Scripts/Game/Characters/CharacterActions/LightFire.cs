using Facilitating;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Characters.CharacterActions
{
    public class LightFire : BaseCharacterAction
    {
        public LightFire(Player playerCharacter) : base("Tend Fire", playerCharacter)
        {
            HourCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                Campfire.Tend();
                WorldState.HomeInventory().DecrementResource(InventoryResourceType.Fuel, 1);
                Exit();
            };
        }

        public override void Enter()
        {
            base.Enter();
            if (WorldState.HomeInventory().GetResource(InventoryResourceType.Fuel).Quantity() > 0)
                Duration = 1;
            else
                PlayerCharacter.RestAction.Enter();
        }
    }
}