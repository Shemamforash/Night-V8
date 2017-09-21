using Characters;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        private int _enduranceRecoveryAmount = 5;
        
        public Sleep(DesolationCharacter character) : base("Sleep", character)
        {
            HourCallback = () => GetCharacter().Rest(_enduranceRecoveryAmount);
        }

        public void ShowDurationMenu()
        {
            MenuStateMachine.States.NavigateToState("Action Duration Menu");
        }
        
        public override string GetCostAsString()
        {
            return "+" +_enduranceRecoveryAmount + " end/hr";
        }

        public void Exit()
        {
            base.Exit();
            ClearOnExit();
        }
    }
}