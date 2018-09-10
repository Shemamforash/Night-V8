using System;
using System.Xml;
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
        private int InitialDuration;
        protected Action HourCallback;
        protected Action MinuteCallback;
        private float _startTime, _endTime;

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
            if(Duration == InitialDuration) PlayerCharacter.CharacterView.SetCurrentAction(DisplayName, _startTime, _endTime);
            --_timeRemaining;
            MinuteCallback?.Invoke();
            if (_timeRemaining != 0) return;
            HourCallback?.Invoke();
            ResetTimeRemaining();
        }

        private void ResetTimeRemaining()
        {
            _timeRemaining = WorldState.MinutesPerHour;
            _startTime = WorldState.GetTimeSinceStart();
            _endTime = WorldState.GetTimeInXMinutes(Duration);
        }

        public override void Enter()
        {
            base.Enter();
            ResetTimeRemaining();
        }

        protected string DisplayName;

        protected void SetDuration(int duration = -1)
        {
            if (duration == -1) Duration = WorldState.MinutesPerHour / 2;
            else Duration = duration; 
            InitialDuration = Duration;
        }
        

        public void Save(XmlNode doc)
        {
        }
    }
}