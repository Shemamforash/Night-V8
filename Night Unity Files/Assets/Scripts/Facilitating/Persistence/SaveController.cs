using System.IO;
using System.Xml;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.Persistence
{
    public static class SaveController
    {
        private static readonly string GameSaveLocation = Application.persistentDataPath + "/Saves/NightSave.xml";
        private static readonly string SettingsSaveLocation = Application.persistentDataPath + "/Saves/GameSettings.xml";
        private static XmlDocument _saveDoc;


        public static void SaveGame()
        {
            Save(GameSaveLocation, "Game");
        }

        public static void ClearSave()
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            _saveDoc.CreateChild("Game");
            _saveDoc.Save(GameSaveLocation);
        }

//        public static bool SaveSettings()
//        {
//            return Save(SettingsSaveLocation, PersistenceType.Settings);
//        }

        public static void LoadGame()
        {
            Load(GameSaveLocation, "Game");
//            return Load(GameSaveLocation, PersistenceType.Game);
        }

        public static bool SaveExists()
        {
            return File.Exists(GameSaveLocation);
        }

        //Node creation
        public static XmlNode CreateChild(this XmlNode parent, string tagName)
        {
            XmlNode newNode = _saveDoc.CreateElement(tagName);
            parent.AppendChild(newNode);
            return newNode;
        }

        public static void CreateChild<T>(this XmlNode parent, string tagName, T value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value == null ? "" : value.ToString();
            return;
        }

        //Basic Load/Save Functions
        private static void Load(string fileLocation, string saveType)
        {
            if (!File.Exists(fileLocation)) return;
            _saveDoc = new XmlDocument();
            _saveDoc.Load(fileLocation);
            XmlNode root = _saveDoc.GetNode(saveType);
            WorldState.Load(root);
        }

        private static void TryCreateDirectory()
        {
            string saveDirectory = Application.persistentDataPath + "/Saves";
            if (Directory.Exists(saveDirectory)) return;
            Directory.CreateDirectory(saveDirectory);
            File.Create(GameSaveLocation);
            File.Create(SettingsSaveLocation);
        }
        
        private static void Save(string fileLocation, string saveType)
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild(saveType);
            WorldState.Save(root);
            _saveDoc.Save(fileLocation);
        }
    }
}