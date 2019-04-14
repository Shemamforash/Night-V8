using System.Globalization;
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
        private static readonly string SaveDirectory = Application.persistentDataPath + "/Saves";
        private static readonly string ManualSaveLocation = SaveDirectory + "/ManualSave.xml";
        private static readonly string AutoSaveLocation = SaveDirectory + "/AutoSave.xml";
        private static readonly string SettingsSaveLocation = SaveDirectory + "/GameSettings.xml";
        private static XmlDocument _saveDoc;

        public static void ManualSave() => SaveGame(ManualSaveLocation);

        public static void AutoSave() => SaveGame(AutoSaveLocation);

        private static void SaveGame(string directory)
        {
            TryCreateDirectory();
            if (File.Exists(directory)) File.Delete(directory);
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild("BTVSave");
            WorldState.Save(root);
            _saveDoc.Save(directory);
            Debug.Log(directory);
        }

        public static void ClearSave()
        {
            TryCreateDirectory();
            if (File.Exists(ManualSaveLocation)) File.Delete(ManualSaveLocation);
            if (File.Exists(AutoSaveLocation)) File.Delete(AutoSaveLocation);
        }

//        public static bool SaveSettings()
//        {
//            return Save(SettingsSaveLocation, PersistenceType.Settings);
//        }

        public static Save LoadAutoSave() => new Save(AutoSaveLocation);

        public static Save LoadManualSave() => new Save(ManualSaveLocation);

        public static bool SaveExists()
        {
            return File.Exists(ManualSaveLocation);
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

        public static void CreateChild(this XmlNode parent, string tagName, float value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
        }

        public static void CreateChild(this XmlNode parent, string tagName, int value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
        }

        public static void CreateChild(this XmlNode parent, string tagName, bool value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
        }

        public static void CreateChild(this XmlNode parent, string tagName, string value)
        {
            XmlNode newNode = parent.CreateChild(tagName);
            newNode.InnerText = value;
        }

        private static void TryCreateDirectory()
        {
            if (Directory.Exists(SaveDirectory)) return;
            Directory.CreateDirectory(SaveDirectory);
        }

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsSaveLocation))
            {
                VolumeController.SetToDefaultVolume();
                return;
            }

            _saveDoc = new XmlDocument();
            _saveDoc.Load(SettingsSaveLocation);
            XmlNode root = _saveDoc.GetNode("Settings");
            VolumeController.Load(root);
        }

        public static void SaveSettings()
        {
            TryCreateDirectory();
            _saveDoc = new XmlDocument();
            XmlNode root = _saveDoc.CreateChild("Settings");
            VolumeController.Save(root);
            _saveDoc.Save(SettingsSaveLocation);
        }
    }
}