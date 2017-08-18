using Characters;
using Game.Combat;
using Game.World;
using SamsHelper.ReactiveUI;
using UnityEngine;
using World;

namespace Game.Characters
{
    public abstract class CharacterAction
    {
        private readonly string _actionName;
        public GameObject ActionObject;
        private readonly bool _durationFixed;
        private int _duration;
        private TimeListener _timeListener = new TimeListener();
        private MyFloat _timeRemaining = new MyFloat();
        private Character _parent;

        public int Duration
        {
            get { return _duration; }
            set
            {
                if (!_durationFixed)
                {
                    _duration = value;
                }
            }
        }

        public Character Parent()
        {
            return _parent;
        }

        protected CharacterAction(string actionName, bool durationFixed, int duration, Character parent)
        {
            _actionName = actionName;
            _durationFixed = durationFixed;
            _duration = duration;
            _timeListener.OnHour(UpdateTime);
            _parent = parent;
            _parent.CharacterUi.CurrentActionText.SetFormattingFunction(f => _actionName + " (" + (int) f + "hrs)");
            _timeRemaining.AddLinkedText( _parent.CharacterUi.CurrentActionText);
        }

        protected virtual void ExecuteAction()
        {
            _parent.CharacterUi.CurrentActionText.Text("Doing nothing");
        }

        public virtual void InitialiseAction()
        {
            _timeRemaining.Val = _duration;
            if (_duration == 0)
            {
                ExecuteAction();
            }
        }

        protected void ImmobiliseParent()
        {
            //for preventing weapon switching when exploring
        }

        public string GetActionName()
        {
            return _actionName;
        }

        public bool IsDurationFixed()
        {
            return _durationFixed;
        }

        private void UpdateTime()
        {
            if (_timeRemaining > 0)
            {
                --_timeRemaining.Val;
                if (_timeRemaining == 0)
                {
                    ExecuteAction();
                }
            }
        }
        
        public class FindResources : CharacterAction
        {
            public FindResources(Character c) : base("Find Resources", false, 1, c)
            {
            }

            protected override void ExecuteAction()
            {
                base.ExecuteAction();
                Home.IncrementResource(ResourceType.Water, 1);
                Home.IncrementResource(ResourceType.Food, 1);
            }
        }

        public class Hunt : CharacterAction
        {
            public Hunt(Character c) : base("Hunt", true, 0, c)
            {
            }

            protected override void ExecuteAction()
            {
                base.ExecuteAction();
                CombatManager.EnterCombat(_parent);
            }
        }
    }
}