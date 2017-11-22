using System;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;

namespace Game.World.Environment_and_Weather
{
    public class Weather : ProbabalisticState
    {
        private MyValue _temperature, _visibility, _water, _duration;
        private List<Danger> _dangers = new List<Danger>();
        private int _timeRemaining;
        public WeatherAttributes Attributes;

        public Weather(string name, MyValue temperature, MyValue visibility, MyValue water, MyValue duration) : base(name, StateSubtype.Weather)
        {
            _temperature = temperature;
            _visibility = visibility;
            _water = water;
            _duration = duration;
        }

        public void AddDanger(string dangerType, float severity)
        {
            _dangers.Add(new Danger(dangerType, severity));
        }

        public override void Enter()
        {
            _temperature.SetCurrentValue(_temperature.RandomInRange());
            _timeRemaining = (int) (_duration.RandomInRange() * WorldState.MinutesPerHour);
            WeatherSystemController.Instance().ChangeWeather(this, _timeRemaining);
        }

        public int Temperature()
        {
            return (int) _temperature.CurrentValue();
        }

        public void UpdateWeather()
        {
            --_timeRemaining;
            if (_timeRemaining == 0)
            {
                NavigateToState(Name);
            }
        }

        protected override void NavigateToState(string name)
        {
            WeatherManager.Instance().NavigateToState(Name);
        }

        private class Danger
        {
            private enum DangerType
            {
                Acid,
                Physical,
                Mental,
                Lightning,
                Fire,
                Cold
            }

            private DangerType _dangerType;
            private float _severity;

            public Danger(string dangerType, float severity)
            {
                foreach (DangerType type in Enum.GetValues(typeof(DangerType)))
                {
                    if (type.ToString() == dangerType)
                    {
                        _dangerType = type;
                        break;
                    }
                }
                _severity = severity;
            }
        }
    }
}