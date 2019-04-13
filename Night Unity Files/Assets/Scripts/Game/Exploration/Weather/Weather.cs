using System;
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
        private static readonly List<string> _weatherEventStrings;

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
            WorldEventManager.GenerateEvent(new WorldEvent(GetWeatherEventString()));
        }

        private string GetWeatherEventString()
        {
            string[] events;
            switch (Name)
            {
                case "Drizzle":
                    events = new[] {"A veil of cold rain sweeps across the land", "Light rain soothes the dry ground"};
                    break;
                case "Rain":
                    events = new[] {"Rain again, enough to dampen the spirits", "A small rain shower passes overhead"};
                    break;
                case "Downpour":
                    events = new[] {"When it rains, it pours", "The heavens are open", "A deluge descends on us"};
                    break;
                case "Haze":
                    events = new[] {"A light haze obscures the land", "A dreamlike haze falls upon us"};
                    break;
                case "Mist":
                    events = new[] {"It'll be a struggle to see much in this mist", "A misty pall envelopes the world"};
                    break;
                case "Fog":
                    events = new[] {"A Heavy fog settles", "Fog- cold and wet, much worse than rain"};
                    break;
                case "Clear":
                    events = new[] {"Clear weather again", "The suns shine once more", "The clear sky reveals the stars above"};
                    break;
                case "Cloudy":
                    events = new[] {"A few clouds dot the sky", "Puffs of cloud glide over us", "Clouds bring welcome relief"};
                    break;
                case "Overcast":
                    events = new[] {"The sky turns grey", "The clouds cast a dimness across the land"};
                    break;
                case "Hail":
                    events = new[] {"Ice falls painfully from the sky", "Hail viciously strikes the ground", "Freezing rain lashes at exposed skin"};
                    break;
                case "Duststorm":
                    events = new[] {"An angry dust cloud blasts through the land", "Dust and sand rips across the world"};
                    break;
                case "Lightning":
                    events = new[] {"Lightning strikes and thunder roars.", "The thunder awakens primal terror inside"};
                    break;
                case "Heatwave":
                    events = new[] {"The earth burns beneath the scorching sky", "The air seems to crackle in the heat"};
                    break;
                case "Icestorm":
                    events = new[] {"Ice laden winds whip around me", "A polar storm suddenly descends"};
                    break;
                case "Hurricane":
                    events = new[] {"The winds howl and the sky roils", "Hurricane winds blow"};
                    break;
                default:
                    throw new Exception("Unknown weather: " + Name);
            }

            return events.RandomElement();
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