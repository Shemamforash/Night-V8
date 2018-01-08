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
        private bool Interrupted;
        protected bool IsVisible = true;
        protected Action MinuteCallback;
        protected Player PlayerCharacter;
        private int _timeRemaining;
        private readonly int UpdateInterval = 1;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(playerCharacter.States, name, StateSubtype.Character)
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

        public void SetDuration(int hours)
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
            Interrupted = true;
        }

        public virtual void Resume()
        {
            WorldState.RegisterMinuteEvent(UpdateAction);
            Interrupted = false;
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

        public override void Enter()
        {
            Debug.Log(Name);
            base.Enter();
        }
        
        public override void Exit()
        {
            Debug.Log(Name);
            if (!Interrupted)
                base.Exit();
        }

        private int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling((float) _timeRemaining / WorldState.MinutesPerHour);
        }

        public virtual string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs";
        }

        public bool IsStateVisible()
        {
            return IsVisible;
        }

        protected Player GetCharacter()
        {
            return PlayerCharacter;
        }
    }
}