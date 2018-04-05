using Game.World.WorldEvents;

namespace Game.Characters.CharacterActions
{
    public class Rest : BaseCharacterAction
    {
        public Rest(Player.Player playerCharacter) : base(nameof(Rest), playerCharacter)
        {
            IsVisible = false;
            HourCallback = () =>
            {
                string storyProgress = playerCharacter.GetCurrentStoryProgress();
                if (storyProgress != null)
                {
                    WorldEventManager.GenerateEvent(new WorldEvent(storyProgress));
                }

                PlayerCharacter.Rest(1);
                if (PlayerCharacter.Energy.ReachedMax() && PlayerCharacter.DistanceFromHome > 0)
                {
                    PlayerCharacter.ReturnAction.Enter();
                }
            };
        }

        public override string GetActionText()
        {
            return "Resting";
        }
    }
}