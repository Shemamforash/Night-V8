namespace Game.Characters.CharacterActions
{
    public class Idle : BaseCharacterAction
    {
        public Idle(Player playerCharacter) : base("Idle", playerCharacter)
        {
            IsVisible = false;
            HourCallback = () => GetCharacter().Rest(1);
        }

        public override string GetCostAsString()
        {
            return "";
        }
    }
}