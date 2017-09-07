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
        protected bool PlayerSetsDuration, IsVisible = true, Interrupted = false;
        public GameObject ActionButtonGameObject;
        protected Action HourCallback;

        public BaseCharacterAction(string name, Character character) : base(name, character)
        {
            DefaultDuration = WorldTime.MinutesPerHour;
        }

        public virtual bool IncreaseDuration()
        {
            return IncreaseDuration(1);
        }

        public virtual bool IncreaseDuration(int hours)
        {
            TimeRemaining += WorldTime.MinutesPerHour * hours;
            return true;
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

        public void Enter(int duration)
        {
            TimeRemaining = duration;
            if (PlayerSetsDuration)
            {
                MenuStateMachine.Instance.NavigateToState("Action Duration Menu");
            }
            ((Character)ParentMachine).SetActionListActive(false);
        }

        public void Start()
        {
            WorldTime.Instance().MinuteEvent += Update;
        }

        public void Interrupt()
        {
            WorldTime.Instance().MinuteEvent -= Update;
        }

        public void Resume()
        {
            WorldTime.Instance().MinuteEvent += Update;
        }
        
        public virtual void Update()
        {
            if (TimeRemaining > 0)
            {
                --TimeRemaining;
                TryOnUpdate();
                if (TimeRemaining % WorldTime.MinutesPerHour == 0)
                {
                    if (HourCallback != null)
                    {
                        HourCallback();
                    }
                }
                if (TimeRemaining == 0)
                {
                    Exit();
                }
            }
        }

        public void Exit(bool returnToDefault)
        {
            base.Exit();
            WorldTime.Instance().MinuteEvent -= Update;
            if(returnToDefault) ParentMachine.ReturnToDefault();
            ((Character)ParentMachine).SetActionListActive(true);
        }

        public int TimeRemainingAsHours()
        {
            return (int)Math.Ceiling((float)TimeRemaining / WorldTime.MinutesPerHour);
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
            return (Character)ParentMachine;
        }
    }
}