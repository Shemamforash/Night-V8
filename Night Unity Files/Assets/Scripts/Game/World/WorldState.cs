using System;
using Audio;
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
	public class WorldState : MonoBehaviour
    {
	    public static int StormDistanceMax, StormDistanceActual;
		public static int DaysSpentHere;
		public static int NoPreviousLocations;
		public static EnvironmentManager EnvironmentManager;
	    private static PersistenceListener _persistenceListener;
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
		    _persistenceListener = new PersistenceListener(Load, Save, "Home");
		    
		    SaveController.LoadSettings();
            SaveController.LoadGameFromFile();
            Camera.main.GetComponent<GlobalAudioManager>().Initialise();

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
	    
		public WorldState(){
			WorldTime.Instance().DayEvent += IncrementDaysSpentHere;
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
			    Debug.Log("banana");
			    resourceText.text = Mathf.Round(f) + " " + convention;
		    });
	    }
        
	    public static void Load()
	    {
		    _homeInventory.IncrementResource("Water", GameData.StoredWater);
		    _homeInventory.IncrementResource("Food", GameData.StoredFood);
		    _homeInventory.IncrementResource("Fuel", GameData.StoredFuel);
	    }

	    public static void Save()
	    {
		    GameData.StoredWater = _homeInventory.GetResourceQuantity("Water");
		    GameData.StoredFood = _homeInventory.GetResourceQuantity("Food");
		    GameData.StoredFuel = _homeInventory.GetResourceQuantity("Fuel");
	    }
    }
}
