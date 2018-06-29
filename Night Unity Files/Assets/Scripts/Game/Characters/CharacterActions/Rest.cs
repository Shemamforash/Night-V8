namespace Game.Characters.CharacterActions
{
    public class Rest : BaseCharacterAction
    {
        public Rest(Player playerCharacter) : base(nameof(Rest), playerCharacter)
        {
            DisplayName = "Resting";
            ShowTime = false;
            IsVisible = false;
            HourCallback = playerCharacter.Rest;
        }
    }
}