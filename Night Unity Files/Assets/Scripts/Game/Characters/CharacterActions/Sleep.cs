using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Sleep : BaseCharacterAction
    {
        private int _timePassed;
        private TextMeshProUGUI _buttonText;
        private bool _sleeping;

        public Sleep(Player playerCharacter) : base(nameof(Sleep), playerCharacter)
        {
            DisplayName = "Sleeping";
            MinuteCallback = () =>
            {
                --_timePassed;
                if (_timePassed == 0)
                {
                    playerCharacter.Sleep();
                    ResetTimePassed();
                }

                --Duration;
                if (Duration != 0) return;
              WakeUp();  
            };
        }

        private void WakeUp()
        {
            _sleeping = false;
            PlayerCharacter.RestAction.Enter();
            _buttonText.text = "Sleep";
        }

        public override void SetButton(EnhancedButton button)
        {
            base.SetButton(button);
            _buttonText = button.FindChildWithName<TextMeshProUGUI>("Text");
        }
        
        private void ResetTimePassed()
        {
            _timePassed = WorldState.MinutesPerHour / 4;
        }

        protected override void OnClick()
        {
            if (_sleeping)
            {
                WakeUp();
                return;
            }
            int maxSleepTime = PlayerCharacter.GetMaxSleepTime();
            if (maxSleepTime == 0) return;
            SetDuration(maxSleepTime * WorldState.MinutesPerHour / 4);
            ResetTimePassed();
            Enter();
            _sleeping = true;
            _buttonText.text = "Awaken";
        }
    }
}