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
    public class WorldEventManager : MonoBehaviour, IPersistenceTemplate
    {
        private static TextMeshProUGUI _eventLogText;
        private static List<string> _eventLog = new List<string>();

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            string events = doc.SelectSingleNode("WorldEvents").InnerText;
            _eventLog = new List<string>(events.Split(','));
            PrintFirstFourEvents();
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            string commaSeperatedEvents = "";
            for (int i = 0; i < _eventLog.Count; ++i)
            {
                commaSeperatedEvents += _eventLog[i];
                if (i < _eventLog.Count - 1) commaSeperatedEvents += ",";
            }

            SaveController.CreateNodeAndAppend("WorldEvents", doc, commaSeperatedEvents);
            return doc;
        }

        public void Awake()
        {
            if (SceneManager.GetActiveScene().name == "Game") _eventLogText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Event Log");
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