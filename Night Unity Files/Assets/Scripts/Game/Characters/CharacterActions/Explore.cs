using System;
using Characters;
using Game.World;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Explore : BaseCharacterAction
    {
        private Action _endOfExplorationAction;
        
        public Explore(Character character) : base("Explore", character)
        {
        }

        public override void Enter()
        {
            RegionManager.EnterManager(this);
            _endOfExplorationAction = null;
        }

        public void SetExplorationAction(Action endOfExplorationAction)
        {
            _endOfExplorationAction = endOfExplorationAction;
            Start();
        }

        public override void Exit()
        {
            if (_endOfExplorationAction != null)
            {
                _endOfExplorationAction();
            }
            base.Exit();
        }
    }
}