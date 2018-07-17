using System.Collections.Generic;
using System.Xml;
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

        public Weather(ProbabalisticStateMachine weatherStates, XmlNode weatherNode) : base(weatherStates, weatherNode.SelectSingleNode("Name").InnerText)
        {
            Name = weatherNode.SelectSingleNode("Name").InnerText;
            _displayName = weatherNode.SelectSingleNode("DisplayName").InnerText;
            _temperature = int.Parse(weatherNode.SelectSingleNode("Temperature").InnerText);
            _visibility = float.Parse(weatherNode.SelectSingleNode("Visibility").InnerText);
            _water = float.Parse(weatherNode.SelectSingleNode("Water").InnerText);
            _food = float.Parse(weatherNode.SelectSingleNode("Food").InnerText);
            _duration = int.Parse(weatherNode.SelectSingleNode("Duration").InnerText);
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