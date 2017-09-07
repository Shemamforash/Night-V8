using Characters;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        private int _enduranceRecoveryAmount = 5;
        
        public Sleep(Character character) : base("Sleep", character)
        {
            HourCallback = () => GetCharacter().Rest(_enduranceRecoveryAmount);
        }

        public override void Exit()
        {
            Debug.Log("banana");
            base.Exit(false);
            ClearOnExit();
        }

        public override string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs & +" + _enduranceRecoveryAmount * TimeRemainingAsHours() + " end ";
        }
    }
}