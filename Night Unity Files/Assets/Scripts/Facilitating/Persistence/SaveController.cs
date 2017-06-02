using UnityEngine;
using System.IO;

public class SaveController
{
    private static string saveLocation = Application.dataPath + "/NightSave.xml";
    public static void Load()
    {

    }

	public static bool SaveExists(){
		return File.Exists(saveLocation);
	}

    public static bool Save()
    {
        try
        {
            File.WriteAllText(saveLocation, "<Night></Night>");
            return true;
        }
        catch (IOException e)
        {
            return false;
        }
    }
}
