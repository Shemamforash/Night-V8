using System;
using Game.World;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        private string _stateTransitionTarget = "Idle";
        public GameObject ActionButtonGameObject;
        protected int DefaultDuration;
        protected Action HourCallback;
        private bool Interrupted;
        protected bool IsVisible = true;
        protected Action MinuteCallback;
        protected Player PlayerCharacter;
        private int TimeRemaining;
        private readonly int UpdateInterval = 1;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(name, StateSubtype.Character)
        {
            PlayerCharacter = playerCharacter;
            DefaultDuration = WorldState.MinutesPerHour;
            AddOnExit(() => WorldState.UnregisterMinuteEvent(Update));
        }

        public override ViewParent CreateUi(Transform parent)
        {
            SimpleView ui = new SimpleView(this, parent, "Prefabs/Player Action");
            ui.SetPreferredHeight(30);
            ui.SetCentralTextCallback(() => Name);
            ui.PrimaryButton.AddOnClick(() =>
            {
                NavigateToState(Name);
            });
            return ui;
        }

        protected override void NavigateToState(string stateName)
        {
            PlayerCharacter.States.NavigateToState(stateName);
        }

        protected override void ReturnToDefault()
        {
            PlayerCharacter.States.ReturnToDefault();
        }

        public bool SetDuration(int hours)
        {
            TimeRemaining = WorldState.MinutesPerHour * hours;
            return true;
        }

        public void SetStateTransitionTarget(string stateTransitionTarget)
        {
            _stateTransitionTarget = stateTransitionTarget;
        }

        public bool DecreaseDuration()
        {
            if (TimeRemaining == WorldState.MinutesPerHour) 
                return false;
            TimeRemaining -= WorldState.MinutesPerHour;
            return true;
        }

        public void Start()
        {
            WorldState.RegisterMinuteEvent(UpdateAction);
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
                GetCharacter().States.NavigateToState(_stateTransitionTarget);
            else
                GetCharacter().States.ReturnToDefault();
        }

        private void UpdateAction()
        {
            --TimeRemaining;
            if (TimeRemaining <= 0)
                FinishUpdate();
            MinuteCallback?.Invoke();
            if (TimeRemaining % (WorldState.MinutesPerHour / UpdateInterval) != 0) return;
            HourCallback?.Invoke();
        }

        public override void Exit()
        {
            if (!Interrupted)
                base.Exit();
        }

        private int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling((float) TimeRemaining / WorldState.MinutesPerHour);
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