using System.Collections.Generic;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Weather
{
    public class Weather : ProbabalisticState
    {
        private readonly int _temperature, _duration;
        private readonly float _visibility, _water, _food;
        private int _timeRemaining;
        public WeatherAttributes Attributes;

        public Weather(StateMachine stateMachine, string name, int temperature, float visibility, float water, float food, int duration) : base(stateMachine, name, StateSubtype.Weather)
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
            List<Region> discoveredRegions = MapGenerator.DiscoveredNodes();
            float waterChance = Mathf.Abs(_water);
            float foodChance = Mathf.Abs(_food);
            discoveredRegions.ForEach(r =>
            {
                if(Random.Range(0f, 1f) < waterChance) r.ChangeWater((int)Helper.Polarity(_water));
                if(Random.Range(0f, 1f) < foodChance) r.ChangeFood((int)Helper.Polarity(_food));
            });
        }
    }
}