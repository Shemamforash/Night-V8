using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Facilitating.Persistence
{
    public static class SaveController
    {
        private static readonly string GameSaveLocation = Application.dataPath + "/Saves/NightSave.xml";
        private static readonly string SettingsSaveLocation = Application.dataPath + "/Saves/GameSettings.xml";
        private static XmlDocument _saveDoc;


        public static void SaveGame()
        {
            Save(GameSaveLocation, "Game");
        }

        public static void ClearSave()
        {
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

        public static XmlNode CreateChild<T>(this XmlNode parent, string tagName, T value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value == null ? "" : value.ToString();
            return newNode;
        }

        //Basic Load/Save Functions
        private static bool Load(string fileLocation, string saveType)
        {
            if (!File.Exists(fileLocation)) return false;
            _saveDoc = new XmlDocument();
            _saveDoc.Load(fileLocation);
            XmlNode root = _saveDoc.GetNode(saveType);
            WorldState.Load(root);
            return true;

        }

        private static bool Save(string fileLocation, string saveType)
        {
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild(saveType);
            WorldState.Save(root);
            _saveDoc.Save(fileLocation);
            return true;
        }
    }
}