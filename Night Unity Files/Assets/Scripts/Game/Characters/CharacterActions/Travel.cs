﻿using Game.Combat;
using Game.World.Region;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        public Travel(DesolationCharacter character) : base("Travel", character)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
        }

        public void SetTargetRegion(Region targetRegion)
        {
            GetCharacter().CurrentRegion = targetRegion;
            SetDuration(GetCharacter().CurrentRegion.Distance());
            Start();
            SetStateTransitionTarget(CheckForEnterCombat());
        }

        public Region GetTargetRegion()
        {
            return GetCharacter().CurrentRegion;
        }

        private string CheckForEnterCombat()
        {
            CombatScenario scenario = GetCharacter().CurrentRegion.GetCombatScenario();
            if (scenario != null)
            {
                return "Combat";
            }
            return "Collect Resources";
        }
    }
}