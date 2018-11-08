using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        private int _timePassed;
        
        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping";
            MinuteCallback = () =>
            {
                --_timePassed;
                if (_timePassed == 0)
                {
                    playerCharacter.Sleep();
                    _timePassed = WorldState.MinutesPerHour / 2;
                }
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
            _timePassed = 0;
            Enter();
        }
    }
}