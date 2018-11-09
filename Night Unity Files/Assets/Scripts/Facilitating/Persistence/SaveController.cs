using System.IO;
using System.Xml;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.Persistence
{
    public static class SaveController
    {
        private static readonly string GameSaveLocation = Application.persistentDataPath + "/Saves/FullSave.xml";
        private static readonly string SettingsSaveLocation = Application.persistentDataPath + "/Saves/GameSettings.xml";
        private static XmlDocument _saveDoc;
        public static Travel ResumeInCombat;


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

        private static string GetMostRecentSaveLocation()
        {
            bool isPermanentSave = File.Exists(GameSaveLocation);
            return !isPermanentSave ? null : GameSaveLocation;
        }

        public static Travel LoadGame()
        {
            string saveLocation = GetMostRecentSaveLocation();
            if (saveLocation == null) return null;
            Debug.Log(saveLocation);
            _saveDoc = new XmlDocument();
            _saveDoc.Load(saveLocation);
            XmlNode root = _saveDoc.GetNode("BTVSave");
            WorldState.Load(root);
            if (ResumeInCombat == null) return null;
            ResumeInCombat.Enter();
            ResumeInCombat = null;
            return ResumeInCombat;
        }

        public static bool SaveExists()
        {
            return File.Exists(GameSaveLocation);
        }

        //Node creation
        public static XmlNode CreateChild(this XmlNode parent, string tagName)
        {
            try
            {
                XmlNode newNode = _saveDoc.CreateElement(tagName);
                parent.AppendChild(newNode);
                return newNode;
            }
            catch (XmlException e)
            {
                Debug.Log(tagName);
                throw e;
            }
        }

        public static void CreateChild<T>(this XmlNode parent, string tagName, T value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value == null ? "" : value.ToString();
            return;
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

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsSaveLocation)) return;
            _saveDoc = new XmlDocument();
            _saveDoc.Load(SettingsSaveLocation);
            XmlNode root = _saveDoc.GetNode("Settings");
            VolumeController.Load(root);
            FullScreenController.Load(root);
            CameraLock.Load(root);
        }

        public static void SaveSettings()
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild("Settings");
            VolumeController.Save(root);
            FullScreenController.Save(root);
            CameraLock.Save(root);
            _saveDoc.Save(SettingsSaveLocation);
        }
    }
}