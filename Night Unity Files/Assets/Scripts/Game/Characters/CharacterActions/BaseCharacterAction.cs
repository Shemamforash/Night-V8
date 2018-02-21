using System;
using Game.World;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected Action HourCallback;
        public bool IsVisible = true;
        protected readonly Player.Player PlayerCharacter;
        private int _timeRemaining;
        protected int Duration;

        protected BaseCharacterAction(string name, Player.Player playerCharacter) : base(playerCharacter.States, name)
        {
            PlayerCharacter = playerCharacter;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            SimpleView ui = new SimpleView(this, parent, "Prefabs/Player Action");
            ui.SetPreferredHeight(30);
            ui.SetCentralTextCallback(() => Name);
            ui.PrimaryButton.AddOnClick(OnClick);
            return ui;
        }

        protected virtual void OnClick()
        {
            
        }

        public void UpdateAction()
        {
            --_timeRemaining;
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
        
        private int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling((float) _timeRemaining / WorldState.MinutesPerHour);
        }

        public virtual string GetActionText()
        {
            return TimeRemainingAsHours() + " hrs";
        }
    }
}