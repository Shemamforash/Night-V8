﻿using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.WorldEvents
{
    public class WorldEventManager : MonoBehaviour, IPersistenceTemplate
    {
        private static Text _eventLogText;
        private static List<string> _eventLog = new List<string>();

        public void Awake()
        {
            _eventLogText = Helper.FindChildWithName<Text>(gameObject, "Event Log");
            PrintFirstFourEvents();
        }

        public static void GenerateEvent(WorldEvent worldEvent)
        {
            _eventLog.Add(worldEvent.EventText());
            PrintFirstFourEvents();
        }

        private static void PrintFirstFourEvents()
        {
            string events = "";
            int textSize = 40;
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
            _eventLogText.text = events;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            string events = doc.SelectSingleNode("WorldEvents").InnerText;
            _eventLog = new List<string>(events.Split(','));
            PrintFirstFourEvents();
        }

        public void Save(XmlNode doc, PersistenceType saveType)
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
        }
    }
}