using System;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(Player playerCharacter) : base("Return", playerCharacter)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Return;
            AddOnExit(ReturnToVehicle);
        }

        public override void Enter()
        {
            SetDuration(GetCharacter().DistanceFromHome);
            Start();
        }

        public void ReturnToVehicle()
        {
            GetCharacter().Inventory().MoveAllResources(WorldState.HomeInventory());
        }
    }
}