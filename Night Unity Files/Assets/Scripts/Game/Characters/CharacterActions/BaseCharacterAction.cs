using System;
using Characters;
using Facilitating.MenuNavigation;
using Game.World;
using Game.World.Time;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.CustomTypes;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected int _defaultDuration;
        protected readonly MyFloat _timeRemaining = new MyFloat();
        protected bool IsDurationFixed, Ready, IsVisible = true;
        private readonly TimeListener _timeListener = new TimeListener();
        public GameObject ActionButtonGameObject;
        protected Character Character;

        public BaseCharacterAction(string name, Character character) : base(name, character)
        {
            Character = character;
            _timeListener.OnMinute(UpdateAction);
            _defaultDuration = WorldTime.MinutesPerHour;
            _timeRemaining.AddLinkedText(Character.CharacterUi.CurrentActionText);
            _timeRemaining.AddLinkedText(Character.CharacterUi.DetailedCurrentActionText);
        }

        public virtual bool IncreaseDuration()
        {
            return IncreaseDuration(1);
        }

        public virtual bool IncreaseDuration(int hours)
        {
            _timeRemaining.Val += WorldTime.MinutesPerHour * hours;
            return true;
        }

        public virtual bool DecreaseDuration()
        {
            if (_timeRemaining == WorldTime.MinutesPerHour)
            {
                return false;
            }
            _timeRemaining.Val -= WorldTime.MinutesPerHour;
            return true;
        }

        public override void Enter()
        {
            _timeRemaining.Val = _defaultDuration;
            if (IsDurationFixed)
            {
                Ready = true;
                if (_defaultDuration == 0)
                {
                    Exit();
                }
            }
            else
            {
                MenuStateMachine.Instance.NavigateToState("Action Duration Menu");
            }
        }

        public void Start()
        {
            Ready = true;
            ((Character)ParentMachine).SetActionListActive(false);
        }

        public virtual void UpdateAction()
        {
            if (Ready && _timeRemaining > 0)
            {
                --_timeRemaining.Val;
                TryUpdateCallback();
                if (_timeRemaining == 0)
                {
                    Exit();
                }
            }
        }

        public override void Exit()
        {
            Ready = false;
            ParentMachine.ReturnToDefault();
            ((Character)ParentMachine).SetActionListActive(true);
        }

        public int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling(_timeRemaining / WorldTime.MinutesPerHour);
        }

        public virtual string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs";
        }

        public bool IsStateVisible()
        {
            return IsVisible;
        }

        public Character GetCharacter()
        {
            return Character;
        }
    }
}