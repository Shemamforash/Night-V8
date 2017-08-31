using System;
using System.Collections.Generic;
using Game.World.Time;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.CustomTypes;

namespace Game.World.Weather
{
    public class Weather : ProbabalisticState
    {
        private MyFloat _temperature, _visibility, _water, _duration;
        private List<Danger> _dangers = new List<Danger>();
        private int _timeRemaining;
        
        public Weather(string name, MyFloat temperature, MyFloat visibility, MyFloat water, MyFloat duration) : base(name, WeatherManager.GetWeatherManager())
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
            _timeRemaining = (int)(_duration.RandomInRange() * WorldTime.MinutesPerHour);
        }

        public void UpdateWeather()
        {
            --_timeRemaining;
            if (_timeRemaining == 0)
            {
                Exit();
            }
        }

        public override void Exit()
        {
            WeatherManager.GetWeatherManager().NavigateToState(NextState(Name()));
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