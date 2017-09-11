using Characters;

namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(Character character) : base("Idle", character)
        {
            IsVisible = false;
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