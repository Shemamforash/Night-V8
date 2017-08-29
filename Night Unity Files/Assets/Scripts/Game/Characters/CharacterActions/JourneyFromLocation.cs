using System;
using Characters;
using Game.World;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class JourneyFromLocation : BaseCharacterAction
    {
        private Action _endOfExplorationAction;
        
        public JourneyFromLocation(Character character) : base("Journey From Location", character)
        {
            IsVisible = false;
        }

        public override void Exit()
        {
            //todo transfer resources from player to home
            base.Exit();
        }
    }
}