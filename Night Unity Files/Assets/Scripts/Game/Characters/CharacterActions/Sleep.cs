namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        private int _enduranceRecoveryAmount = 1;
        
        public Sleep(Player playerCharacter) : base("Sleep", playerCharacter)
        {
            HourCallback = () => GetCharacter().Rest(_enduranceRecoveryAmount);
        }

        public override string GetCostAsString()
        {
            return "+" +_enduranceRecoveryAmount + " energy/hr";
        }

        public override void Enter()
        {
            base.Enter();
            SetDuration((int) (GetCharacter().Energy.Max - GetCharacter().Energy.CurrentValue()));
            Start();
        }

        public override void Exit()
        {
            base.Exit();
            ClearOnExit();
        }
    }
}