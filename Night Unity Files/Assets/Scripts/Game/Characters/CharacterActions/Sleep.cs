using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping\n+Fettle +Grit";
            HourCallback = playerCharacter.Sleep;
            MinuteCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                PlayerCharacter.RestAction.Enter();
            };
        }

        protected override void OnClick()
        {
            int maxSleepTime = PlayerCharacter.GetMaxSleepTime();
            if (maxSleepTime == 0) return;
            SetDuration(maxSleepTime * WorldState.MinutesPerHour / 2);
            Enter();
        }
    }
}