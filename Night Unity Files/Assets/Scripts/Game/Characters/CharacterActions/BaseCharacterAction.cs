using System;
using Game.World;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected int DefaultDuration;
        private int TimeRemaining;
        private int UpdateInterval = 1;
        protected bool IsVisible = true;
        private bool Interrupted;
        public GameObject ActionButtonGameObject;
        protected Action HourCallback;
        protected Action MinuteCallback;
        private string _stateTransitionTarget = "Idle";
        protected Player PlayerCharacter;

        protected BaseCharacterAction(string name, Player playerCharacter) : base(name, StateSubtype.Character)
        {
            PlayerCharacter = playerCharacter;
            DefaultDuration = WorldState.MinutesPerHour;
            AddOnExit(() => WorldState.UnregisterMinuteEvent(Update));
        }

        public override ViewParent CreateUi(Transform parent)
        {
            SimpleView ui = new SimpleView(this, parent);
            ui.SetPreferredHeight(30);
            ui.SetCentralTextCallback(() => Name);
            ui.PrimaryButton.AddOnClick(() =>
            {
                PlayerCharacter.CharacterView.CollapseCharacterButton.Select();
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
            {
                return false;
            }
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

        private void UpdateAction()
        {
            --TimeRemaining;
            if (TimeRemaining == 0)
            {
                WorldState.UnregisterMinuteEvent(UpdateAction);
                if (_stateTransitionTarget != null)
                {
                    GetCharacter().States.NavigateToState(_stateTransitionTarget);
                }
                else
                {
                    GetCharacter().States.ReturnToDefault();
                }
            }
            MinuteCallback?.Invoke();
            if (TimeRemaining % (WorldState.MinutesPerHour / UpdateInterval) != 0) return;
            HourCallback?.Invoke();
        }

        public override void Exit()
        {
            if (!Interrupted)
            {
                base.Exit();
            }
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