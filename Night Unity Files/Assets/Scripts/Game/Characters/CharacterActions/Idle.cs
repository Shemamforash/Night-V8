namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(Player playerCharacter) : base("Idle", playerCharacter)
        {
            IsVisible = false;
        }

        public override string GetCostAsString()
        {
            return "";
        }
    }
}