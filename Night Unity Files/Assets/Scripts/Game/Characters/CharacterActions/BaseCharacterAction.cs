using System;
using Characters;
using Facilitating.MenuNavigation;
using Game.World;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.ReactiveUI;
using UnityEngine;
using World;

namespace Game.Characters.CharacterActions
{
    public class BaseCharacterAction : State
    {
        protected int _defaultDuration;
        private readonly MyFloat _timeRemaining = new MyFloat();
        protected bool IsDurationFixed, Ready;
        private readonly TimeListener _timeListener = new TimeListener();
        public GameObject ActionButtonGameObject;
        protected Character Character;

        public BaseCharacterAction(string name, Character character) : base(name, character)
        {
            Character = character;
            _timeListener.OnMinute(UpdateAction);
            _defaultDuration = WorldTime.MinutesPerHour;
            _timeRemaining.AddLinkedText(Character.CharacterUi.CurrentActionText);
        }

        public virtual bool IncreaseDuration()
        {
            _timeRemaining.Val += WorldTime.MinutesPerHour;
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
                GameMenuNavigator.MenuNavigator.ShowActionDurationMenu(this);
            }
        }

        public void Start()
        {
            Ready = true;
        }

        public void UpdateAction()
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
        }

        public int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling(_timeRemaining / WorldTime.MinutesPerHour);
        }

        public virtual string GetCostAsString()
        {
            return TimeRemainingAsHours() + " hrs";
        }
    }
}