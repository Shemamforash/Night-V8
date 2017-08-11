using Game.Misc;
using UnityEngine;
using World;

namespace Characters
{
    public abstract class CharacterAction
    {
        private readonly string _actionName;
        public GameObject ActionObject;
        private readonly bool _durationFixed;
        private int _duration;
        private TimeListener _timeListener = new TimeListener();
        private MyFloat _timeRemaining;
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
            TextAssociation currentActionAssociation = new TextAssociation(_parent.CharacterUi.CurrentActionText,
                f => _actionName + " (" + (int) f + "hrs)", true);
            _timeRemaining = new MyFloat(_duration, currentActionAssociation);
        }

        protected virtual void ExecuteAction()
        {
            _parent.CharacterUi.CurrentActionText.text = "Doing nothing";
        }

        public virtual void InitialiseAction()
        {
            _timeRemaining.Value = _duration;
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
                --_timeRemaining.Value;
                if (_timeRemaining == 0)
                {
                    ExecuteAction();
                }
            }
        }
    }
}