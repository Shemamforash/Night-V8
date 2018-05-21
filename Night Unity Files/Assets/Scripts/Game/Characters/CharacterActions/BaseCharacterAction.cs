using System;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected readonly Player PlayerCharacter;
        private int _timeRemaining;
        protected int Duration;
        protected Action HourCallback;
        protected Action MinuteCallback;
        public bool IsVisible = true;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name)
        {
            PlayerCharacter = playerCharacter;
        }

        public void SetButton(EnhancedButton button)
        {
            button.AddOnClick(OnClick);
        }

        protected virtual void OnClick()
        {
        }

        public void UpdateAction()
        {
            --_timeRemaining;
            MinuteCallback?.Invoke();
            UpdateActionText();
            if (_timeRemaining != 0) return;
            HourCallback?.Invoke();
            ResetTimeRemaining();
        }

        private void ResetTimeRemaining()
        {
            _timeRemaining = WorldState.MinutesPerHour;
        }

        public override void Enter()
        {
            base.Enter();
            ResetTimeRemaining();
        }

        protected string DisplayName;
        protected bool ShowTime = true;
        
        public void UpdateActionText()
        {
            string actionString = DisplayName;
            if (ShowTime) actionString += "\n" + WorldState.TimeToHours(Duration);
            PlayerCharacter.CharacterView.UpdateCurrentActionText(actionString);
        }
    }
}