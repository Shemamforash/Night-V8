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
        }

        public override void Enter()
        {
            IncreaseDuration(GetCharacter().CurrentRegion.Distance());
            Start();
        }

        public override void Exit()
        {
            GetCharacter().CharacterInventory.MoveAllResources(WorldState.Inventory());
            base.Exit(true);
        }
    }
}