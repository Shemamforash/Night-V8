namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(DesolationCharacter character) : base("Idle", character)
        {
            IsVisible = false;
            HourCallback = () => GetCharacter().Rest(1);
        }

        public override void Enter()
        {
            GetCharacter().CurrentRegion = null;
            GetCharacter().CharacterView.SetActionListActive(true);
        }

        public override void Exit()
        {
            GetCharacter().CharacterView.SetActionListActive(false);
        }
        
        public override string GetCostAsString()
        {
            return "";
        }
    }
}