using System;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Characters.CharacterActions
{
	public abstract class BaseCharacterAction : State
	{
		protected readonly Player         PlayerCharacter;
		private            EnhancedButton _button;
		private            int            _initialDuration;
		private            int            _timeToNextHour;
		protected          string         DisplayName;
		protected          int            Duration;
		public             bool           ForceViewUpdate;
		protected          Action         HourCallback;
		protected          Action         MinuteCallback;

		protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name) => PlayerCharacter = playerCharacter;

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
			if (duration == -1)
			{
				Duration = WorldState.MinutesPerHour / 2;
			}
			else
			{
				Duration = duration;
			}

			_initialDuration = Duration;
		}

		public virtual XmlNode Save(XmlNode doc)
		{
			doc = doc.CreateChild("CurrentAction");
			doc.CreateChild("Name",            GetType().Name);
			doc.CreateChild("InitialDuration", _initialDuration);
			doc.CreateChild("Duration",        Duration);
			doc.CreateChild("TimeRemaining",   _timeToNextHour);
			return doc;
		}

		public virtual XmlNode Load(XmlNode doc)
		{
			doc              = doc.SelectSingleNode("CurrentAction");
			_initialDuration = doc.ParseInt("InitialDuration");
			Duration         = doc.ParseInt("Duration");
			_timeToNextHour  = doc.ParseInt("TimeRemaining");
			return doc;
		}

		public virtual string GetDisplayName() => DisplayName;

		public virtual float GetNormalisedProgress() => Duration / (float) _initialDuration;

		public float GetRealTimeRemaining() => Duration * WorldState.MinuteInSeconds;
	}
}