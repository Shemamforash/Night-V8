using System;
using Characters;
using Game.World;

namespace Game.Characters.CharacterActions
{
    public class Return : BaseCharacterAction
    {
        private Action _endOfExplorationAction;
        
        public Return(Character character) : base("Return", character)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
            SetStateTransitionTarget("Idle");
        }

        public override void Enter()
        {
            SetDuration(GetCharacter().CurrentRegion.Distance());
            Start();
        }

        public void ReturnToVehicle()
        {
            GetCharacter().Travel();
            GetCharacter().CharacterInventory.MoveAllResources(WorldState.Inventory());
        }
    }
}