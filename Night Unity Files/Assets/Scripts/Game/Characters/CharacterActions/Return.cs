using System;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(Player playerCharacter) : base("Return", playerCharacter)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
            AddOnExit(ReturnToVehicle);
        }

        public override void Enter()
        {
            SetDuration(GetCharacter().CurrentRegion.Distance());
            Start();
        }

        public void ReturnToVehicle()
        {
            GetCharacter().Travel();
            GetCharacter().Inventory().MoveAllResources(WorldState.HomeInventory());
        }
    }
}