using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.WorldEvents;
using Game.Global;
using Extensions;
using SamsHelper.BaseGameFunctionality.StateMachines;

namespace Game.Exploration.Weather
{
	public class Weather : State
	{
		private static readonly List<string>      _weatherEventStrings;
		private readonly        string            _displayName;
		private readonly        int               _duration;
		private readonly        float             _visibility;
		public readonly         WeatherAttributes Attributes;
		public readonly         int               Thunder;
		private                 int               _timeRemaining;

		public Weather(StateMachine weatherStates, XmlNode weatherNode) : base(weatherStates, weatherNode.ParseString("Name"))
		{
			_displayName = weatherNode.ParseString("DisplayName");
			_visibility  = weatherNode.ParseFloat("Visibility");
			Thunder      = weatherNode.ParseInt("Thunder");
			_duration    = weatherNode.ParseInt("Duration");
			Attributes   = new WeatherAttributes(weatherNode);
//            _weatherEventStrings = weatherNode.ParseString("Events").Split(',').ToList();
		}

		public float GetVisibility() => _visibility;

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
			if (!NumericExtensions.RollDie(0, 3)) return;
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
					events = new[] {"Lightning strikes and thunder roars", "The thunder awakens primal terror inside"};
					break;
				case "Heatwave":
					events = new[] {"The earth burns beneath the scorching sky", "The air seems to crackle in the heat"};
					break;
				case "Icestorm":
					events = new[] {"Ice laden winds whip around me", "A polar storm descends suddenly"};
					break;
				case "Hurricane":
					events = new[] {"The winds howl and the sky roils", "Hurricane winds blow"};
					break;
				default:
					throw new Exception("Unknown weather: " + Name);
			}

			return events.RandomElement();
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
			doc.CreateChild("Name",          Name);
			doc.CreateChild("TimeRemaining", _timeRemaining);
		}

		public void SetTimeRemaining(int timeRemaining)
		{
			_timeRemaining = timeRemaining;
			WeatherSystemController.SetWeather(this, true);
		}
	}
}