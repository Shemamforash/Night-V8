using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Weapons;
using Game.World.Environment;
using Game.World.Time;
using Game.World.Weather;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World
{
	public class WorldState : MonoBehaviour
    {
	    public static int StormDistanceMax, StormDistanceActual;
		public static int DaysSpentHere;
	    private static GameObject _inventoryButton;
	    public static EnvironmentManager EnvironmentManager = new EnvironmentManager();
	    public static DesolationCharacterManager HomeInventory = new DesolationCharacterManager();
	    private WeatherManager Weather = new WeatherManager();

	    public void Awake()
	    {
		    HomeInventory.Start();
		    EnvironmentManager.Start();
		    Weather.Start();
		    SetResourceSuffix("Water", "sips");
		    SetResourceSuffix("Food", "meals");
		    SetResourceSuffix("Fuel", "dregs");
		    SetResourceSuffix("Ammo", "rounds");
		    SetResourceSuffix("Scrap", "bits");
#if UNITY_EDITOR
		    HomeInventory.IncrementResource("Ammo", 100);
		    for(int i = 0; i < 10; ++i)
		    {
			    HomeInventory.AddItem(WeaponGenerator.GenerateWeapon());
		    }
#endif
		    SaveController.LoadSettings();
            SaveController.LoadGame();

		    WorldTime.Instance().DayEvent += IncrementDaysSpentHere;
		    StormDistanceMax = 10;
		    StormDistanceActual = 10;
		    
		    _inventoryButton = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Inventory");
		    _inventoryButton.GetComponent<Button>().onClick.AddListener(() => InventoryTransferManager.Instance().ShowSingleInventory(Home()));
	    }

	    public static GameObject GetInventoryButton() => _inventoryButton;

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
	    
	    public static Inventory Home()
	    {
		    return HomeInventory;
	    }
        
	    private static void SetResourceSuffix(string name, string convention)
	    {
		    TextMeshProUGUI resourceText = GameObject.Find(name).GetComponent<TextMeshProUGUI>();
		    HomeInventory.GetResource(name).AddOnUpdate(f =>
		    {
			    resourceText.text = "<sprite name=\"" + name + "\">" + Mathf.Round(f) + " " + convention;
		    });
	    }
    }
}
