using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SamsHelper.Persistence;
using UnityEngine;

namespace Facilitating.Persistence
{
    public static class SaveController
    {
        private static readonly string GameSaveLocation = Application.dataPath + "/NightSave.xml";
        private static readonly string SettingsSaveLocation = Application.dataPath + "/GameSettings.xml";
        private static XmlDocument _saveDoc;
        private static readonly List<Action<XmlNode, PersistenceType>> OnLoad = new List<Action<XmlNode, PersistenceType>>();
        private static readonly List<Action<XmlNode, PersistenceType>> OnSave = new List<Action<XmlNode, PersistenceType>>();

        public static void AddPersistenceListener(IPersistenceTemplate persistentObject)
        {
            OnLoad.Add(persistentObject.Load);
            OnSave.Add((doc, saveType) => persistentObject.Save(doc, saveType));
        }

        public static void BroadcastLoad(XmlNode root, PersistenceType persistenceType)
        {
            OnLoad.ForEach(a => a(root, persistenceType));
        }

        public static void BroadcastSave(XmlNode root, PersistenceType persistenceType)
        {
            OnSave.ForEach(a => a(root, persistenceType));
        }

        public static bool SaveGame()
        {
            return Save(GameSaveLocation, PersistenceType.Game);
        }

        public static bool SaveSettings()
        {
            return Save(SettingsSaveLocation, PersistenceType.Settings);
        }
        
        public static bool LoadGame()
        {
            return Load(GameSaveLocation, PersistenceType.Game);
        }
        
        public static bool LoadSettings()
        {
            return Load(SettingsSaveLocation, PersistenceType.Settings);
        }
        
        public static bool SaveExists()
        {
            return File.Exists(GameSaveLocation);
        }

        //Node creation
        public static XmlNode CreateNodeAndAppend(string tagName, XmlNode parent)
        {
            XmlNode newNode = _saveDoc.CreateElement(tagName);
            parent.AppendChild(newNode);
            return newNode;
        }
        
        public static XmlNode CreateNodeAndAppend<T>(string tagName, XmlNode parent, T value)
        {
            XmlNode newNode = CreateNodeAndAppend(tagName, parent);
            newNode.InnerText = value.ToString();
            return newNode;
        }

        //Float parsing
        public static float ParseFloatFromNodeAndString(XmlNode root, string str)
        {
            XmlNode node = root.SelectSingleNode(str);
            return ParseFloatFromNode(node);
        }
        
        public static float ParseFloatFromNode(XmlNode node)
        {
            if (node != null) return float.Parse(node.InnerText);
            throw new Exception("Could not parse float from node");
        }
        
        //Int Parsing
        public static int ParseIntFromNodeAndString(XmlNode root, string str)
        {
            XmlNode node = root.SelectSingleNode(str);
            return ParseIntFromSubNode(node);
        }
        
        public static int ParseIntFromSubNode(XmlNode node)
        {
            if (node != null) return int.Parse(node.InnerText);
            throw new Exception("Could not parse int from node");
        }
        
        //Bool parsing
        public static bool ParseBoolFromSubNode(XmlNode n, string boolName)
        {
            XmlNode selectSingleNode = n.SelectSingleNode(boolName);
            if (selectSingleNode != null) return selectSingleNode.InnerText == "True";
            throw new Exception("Could not parse float from node");
        }
        
        //Basic Load/Save Functions
        private static bool Load(string fileLocation, PersistenceType saveType)
        {
            try
            {
                if (File.Exists(fileLocation))
                {
                    _saveDoc = new XmlDocument();
                    _saveDoc.Load(fileLocation);
                    XmlNode root = _saveDoc.SelectSingleNode(saveType.ToString());
                    BroadcastLoad(root, saveType);
                    return true;
                }
                return false;
            }
            catch (IOException e)
            {
                return false;
            }
        }
        
        private static bool Save(string fileLocation, PersistenceType saveType)
        {
            try
            {
                _saveDoc = new XmlDocument();
                XmlNode root = CreateNodeAndAppend(saveType.ToString(), _saveDoc);
                BroadcastSave(root, saveType);
                _saveDoc.Save(fileLocation);
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }
    }
}