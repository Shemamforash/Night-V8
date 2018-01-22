using System;
using Game.World;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        private BaseCharacterAction _stateTransitionTarget;
        public GameObject ActionButtonGameObject;
        protected Action HourCallback;
        private bool _interrupted;
        protected bool IsVisible = true;
        protected Action MinuteCallback;
        protected readonly Player.Player PlayerCharacter;
        private int _timeRemaining;
        private const int UpdateInterval = 1;

        protected BaseCharacterAction(string name, Player.Player playerCharacter) : base(playerCharacter.States, name, StateSubtype.Character)
        {
            PlayerCharacter = playerCharacter;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            SimpleView ui = new SimpleView(this, parent, "Prefabs/Player Action");
            ui.SetPreferredHeight(30);
            ui.SetCentralTextCallback(() => Name);
            ui.PrimaryButton.AddOnClick(Enter);
            return ui;
        }

        protected void SetDuration(int hours)
        {
            _timeRemaining = WorldState.MinutesPerHour * hours;
        }

        public void SetStateTransitionTarget(BaseCharacterAction stateTransitionTarget)
        {
            _stateTransitionTarget = stateTransitionTarget;
        }

        public bool DecreaseDuration()
        {
            if (_timeRemaining == WorldState.MinutesPerHour) 
                return false;
            _timeRemaining -= WorldState.MinutesPerHour;
            return true;
        }

        protected void Start()
        {
            WorldState.RegisterMinuteEvent(UpdateAction);
            if(_timeRemaining == 0) FinishUpdate();
        }

        public virtual void Interrupt()
        {
            WorldState.UnregisterMinuteEvent(UpdateAction);
            _interrupted = true;
        }

        public virtual void Resume()
        {
            WorldState.RegisterMinuteEvent(UpdateAction);
            _interrupted = false;
        }

        private void FinishUpdate()
        {
            WorldState.UnregisterMinuteEvent(UpdateAction);
            if (_stateTransitionTarget != null)
            {
                _stateTransitionTarget.Enter();
            }
            else
            {
                GetCharacter().States.ReturnToDefault();
            }
        }

        private void UpdateAction()
        {
            --_timeRemaining;
            MinuteCallback?.Invoke();
            if (_timeRemaining % (WorldState.MinutesPerHour / UpdateInterval) == 0) HourCallback?.Invoke();
            if (_timeRemaining == 0)
                FinishUpdate();
        }

        public override void Exit()
        {
            if (!_interrupted)
                base.Exit();
        }

        private int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling((float) _timeRemaining / WorldState.MinutesPerHour);
        }

        public virtual string GetActionText()
        {
            return TimeRemainingAsHours() + " hrs";
        }

        public bool IsStateVisible()
        {
            return IsVisible;
        }

        protected Player.Player GetCharacter()
        {
            return PlayerCharacter;
        }
    }
}