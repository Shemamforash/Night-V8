using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Facilitating.Persistence;

namespace Persistence
{
    public class SaveController
    {
        private static string gameSaveLocation = Application.dataPath + "/NightSave.xml";
        private static string settingsSaveLocation = Application.dataPath + "/GameSettings.xml";
        private static List<PersistenceListener> persistenceListeners = new List<PersistenceListener>();
        private static bool loaded = false;

        public static bool LoadGameFromFile()
        {
            try
            {
                saveDoc = new XmlDocument();
                saveDoc.Load(gameSaveLocation);
                GameData.SetDifficultyFromString(saveDoc.SelectSingleNode("/SaveData/SessionSettings/Difficulty").InnerText);
                GameData.PermadeathOn = saveDoc.SelectSingleNode("/SaveData/SessionSettings/Permadeath").InnerText.ToLower() == "true";
                GameData.StoredFood = float.Parse(saveDoc.SelectSingleNode("/SaveData/Home/StoredFood").InnerText);
                GameData.StoredWater = float.Parse(saveDoc.SelectSingleNode("/SaveData/Home/StoredWater").InnerText);
                GameData.StoredFuel = float.Parse(saveDoc.SelectSingleNode("/SaveData/Home/StoredFuel").InnerText);
                NotifyListenersLoad();
                loaded = true;
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

                saveDoc = new XmlDocument();
                XmlNode root = CreateNodeAndAppend("SaveData", saveDoc);

                XmlNode gameSettings = CreateNodeAndAppend("SessionSettings", root);
                CreateNodeAndAppend("Difficulty", gameSettings, GameData.DifficultySetting.ToString());
                CreateNodeAndAppend("Permadeath", gameSettings, GameData.PermadeathOn.ToString());
                
                XmlNode homeData = CreateNodeAndAppend("Home", root);
                CreateNodeAndAppend("StoredFood", homeData, GameData.StoredFood.ToString());
                CreateNodeAndAppend("StoredWater", homeData, GameData.StoredWater.ToString());
                CreateNodeAndAppend("StoredFuel", homeData, GameData.StoredFuel.ToString());
                
                saveDoc.Save(gameSaveLocation);
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        private static void NotifyListenersLoad()
        {
            foreach (PersistenceListener listener in persistenceListeners)
            {
                listener.Load();
            }
        }

        private static void NotifyListenersSave()
        {
            foreach (PersistenceListener listener in persistenceListeners)
            {
                listener.Save();
            }
        }

        public static void Register(PersistenceListener pl)
        {
            persistenceListeners.Add(pl);
            if (loaded)
            {
                pl.Load();
            }
        }

        public static bool SaveExists()
        {
            return File.Exists(gameSaveLocation);
        }

        private static XmlDocument saveDoc;

        public static bool SaveSettings()
        {
            try
            {
                saveDoc = new XmlDocument();
                XmlNode root = CreateNodeAndAppend("SettingsData", saveDoc);
                CreateNodeAndAppend("MasterVolume", root, GameData.MasterVolume.ToString());
                CreateNodeAndAppend("MusicVolume", root, GameData.MusicVolume.ToString());
                CreateNodeAndAppend("EffectsVolume", root, GameData.EffectsVolume.ToString());
                saveDoc.Save(settingsSaveLocation);
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
                if (File.Exists(settingsSaveLocation))
                {
                    saveDoc = new XmlDocument();
                    saveDoc.Load(settingsSaveLocation);
                    GameData.MasterVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/MasterVolume").InnerText);
                    GameData.MusicVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/MusicVolume").InnerText);
                    GameData.EffectsVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/EffectsVolume").InnerText);
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
            XmlNode newNode = saveDoc.CreateElement(tagName);
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