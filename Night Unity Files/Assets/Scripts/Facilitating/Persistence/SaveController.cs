using System.IO;
using System.Xml;
using Facilitating.Audio;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.Persistence
{
    public static class SaveController
    {
        private static readonly string GameSaveLocation = Application.persistentDataPath + "/Saves/FullSave.xml";
        private static readonly string QuickSaveLocation = Application.persistentDataPath + "/Saves/QuickSave.xml";
        private static readonly string SettingsSaveLocation = Application.persistentDataPath + "/Saves/GameSettings.xml";
        private static XmlDocument _saveDoc;


        public static void SaveGame()
        {
            Save(GameSaveLocation);
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
            Load(GameSaveLocation);
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
        private static void Load(string fileLocation)
        {
            if (!File.Exists(fileLocation)) return;
            _saveDoc = new XmlDocument();
            _saveDoc.Load(fileLocation);
            XmlNode root = _saveDoc.GetNode("BTVSave");
            WorldState.Load(root);
        }

        private static void TryCreateDirectory()
        {
            string saveDirectory = Application.persistentDataPath + "/Saves";
            if (Directory.Exists(saveDirectory)) return;
            Directory.CreateDirectory(saveDirectory);
        }

        private static void Save(string fileLocation)
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild("BTVSave");
            WorldState.Save(root);
            _saveDoc.Save(fileLocation);
        }

        public static void QuickSave()
        {
            Save(QuickSaveLocation);
        }

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsSaveLocation)) return;
            _saveDoc = new XmlDocument();
            _saveDoc.Load(SettingsSaveLocation);
            XmlNode root = _saveDoc.GetNode("Settings");
            GlobalAudioManager.Load(root);
            PauseMenuController.Load(root);
        }
        
        public static void SaveSettings()
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild("Settings");
            GlobalAudioManager.Save(root);
            PauseMenuController.Save(root);
            _saveDoc.Save(SettingsSaveLocation);
        }
    }
}