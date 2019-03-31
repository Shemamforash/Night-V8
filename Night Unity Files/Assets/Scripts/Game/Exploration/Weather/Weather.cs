using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;

namespace Game.Exploration.Weather
{
    public class Weather : State
    {
        private readonly int _temperature, _duration;
        private readonly float _visibility;
        public readonly int Thunder;
        private int _timeRemaining;
        private readonly string _displayName;
        public readonly WeatherAttributes Attributes;
        private readonly List<string> _weatherEventStrings;

        public Weather(StateMachine weatherStates, XmlNode weatherNode) : base(weatherStates, weatherNode.StringFromNode("Name"))
        {
            _displayName = weatherNode.StringFromNode("DisplayName");
            _temperature = weatherNode.IntFromNode("Temperature");
            _visibility = weatherNode.FloatFromNode("Visibility");
            Thunder = weatherNode.IntFromNode("Thunder");
            _duration = weatherNode.IntFromNode("Duration");
            Attributes = new WeatherAttributes(weatherNode);
//            _weatherEventStrings = weatherNode.StringFromNode("Events").Split(',').ToList();
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
            TryShowWeatherEvent();
        }

        private void TryShowWeatherEvent()
        {
            if (!Helper.RollDie(0, 3)) return;
            return;
            WorldEventManager.GenerateEvent(new WorldEvent(_weatherEventStrings.RandomElement()));
        }

        public int Temperature()
        {
            return _temperature;
        }

        public void Update()
        {
            --_timeRemaining;
            if (_timeRemaining != 0) return;
            WeatherManager.GoToWeather();
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