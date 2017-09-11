using System;
using System.Xml;
using Audio;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.World.Environment;
using Game.World.Time;
using Persistence;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World
{
	public class WorldState : MonoBehaviour , IPersistenceTemplate
    {
	    public static int StormDistanceMax, StormDistanceActual;
		public static int DaysSpentHere;
		public static int NoPreviousLocations;
		public static EnvironmentManager EnvironmentManager;
	    private static DesolationInventory _homeInventory = new DesolationInventory();

	    public void Awake()
	    {
		    SetResourceSuffix("Water", "sips");
		    SetResourceSuffix("Food", "meals");
		    SetResourceSuffix("Fuel", "dregs");
		    SetResourceSuffix("Ammo", "rounds");
		    SetResourceSuffix("Scrap", "bits");
#if UNITY_EDITOR
		    _homeInventory.IncrementResource("Ammo", 100);
#endif
		    SaveController.AddPersistenceListener(this);
		    SaveController.LoadSettings();
            SaveController.LoadGame();

		    WorldTime.Instance().DayEvent += IncrementDaysSpentHere;
		    EnvironmentManager = GameObject.Find("Canvas").GetComponent<EnvironmentManager>();
		    StormDistanceMax = 10;
		    StormDistanceActual = 10;
	    }

		private void IncrementDaysSpentHere(){
			++DaysSpentHere;
			--StormDistanceActual;
			if(StormDistanceActual == 10){
				FindNewLocation();
			}
			if(DaysSpentHere == 7){
				//TODO gameover
			}
		}

	    private void FindNewLocation()
	    {
		    StormDistanceMax--;
		    StormDistanceActual = StormDistanceMax;
		    if (StormDistanceMax == 0)
		    {
			    InitiateFinalEncounter();
		    }
	    }

	    private void InitiateFinalEncounter()
	    {
		    //TODO
	    }
	    
	    public static Inventory Inventory()
	    {
		    return _homeInventory;
	    }
        
	    private static void SetResourceSuffix(string name, string convention)
	    {
		    Text resourceText = GameObject.Find(name).GetComponent<Text>();
		    _homeInventory.GetResource(name).AddOnUpdate(f =>
		    {
			    resourceText.text = Mathf.Round(f) + " " + convention;
		    });
	    }
        
	    public void Load(XmlNode root, PersistenceType saveData)
	    {
		    if (saveData == PersistenceType.Game)
		    {
			    _homeInventory.Resources().ForEach(r => LoadResource(r.Name(), root));
		    }
	    }
	    
	    public void Save(XmlNode root, PersistenceType saveData)
	    {
		    if (saveData == PersistenceType.Game)
		    {
			    _homeInventory.Resources().ForEach(r => SaveResource(r.Name(), root));
		    }
	    }

	    private static void LoadResource(string resourceName, XmlNode root)
	    {
		    _homeInventory.IncrementResource(resourceName, SaveController.ParseIntFromNodeAndString(root, resourceName));
	    }
	    
	    private static void SaveResource(string resourceName, XmlNode root)
	    {
		    SaveController.CreateNodeAndAppend(resourceName, root, _homeInventory.GetResourceQuantity(resourceName));
	    }
    }
}
