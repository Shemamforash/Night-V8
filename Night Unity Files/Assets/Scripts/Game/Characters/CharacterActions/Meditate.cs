using Game.Global;

namespace Game.Characters.CharacterActions
{
    public class Meditate : BaseCharacterAction
    {
        private int _timePassed;

        public Meditate(Player playerCharacter) : base(nameof(Meditate), playerCharacter)
        {
            DisplayName = "Meditating";
            HourCallback = playerCharacter.Meditate;
            MinuteCallback = () =>
            {
                --_timePassed;
                if (_timePassed == 0)
                {
                    playerCharacter.Meditate();
                    ResetTimePassed();
                }

                --Duration;
                if (Duration != 0) return;
                PlayerCharacter.RestAction.Enter();
            };
        }

        private void ResetTimePassed()
        {
            _timePassed = WorldState.MinutesPerHour / 2;
        }

        protected override void OnClick()
        {
            int maxMeditateTime = PlayerCharacter.GetMaxMeditateTime();
            if (maxMeditateTime == 0) return;
            SetDuration(maxMeditateTime * WorldState.MinutesPerHour / 2);
            ResetTimePassed();
            Enter();
        }
    }
}