using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Facilitating.Persistence;
using SamsHelper.Persistence;

namespace Persistence
{
    public class SaveController
    {
        private static readonly string GameSaveLocation = Application.dataPath + "/NightSave.xml";
        private static readonly string SettingsSaveLocation = Application.dataPath + "/GameSettings.xml";
        private static readonly List<PersistenceListener> PersistenceListeners = new List<PersistenceListener>();
        private static bool _loaded;
        private static XmlDocument _saveDoc;


        public static bool LoadGameFromFile()
        {
            try
            {
                _saveDoc = new XmlDocument();
                _saveDoc.Load(GameSaveLocation);
                GameData.SetDifficultyFromString(_saveDoc.SelectSingleNode("/SaveData/SessionSettings/Difficulty").InnerText);
                GameData.PermadeathOn = _saveDoc.SelectSingleNode("/SaveData/SessionSettings/Permadeath").InnerText.ToLower() == "true";
                GameData.StoredFood = float.Parse(_saveDoc.SelectSingleNode("/SaveData/Home/StoredFood").InnerText);
                GameData.StoredWater = float.Parse(_saveDoc.SelectSingleNode("/SaveData/Home/StoredWater").InnerText);
                GameData.StoredFuel = float.Parse(_saveDoc.SelectSingleNode("/SaveData/Home/StoredFuel").InnerText);
                NotifyListenersLoad();
                _loaded = true;
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public static bool SaveGameToFile()
        {
            try
            {
                NotifyListenersSave();

                _saveDoc = new XmlDocument();
                XmlNode root = CreateNodeAndAppend("SaveData", _saveDoc);

                XmlNode gameSettings = CreateNodeAndAppend("SessionSettings", root);
                CreateNodeAndAppend("Difficulty", gameSettings, GameData.DifficultySetting.ToString());
                CreateNodeAndAppend("Permadeath", gameSettings, GameData.PermadeathOn.ToString());
                
                XmlNode homeData = CreateNodeAndAppend("Home", root);
                CreateNodeAndAppend("StoredFood", homeData, GameData.StoredFood.ToString());
                CreateNodeAndAppend("StoredWater", homeData, GameData.StoredWater.ToString());
                CreateNodeAndAppend("StoredFuel", homeData, GameData.StoredFuel.ToString());
                
                _saveDoc.Save(GameSaveLocation);
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        private static void NotifyListenersLoad()
        {
            foreach (PersistenceListener listener in PersistenceListeners)
            {
                listener.Load();
            }
        }

        private static void NotifyListenersSave()
        {
            foreach (PersistenceListener listener in PersistenceListeners)
            {
                listener.Save();
            }
        }

        public static void Register(PersistenceListener pl)
        {
            PersistenceListeners.Add(pl);
            if (_loaded)
            {
                pl.Load();
            }
        }

        public static bool SaveExists()
        {
            return File.Exists(GameSaveLocation);
        }

        public static bool SaveSettings()
        {
            try
            {
                _saveDoc = new XmlDocument();
                XmlNode root = CreateNodeAndAppend("SettingsData", _saveDoc);
                CreateNodeAndAppend("MasterVolume", root, GameData.MasterVolume.ToString());
                CreateNodeAndAppend("MusicVolume", root, GameData.MusicVolume.ToString());
                CreateNodeAndAppend("EffectsVolume", root, GameData.EffectsVolume.ToString());
                _saveDoc.Save(SettingsSaveLocation);
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public static bool LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsSaveLocation))
                {
                    _saveDoc = new XmlDocument();
                    _saveDoc.Load(SettingsSaveLocation);
                    GameData.MasterVolume = float.Parse(_saveDoc.SelectSingleNode("/SettingsData/MasterVolume").InnerText);
                    GameData.MusicVolume = float.Parse(_saveDoc.SelectSingleNode("/SettingsData/MusicVolume").InnerText);
                    GameData.EffectsVolume = float.Parse(_saveDoc.SelectSingleNode("/SettingsData/EffectsVolume").InnerText);
                }
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }
        
        private static XmlNode CreateNodeAndAppend(string tagName, XmlNode parent)
        {
            XmlNode newNode = _saveDoc.CreateElement(tagName);
            parent.AppendChild(newNode);
            return newNode;
        }

        private static XmlNode CreateNodeAndAppend(string tagName, XmlNode parent, string text)
        {
            XmlNode newNode = CreateNodeAndAppend(tagName, parent);
            newNode.InnerText = text;
            return newNode;
        }
    }
}