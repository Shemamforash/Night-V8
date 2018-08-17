using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
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
        private readonly string _displayName;
        public readonly WeatherAttributes Attributes;

        public Weather(ProbabalisticStateMachine weatherStates, XmlNode weatherNode) : base(weatherStates, weatherNode.StringFromNode("Name"))
        {
            _displayName = weatherNode.StringFromNode("DisplayName");
            _temperature = weatherNode.IntFromNode("Temperature");
            _visibility = weatherNode.FloatFromNode("Visibility");
            _water = weatherNode.FloatFromNode("Water");
            _food = weatherNode.FloatFromNode("Food");
            _duration = weatherNode.IntFromNode("Duration");
            Attributes = new WeatherAttributes(weatherNode);
        }

        public float GetVisibility()
        {
            return _visibility;
        }

       public override void Enter()
        {
            base.Enter();
            WorldView.SetWeatherText(_displayName);
            _timeRemaining = _duration * WorldState.MinutesPerHour;
            WeatherSystemController.SetWeather(this, false);
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
            List<Region> discoveredRegions = MapGenerator.DiscoveredRegions();
            float waterChance = Mathf.Abs(_water);
            float foodChance = Mathf.Abs(_food);
            discoveredRegions.ForEach(r =>
            {
                if(Random.Range(0f, 1f) < waterChance) r.ChangeWater((int)_water.Polarity());
                if(Random.Range(0f, 1f) < foodChance) r.ChangeFood((int)_food.Polarity());
            });
        }

        public void Save(XmlNode doc)
        {
            doc = doc.CreateChild("Weather");
            doc.CreateChild("Name", Name);
            doc.CreateChild("TimeRemaining", _timeRemaining);
        }

        public void SetTimeRemaining(int timeRemaining)
        {
            _timeRemaining = timeRemaining;
            WeatherSystemController.SetWeather(this, true);
        }
    }
}