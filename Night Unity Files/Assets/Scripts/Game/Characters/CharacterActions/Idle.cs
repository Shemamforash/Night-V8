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
            Character.CurrentRegion = null;
            Character.CharacterUi.CurrentActionText.text = Name();
        }

        public override string GetCostAsString()
        {
            return "";
        }
    }
}