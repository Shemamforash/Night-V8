namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        public Travel(Player playerCharacter) : base("Travel", playerCharacter)
        {
            IsVisible = false;
            HourCallback = GetCharacter().Travel;
        }

        public void SetTravelTime(int distance)
        {
            SetDuration(distance);
            Start();
        }
    }
}