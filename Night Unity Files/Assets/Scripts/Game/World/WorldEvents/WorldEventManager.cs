using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.WorldEvents
{
    public class WorldEventManager : MonoBehaviour, IPersistenceTemplate
    {
        private static readonly ValueTextLink<string> _eventLogText = new ValueTextLink<string>();
        private static List<string> _eventLog = new List<string>();

        public void Awake()
        {
            _eventLogText.AddTextObject(Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Event Log"));
            PrintFirstFourEvents();
        }

        public static void GenerateEvent(WorldEvent worldEvent)
        {
            _eventLog.Add(worldEvent.Text());
            PrintFirstFourEvents();
        }

        private static void PrintFirstFourEvents()
        {
            string events = "";
            int textSize = 35;
            string[] colours = {"#ffffffff", "#ffffffdd", "#ffffffbb", "#ffffff99"};
            for (int i = 0; i < 4; ++i)
            {
                int j = _eventLog.Count - (i + 1);
                if (j >= 0)
                {
                    events += "<i><color=" + colours[i] + "><size=" + textSize + ">" + _eventLog[j] + "</size></color></i>";
                }
                if (i < 3)
                {
                    events += "\n";
                }
                textSize -= 5;
            }
            _eventLogText.Value(events);
        }

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
                if (i < _eventLog.Count - 1)
                {
                    commaSeperatedEvents += ",";
                }
            }
            SaveController.CreateNodeAndAppend("WorldEvents", doc, commaSeperatedEvents);
            return doc;
        }
    }
}