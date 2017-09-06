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
            HourCallback = Character.Travel;
        }

        public override void Enter()
        {
            IncreaseDuration(Character.CurrentRegion.Distance());
            Start();
        }

        public override void Exit()
        {
            Character.CharacterInventory.MoveAllResources(Home.Inventory());
            base.Exit();
        }
    }
}