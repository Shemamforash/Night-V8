using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Exploration.WorldEvents
{
    public class WorldEventManager : MonoBehaviour
    {
        private static TextMeshProUGUI _eventLogText;
        private static string _eventLog = "";

        public static void Load(XmlNode doc)
        {
            UpdateEvent(doc.StringFromNode("WorldEvent"));
        }

        public static XmlNode Save(XmlNode doc)
        {
            doc.CreateChild("WorldEvent", _eventLog);
            return doc;
        }

        public void Awake()
        {
            if (SceneManager.GetActiveScene().name == "Game") _eventLogText = gameObject.FindChildWithName<TextMeshProUGUI>("Event Log");
            UpdateEvent(_eventLog);
        }

        public static void GenerateEvent(WorldEvent worldEvent)
        {
            UpdateEvent(worldEvent.Text());
        }

        private static void UpdateEvent(string eventLog)
        {
            _eventLog = eventLog;
            if (_eventLogText == null) return; 
            _eventLogText.text = _eventLog;
        }

        public static void Clear()
        {
            UpdateEvent("");
        }
    }
}