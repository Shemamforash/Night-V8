using Game.World.WorldEvents;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Player playerCharacter) : base("Sleep", playerCharacter)
        {
            HourCallback = () =>
            {
                string storyProgress = playerCharacter.GetCurrentStoryProgress();
                if (storyProgress != null)
                {
                    WorldEventManager.GenerateEvent(new WorldEvent(storyProgress));
                }
                GetCharacter().Rest(1);
            };
        }

        public override string GetCostAsString()
        {
            return "Resting";
        }

        public override void Enter()
        {
            base.Enter();
            SetDuration((int) (GetCharacter().Energy.Max - GetCharacter().Energy.CurrentValue()));
            Start();
        }

        public override void Exit()
        {
            base.Exit();
            ClearOnExit();
        }
    }
}