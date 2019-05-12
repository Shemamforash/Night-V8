using System.Globalization;
using System.IO;
using System.Xml;
using Extensions;
using Game.Global;
using UnityEngine;

namespace Facilitating.Persistence
{
	public static class SaveController
	{
		private static readonly string      SaveDirectory        = Application.persistentDataPath + "/Saves";
		private static readonly string      ManualSaveLocation   = SaveDirectory                  + "/ManualSave.xml";
		private static readonly string      AutoSaveLocation     = SaveDirectory                  + "/AutoSave.xml";
		private static readonly string      SettingsSaveLocation = SaveDirectory                  + "/GameSettings.xml";
		private static          XmlDocument _saveDoc;

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

		public static Save LoadAutoSave() => new Save(AutoSaveLocation);

		public static Save LoadManualSave() => new Save(ManualSaveLocation);

		public static bool SaveExists() => File.Exists(ManualSaveLocation);

		//Node creation
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
			XmlNode root = _saveDoc.GetChild("Settings");
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