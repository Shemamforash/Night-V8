namespace Game.Characters.CharacterActions
{
    public class Rest : BaseCharacterAction
    {
        public Rest(Player playerCharacter) : base(nameof(Rest), playerCharacter)
        {
            DisplayName = "Resting\n+Endurance +Perception";
            ShowTime = false;
            IsVisible = false;
            HourCallback = playerCharacter.Rest;
        }
    }
}