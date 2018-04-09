using System;
using System.Collections.Generic;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;

namespace Game.Exploration.Weather
{
    public class Weather : ProbabalisticState
    {
        private readonly int _temperature, _water, _food, _duration;
        private readonly float _visibility;
        private int _timeRemaining;
        public WeatherAttributes Attributes;

        public Weather(StateMachine stateMachine, string name, int temperature, int visibility, int water, int food, int duration) : base(stateMachine, name, StateSubtype.Weather)
        {
            _temperature = temperature;
            _visibility = visibility;
            _water = water;
            _food = food;
            _duration = duration;
        }

        public float GetVisibility()
        {
            return _visibility;
        }

       public override void Enter()
        {
            base.Enter();
            WorldView.SetWeatherText(Name);
            _timeRemaining = _duration * WorldState.MinutesPerHour;
            WeatherSystemController.Instance().ChangeWeather(this, _timeRemaining);
        }

        public int Temperature()
        {
            return _temperature;
        }

        public void Update()
        {
            --_timeRemaining;
            if (_timeRemaining != 0) return;
            UpdateEnvironmentResources();
            WeatherManager.GoToWeather();
        }

        private void UpdateEnvironmentResources()
        {
            //todo me
//            List<Region.Region> discoveredRegions = RegionManager.GetDiscoveredRegions();
//            discoveredRegions.ForEach(r =>
//            {
//                r.AddWater(_water);
//                r.AddFood(_food);
//            });
        }

        private class Danger
        {
            private DangerType _dangerType;
            private float _severity;

            public Danger(string dangerType, float severity)
            {
                foreach (DangerType type in Enum.GetValues(typeof(DangerType)))
                    if (type.ToString() == dangerType)
                    {
                        _dangerType = type;
                        break;
                    }

                _severity = severity;
            }

            private enum DangerType
            {
                Acid,
                Physical,
                Mental,
                Lightning,
                Fire,
                Cold
            }
        }
    }
}