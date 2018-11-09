using Game.Global;
using UnityEngine;

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
                Debug.Log(_timePassed + " " + Duration);
                --_timePassed;
                if (_timePassed == 0)
                {
                    playerCharacter.Sleep();
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
            int maxSleepTime = PlayerCharacter.GetMaxSleepTime();
            if (maxSleepTime == 0) return;
            SetDuration(maxSleepTime * WorldState.MinutesPerHour / 2);
            ResetTimePassed();
            Enter();
        }
    }
}