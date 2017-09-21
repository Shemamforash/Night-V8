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
            GetCharacter().CharacterUi.CurrentActionText.text = Name();
            GetCharacter().SetActionListActive(true);
        }

        public override void Exit()
        {
            GetCharacter().SetActionListActive(false);
        }
        
        public override string GetCostAsString()
        {
            return "";
        }
    }
}