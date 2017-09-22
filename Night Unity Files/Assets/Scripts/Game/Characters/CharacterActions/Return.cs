using System;
using Characters;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        public Return(DesolationCharacter character) : base("Return", character)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
            SetStateTransitionTarget("Idle");
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
            GetCharacter().CharacterInventory.MoveAllResources(WorldState.Home());
        }
    }
}