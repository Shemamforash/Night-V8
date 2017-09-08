using System;
using System.Threading;
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
        protected int DefaultDuration;
        protected int TimeRemaining;
        protected int UpdateInterval = 1;
        protected bool IsVisible = true, Interrupted = false;
        public GameObject ActionButtonGameObject;
        protected Action HourCallback;
        private string _stateTransitionTarget;

        public BaseCharacterAction(string name, Character character) : base(name, character)
        {
            DefaultDuration = WorldTime.MinutesPerHour;
            AddOnExit(() => WorldTime.Instance().MinuteEvent -= Update);
        }

        public virtual bool SetDuration(int hours)
        {
            TimeRemaining = WorldTime.MinutesPerHour * hours;
            return true;
        }

        public void SetStateTransitionTarget(string stateTransitionTarget)
        {
            _stateTransitionTarget = stateTransitionTarget;
        }

        public virtual bool DecreaseDuration()
        {
            if (TimeRemaining == WorldTime.MinutesPerHour)
            {
                return false;
            }
            TimeRemaining -= WorldTime.MinutesPerHour;
            return true;
        }

        public void Start()
        {
            WorldTime.Instance().MinuteEvent += UpdateAction;
        }

        public virtual void Interrupt()
        {
            WorldTime.Instance().MinuteEvent -= UpdateAction;
            Interrupted = true;
        }

        public virtual void Resume()
        {
            WorldTime.Instance().MinuteEvent += UpdateAction;
            Interrupted = false;
        }

        public virtual void UpdateAction()
        {
            --TimeRemaining;
            if (TimeRemaining == 0)
            {
                WorldTime.Instance().MinuteEvent -= UpdateAction;
                GetCharacter().NavigateToState(_stateTransitionTarget);
            }
            if (TimeRemaining % (WorldTime.MinutesPerHour / UpdateInterval) == 0)
            {
                if (HourCallback != null)
                {
                    HourCallback();
                }
            }
        }

        public override void Exit()
        {
            if (!Interrupted)
            {
                base.Exit();
            }
        }

        public int TimeRemainingAsHours()
        {
            return (int) Math.Ceiling((float) TimeRemaining / WorldTime.MinutesPerHour);
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
            return (Character) ParentMachine;
        }
    }
}