using System.IO;
using System.Xml;
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

        public Save(string location)
        {
            if (!File.Exists(location)) return;
            _valid = true;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(location);
            _root = xmlDoc.GetNode("BTVSave");
            XmlNode worldValues = _root.GetNode("WorldState");
            RealTime = worldValues.StringFromNode("RealTime");
            GameTime = "Day " + worldValues.IntFromNode("Days") + " - ";
            int hours = worldValues.IntFromNode("Hours");
            int minutes = worldValues.IntFromNode("Minutes");
            GameTime += hours.ToString("D2") + ":" + minutes.ToString("D2");
            GameLocation = _root.StringFromNode("CurrentEnvironment");
        }

        public string GetRealTime()
        {
            if (!_valid) return "-";
            return "Created on " + RealTime;
        }

        public string GetGameInfo()
        {
            if (!_valid) return "-";
            return GameLocation + " - " + GameTime;
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