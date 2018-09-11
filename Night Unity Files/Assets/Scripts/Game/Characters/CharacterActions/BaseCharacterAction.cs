using System;
using System.Xml;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

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
            Debug.Log(Duration + " " + InitialDuration);
//            PlayerCharacter.CharacterView().UpdateCurrentActionText((float) (Duration - 1) / InitialDuration);
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

        protected void SetDuration(int duration = -1)
        {
            if (duration == -1) Duration = WorldState.MinutesPerHour / 2;
            else Duration = duration;
            InitialDuration = Duration;
        }


        public void Save(XmlNode doc)
        {
        }

        public string GetDisplayName()
        {
            return DisplayName;
        }

        public float GetRemainingTime(int offset = 0)
        {
            return (float)(Duration + offset) / InitialDuration;
        }
    }
}