using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Exploration.WorldEvents
{
    public class WorldEventManager : MonoBehaviour
    {
        private static TextMeshProUGUI _eventLogText;
        private static List<string> _eventLog = new List<string>();

        public static void Load(XmlNode doc)
        {
            string events = doc.GetNodeText("WorldEvents");
            _eventLog = new List<string>(events.Split(','));
            PrintFirstFourEvents();
        }

        public static XmlNode Save(XmlNode doc)
        {
            string commaSeperatedEvents = "";
            for (int i = 0; i < _eventLog.Count; ++i)
            {
                commaSeperatedEvents += _eventLog[i];
                if (i < _eventLog.Count - 1) commaSeperatedEvents += ",";
            }

            doc.CreateChild("WorldEvents", commaSeperatedEvents);
            return doc;
        }

        public static void Reset()
        {
            _eventLog.Clear();
        }
        
        public void Awake()
        {
            if (SceneManager.GetActiveScene().name == "Game") _eventLogText = gameObject.FindChildWithName<TextMeshProUGUI>("Event Log");
            PrintFirstFourEvents();
        }

        public static void GenerateEvent(WorldEvent worldEvent)
        {
            _eventLog.Add(worldEvent.Text());
            PrintFirstFourEvents();
        }

        private static void PrintFirstFourEvents()
        {
            if (_eventLogText == null) return;
            string events = "";
            int textSize = 35;
            string[] colours = {"#ffffffff", "#ffffffdd", "#ffffffbb", "#ffffff99"};
            for (int i = 0; i < 4; ++i)
            {
                int j = _eventLog.Count - (i + 1);
                if (j >= 0) events += "<i><color=" + colours[i] + "><size=" + textSize + ">" + _eventLog[j] + "</size></color></i>";
                if (i < 3) events += "\n";
                textSize -= 5;
            }

            _eventLogText.text = events;
        }
    }
}