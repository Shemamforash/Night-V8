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
        protected int DefaultDuration;
        protected int TimeRemaining;
        protected bool PlayerSetsDuration, IsVisible = true;
        public GameObject ActionButtonGameObject;
        protected Character Character;
        protected Action HourCallback;

        public BaseCharacterAction(string name, Character character) : base(name, character)
        {
            Character = character;
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

        public override void Enter()
        {
            TimeRemaining = DefaultDuration;
            if (PlayerSetsDuration)
            {
                MenuStateMachine.Instance.NavigateToState("Action Duration Menu");
            }
        }

        public void Start()
        {
            WorldTime.Instance().MinuteEvent += Update;
            ((Character)ParentMachine).SetActionListActive(false);
        }

        public virtual void Update()
        {
            if (TimeRemaining > 0)
            {
                --TimeRemaining;
                TryUpdateCallback();
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

        public override void Exit()
        {
            WorldTime.Instance().MinuteEvent -= Update;
            ParentMachine.ReturnToDefault();
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
            return Character;
        }
    }
}