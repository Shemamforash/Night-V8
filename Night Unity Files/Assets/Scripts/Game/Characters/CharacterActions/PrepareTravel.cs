using System;
using Characters;
using Game.World;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class PrepareTravel : BaseCharacterAction
    {
        public PrepareTravel(Character character) : base("Prepare Travel", character)
        {
        }

        public override void Enter()
        {
            RegionManager.EnterManager(Character);
        }
    }
}