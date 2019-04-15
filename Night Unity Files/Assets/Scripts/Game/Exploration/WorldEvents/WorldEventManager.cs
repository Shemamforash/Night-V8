using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
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

        public static void Save(XmlNode doc)
        {
            doc.CreateChild("WorldEvent", _eventLog);
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
            _eventLogText.text = "<i>" + _eventLog + "</i>";
        }

        public static void Clear()
        {
            UpdateEvent("");
        }

        private static readonly string[] _saveStrings =
        {
            "I should remember this place if I ever want to return",
            "I can retrace my steps if I record today's journey.",
            "It is easy to lose track of time out here"
        };

        public static void SuggestSave()
        {
            if (CharacterManager.Wanderer == null) return;
            GenerateEvent(new CharacterMessage(_saveStrings.RandomElement(), CharacterManager.Wanderer));
        }
    }
}