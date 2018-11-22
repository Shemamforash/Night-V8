using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
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
        protected string DisplayName;
        private EnhancedButton _button;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name)
        {
            PlayerCharacter = playerCharacter;
        }

        public void SetButton(EnhancedButton button)
        {
            _button = button;
            _button.AddOnClick(TryClick);
        }

        private void TryClick()
        {
            if (_button.IsEnabled()) OnClick();
        }

        protected virtual void OnClick()
        {
        }

        public void UpdateAction()
        {
            --_timeRemaining;
            MinuteCallback?.Invoke();
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

        protected void SetDuration(int duration = -1)
        {
            if (duration == -1) Duration = WorldState.MinutesPerHour / 2;
            else Duration = duration;
            InitialDuration = Duration;
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            doc = doc.CreateChild("CurrentAction");
            doc.CreateChild("Name", GetType().Name);
            doc.CreateChild("InitialDuration", InitialDuration);
            doc.CreateChild("Duration", Duration);
            doc.CreateChild("TimeRemaining", _timeRemaining);
            return doc;
        }

        public virtual XmlNode Load(XmlNode doc)
        {
            doc = doc.SelectSingleNode("CurrentAction");
            InitialDuration = doc.IntFromNode("InitialDuration");
            Duration = doc.IntFromNode("Duration");
            _timeRemaining = doc.IntFromNode("TimeRemaining");
            return doc;
        }

        public  virtual string GetDisplayName()
        {
            return DisplayName;
        }

        public float GetRemainingTime()
        {
            return (float) Duration / InitialDuration;
        }
    }
}