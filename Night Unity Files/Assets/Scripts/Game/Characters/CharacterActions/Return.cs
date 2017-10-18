using System;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(DesolationCharacter character) : base("Return", character)
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