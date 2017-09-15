﻿using Characters;
using Game.Combat;

namespace Game.Characters.CharacterActions
{
    public class Combat : BaseCharacterAction
    {
        public Combat(Character character) : base("Combat", character)
        {
            DefaultDuration = 0;
        }

        public override void Enter()
        {
            CombatManager.EnterCombat(GetCharacter());
        }
    }
}