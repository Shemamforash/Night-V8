using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Game.Global;
using Extensions;
using Environment = Game.Exploration.Environment.Environment;

namespace Facilitating.Persistence
{
	public class Save
	{
		private readonly XmlNode _root;
		private readonly bool    _valid;
		private readonly string  RealTime, GameTime, GameLocation;
		private          bool    _loading;
		private          bool    _mostRecent;

		public Save(string location)
		{
			if (!File.Exists(location)) return;
			_valid = true;
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(location);
			_root = xmlDoc.GetChild("BTVSave");
			XmlNode worldValues = _root.GetChild("WorldState");
			RealTime = worldValues.ParseString("RealTime");
			int days  = worldValues.ParseInt("Days");
			int hours = worldValues.ParseInt("Hours");
			GameTime = "Day " + (days + 1) + " - ";
			string hourTime = WorldView.TimeToName(hours);
			GameTime     += hourTime;
			GameLocation =  _root.ParseString("CurrentEnvironment");
		}

		public bool Valid() => _valid;

		public string GetRealTime()
		{
			string createdOn = "-";
			if (_valid)
			{
				createdOn = "Created on " + RealTime;
				if (_mostRecent) createdOn += " (More Recent)";
			}

			return createdOn;
		}

		public void SetMostRecent()
		{
			_mostRecent = true;
		}

		public string GetGameInfo()
		{
			if (!_valid) return "-";
			string gameInfo = Environment.EnvironmentTypeToName(GameLocation) + " - " + GameTime;
			return gameInfo;
		}

		public void LoadFromSave()
		{
			if (!_valid || _loading) return;
			_loading = true;
			WorldState.Load(_root);
			GameController.StartGame(false);
		}

		public bool IsMostRecent() => _mostRecent;

		public bool MoreRecentThan(Save other)
		{
			DateTime thisTime  = DateTime.ParseExact(RealTime,       "MMMM dd '-' hh:mm tt", CultureInfo.InvariantCulture);
			DateTime otherTime = DateTime.ParseExact(other.RealTime, "MMMM dd '-' hh:mm tt", CultureInfo.InvariantCulture);
			return DateTime.Compare(thisTime, otherTime) > 0;
		}
	}
}