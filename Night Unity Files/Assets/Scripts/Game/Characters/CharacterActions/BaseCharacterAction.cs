using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public abstract class BaseCharacterAction : State
    {
        protected readonly Player PlayerCharacter;
        private int _timeToNextHour;
        protected int Duration;
        private int InitialDuration;
        protected Action HourCallback;
        protected Action MinuteCallback;
        protected string DisplayName;
        private EnhancedButton _button;
        public bool ForceViewUpdate;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name)
        {
            PlayerCharacter = playerCharacter;
        }

        public virtual void SetButton(EnhancedButton button)
        {
            _button = button;
            _button.AddOnClick(TryClick);
        }

        private void TryClick()
        {
            if (_button.IsEnabled()) OnClick();
        }

        protected abstract void OnClick();

        public void UpdateAction()
        {
            --_timeToNextHour;
            MinuteCallback?.Invoke();
            if (_timeToNextHour != 0) return;
            HourCallback?.Invoke();
            ResetTimeRemaining();
        }

        private void ResetTimeRemaining()
        {
            _timeToNextHour = WorldState.MinutesPerHour;
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
            doc.CreateChild("TimeRemaining", _timeToNextHour);
            return doc;
        }

        public virtual XmlNode Load(XmlNode doc)
        {
            doc = doc.SelectSingleNode("CurrentAction");
            InitialDuration = doc.IntFromNode("InitialDuration");
            Duration = doc.IntFromNode("Duration");
            _timeToNextHour = doc.IntFromNode("TimeRemaining");
            return doc;
        }

        public virtual string GetDisplayName()
        {
            return DisplayName;
        }

        public virtual float GetRemainingDuration() => Duration;

        public float GetInitialDuration() => InitialDuration;
    }
}