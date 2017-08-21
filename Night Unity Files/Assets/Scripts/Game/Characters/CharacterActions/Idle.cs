using Characters;

namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(Character character) : base("Idle", character)
        {
        }

        public override void Enter()
        {
            Character.CharacterUi.CurrentActionText.Text(Name());
        }

        public override string GetCostAsString()
        {
            return "";
        }
    }
}