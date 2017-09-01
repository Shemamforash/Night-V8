using System;
using Characters;
using Game.World;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class PrepareExpedition : BaseCharacterAction
    {
        public PrepareExpedition(Character character) : base("Prepare Expedition", character)
        {
        }

        public override void Enter()
        {
            RegionManager.EnterManager(Character);
        }
    }
}