using System;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected readonly Player PlayerCharacter;
        private int _timeRemaining;
        protected int Duration;
        private int InitialDuration;
        protected Action HourCallback;
        protected Action MinuteCallback;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name, GameObjectType.PlayerAction)
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

        protected void SetDuration(int duration)
        {
            Duration = duration;
            InitialDuration = Duration;
        }
        
        public void UpdateActionText()
        {
            float normalisedTime = -1;
            if (ShowTime) normalisedTime = (float)Duration / InitialDuration;
            PlayerCharacter.CharacterView.UpdateCurrentActionText(DisplayName, normalisedTime);
        }
    }
}