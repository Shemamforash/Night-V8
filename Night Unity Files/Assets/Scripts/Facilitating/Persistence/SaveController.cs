using UnityEngine;
using System.IO;
using System.Xml;

public class SaveController
{
    private static string gameSaveLocation = Application.dataPath + "/NightSave.xml";
    private static string settingsSaveLocation = Application.dataPath + "/GameSettings.xml";
    public static bool LoadGame()
    {
        try
        {
            saveDoc = new XmlDocument();
            saveDoc.Load(gameSaveLocation);
            Settings.SetDifficultyFromString(saveDoc.SelectSingleNode("/SaveData/SessionSettings/Difficulty").InnerText);
            Settings.permadeathOn = saveDoc.SelectSingleNode("/SaveData/SessionSettings/Permadeath").InnerText.ToLower() == "true";
            return true;
        }
        catch (IOException e)
        {
            return false;
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
            CreateNodeAndAppend("MasterVolume", root, Settings.masterVolume.ToString());
            CreateNodeAndAppend("MusicVolume", root, Settings.musicVolume.ToString());
            CreateNodeAndAppend("EffectsVolume", root, Settings.effectsVolume.ToString());
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
                Settings.masterVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/MasterVolume").InnerText);
                Settings.musicVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/MusicVolume").InnerText);
                Settings.effectsVolume = float.Parse(saveDoc.SelectSingleNode("/SettingsData/EffectsVolume").InnerText);
            }
            return true;
        }
        catch (IOException e)
        {
            return false;
        }
    }

    public static bool SaveGame()
    {
        try
        {
            saveDoc = new XmlDocument();
            XmlNode root = CreateNodeAndAppend("SaveData", saveDoc);

            XmlNode gameSettings = CreateNodeAndAppend("SessionSettings", root);
            CreateNodeAndAppend("Difficulty", gameSettings, Settings.difficultySetting.ToString());
            CreateNodeAndAppend("Permadeath", gameSettings, Settings.permadeathOn.ToString());

            saveDoc.Save(gameSaveLocation);
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
