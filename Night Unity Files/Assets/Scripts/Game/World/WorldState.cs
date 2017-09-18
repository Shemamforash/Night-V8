using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Weapons;
using Game.World.Environment;
using Game.World.Time;
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
	public class WorldState : MonoBehaviour , IPersistenceTemplate
    {
	    public static int StormDistanceMax, StormDistanceActual;
		public static int DaysSpentHere;
		public static EnvironmentManager EnvironmentManager;
	    private static readonly DesolationInventory HomeInventory = new DesolationInventory("Vehicle");
	    private static GameObject _inventoryButton;

	    public void Awake()
	    {
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
		    SaveController.AddPersistenceListener(this);
		    SaveController.LoadSettings();
            SaveController.LoadGame();

		    WorldTime.Instance().DayEvent += IncrementDaysSpentHere;
		    EnvironmentManager = GameObject.Find("Canvas").GetComponent<EnvironmentManager>();
		    StormDistanceMax = 10;
		    StormDistanceActual = 10;
		    
		    _inventoryButton = Helper.FindChildWithName(GameObject.Find("Game Menu"), "Inventory");
		    _inventoryButton.GetComponent<Button>().onClick.AddListener(() => InventoryTransferManager.Instance().ShowSingleInventory(Inventory()));
	    }

	    public static GameObject GetInventoryButton()
	    {
		    return _inventoryButton;
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
        
	    public void Load(XmlNode root, PersistenceType saveData)
	    {
		    if (saveData == PersistenceType.Game)
		    {
			    HomeInventory.Resources().ForEach(r => LoadResource(r.Name(), root));
		    }
	    }
	    
	    public void Save(XmlNode root, PersistenceType saveData)
	    {
		    if (saveData == PersistenceType.Game)
		    {
			    HomeInventory.Resources().ForEach(r => SaveResource(r.Name(), root));
		    }
	    }

	    private static void LoadResource(string resourceName, XmlNode root)
	    {
		    HomeInventory.IncrementResource(resourceName, SaveController.ParseIntFromNodeAndString(root, resourceName));
	    }
	    
	    private static void SaveResource(string resourceName, XmlNode root)
	    {
		    SaveController.CreateNodeAndAppend(resourceName, root, HomeInventory.GetResourceQuantity(resourceName));
	    }
    }
}
