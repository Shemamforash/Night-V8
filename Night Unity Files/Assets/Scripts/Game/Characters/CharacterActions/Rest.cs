using Game.Exploration.WorldEvents;

namespace Game.Characters.CharacterActions
{
    public class Rest : BaseCharacterAction
    {
        public Rest(Player playerCharacter) : base(nameof(Rest), playerCharacter)
        {
            DisplayName = "Resting";
            ShowTime = false;
            IsVisible = false;
            HourCallback = () =>
            {
                playerCharacter.Rest(1);
                string storyProgress = playerCharacter.GetCurrentStoryProgress();
                if (storyProgress != null) WorldEventManager.GenerateEvent(new WorldEvent(storyProgress));
            };
        }
    }
}