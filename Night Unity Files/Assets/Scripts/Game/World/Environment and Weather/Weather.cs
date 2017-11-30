using System;
using System.Collections.Generic;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;

namespace Game.World.Environment_and_Weather
{
    public class Weather : ProbabalisticState
    {
        private int _temperature, _visibility, _water, _food, _duration;
        private List<Danger> _dangers = new List<Danger>();
        private int _timeRemaining;
        public WeatherAttributes Attributes;

        public Weather(string name, int temperature, int visibility, int water, int food, int duration) : base(name, StateSubtype.Weather)
        {
            _temperature = temperature;
            _visibility = visibility;
            _water = water;
            _food = food;
            _duration = duration;
        }

        public void AddDanger(string dangerType, float severity)
        {
            _dangers.Add(new Danger(dangerType, severity));
        }

        public override void Enter()
        {
            _timeRemaining = _duration * WorldState.MinutesPerHour;
            WeatherSystemController.Instance().ChangeWeather(this, _timeRemaining);
        }

        public int Temperature()
        {
            return _temperature;
        }

        public void UpdateWeather()
        {
            --_timeRemaining;
            if (_timeRemaining != 0) return;
            UpdateEnvironmentResources();
            NavigateToState(Name);
        }

        private void UpdateEnvironmentResources()
        {
            List<Region.Region> discoveredRegions = RegionManager.GetDiscoveredRegions();
            discoveredRegions.ForEach(r =>
            {
                r.AddWater(_water);
                r.AddFood(_food);
            });
        }

        protected override void NavigateToState(string name)
        {
            WeatherManager.Instance().NavigateToState(WeatherManager.Instance().CalculateNextState());
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