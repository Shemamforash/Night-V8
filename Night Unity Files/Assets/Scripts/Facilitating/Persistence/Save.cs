using System.IO;
using System.Xml;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.Persistence
{
    public class Save
    {
        private readonly string RealTime, GameTime, GameLocation;
        private readonly XmlNode _root;
        private readonly bool _valid;
        private bool _loading;
        public readonly int TotalTime;
        private bool _mostRecent;

        public Save(string location)
        {
            if (!File.Exists(location)) return;
            _valid = true;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(location);
            _root = xmlDoc.GetNode("BTVSave");
            XmlNode worldValues = _root.GetNode("WorldState");
            RealTime = worldValues.StringFromNode("RealTime");
            int days = worldValues.IntFromNode("Days");
            int hours = worldValues.IntFromNode("Hours");
            int minutes = worldValues.IntFromNode("Hours");
            TotalTime = days * 24 * 60 + hours * 60 + minutes;
            GameTime = "Day " + days + " - ";
            string hourTime = WorldView.TimeToName(hours);
            GameTime += hourTime;
            GameLocation = _root.StringFromNode("CurrentEnvironment");
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
    }
}