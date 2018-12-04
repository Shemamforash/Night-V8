using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;

namespace Game.Exploration.Weather
{
    public class Weather : State
    {
        private readonly int _temperature, _duration;
        private readonly float _visibility;
        public readonly int Water, Fog, Ice, Thunder;
        private int _timeRemaining;
        private readonly string _displayName;
        public readonly WeatherAttributes Attributes;

        public Weather(StateMachine weatherStates, XmlNode weatherNode) : base(weatherStates, weatherNode.StringFromNode("Name"))
        {
            _displayName = weatherNode.StringFromNode("DisplayName");
            _temperature = weatherNode.IntFromNode("Temperature");
            _visibility = weatherNode.FloatFromNode("Visibility");
            Water = weatherNode.IntFromNode("Water");
            Fog = weatherNode.IntFromNode("Fog");
            Ice = weatherNode.IntFromNode("Ice");
            Thunder = weatherNode.IntFromNode("Thunder");
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