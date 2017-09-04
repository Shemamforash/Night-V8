using System;
using Characters;

namespace Game.Characters.CharacterActions
{
    public class JourneyFromLocation : BaseCharacterAction
    {
        private Action _endOfExplorationAction;
        
        public JourneyFromLocation(Character character) : base("Journey From Location", character)
        {
            IsVisible = false;
            IsDurationFixed = true;
        }

        public override void Exit()
        {
            //todo transfer resources from player to home
            base.Exit();
        }
    }
}