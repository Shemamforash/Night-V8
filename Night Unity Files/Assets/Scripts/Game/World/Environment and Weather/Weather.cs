using System;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.CustomTypes;

namespace Game.World.Environment_and_Weather
{
    public class Weather : ProbabalisticState
    {
        private MyInt _temperature, _visibility, _water, _duration;
        private List<Danger> _dangers = new List<Danger>();
        private int _timeRemaining;
        public WeatherAttributes Attributes;
        
        public Weather(string name, MyInt temperature, MyInt visibility, MyInt water, MyInt duration) : base(name, StateSubtype.Weather, WeatherManager.Instance())
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
            _timeRemaining = _duration.RandomInRange() * WorldState.MinutesPerHour;
            WeatherSystemController.Instance().ChangeWeather(this, _timeRemaining);
        }

        public int Temperature()
        {
            return _temperature.GetCurrentValue();
        }

        public void UpdateWeather()
        {
            --_timeRemaining;
            if (_timeRemaining == 0)
            {
                WeatherManager.Instance().NavigateToState(NextState(Name));
            }
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