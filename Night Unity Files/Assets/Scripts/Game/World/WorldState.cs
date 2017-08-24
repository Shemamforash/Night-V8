using Facilitating.MenuNavigation;
using UnityEngine;
using World;

namespace Game.World
{
	public class WorldState : MonoBehaviour
    {
	    public static float StormDistanceMax, StormDistanceActual;
		public static int DaysSpentHere;
		public static int NoPreviousLocations;
		private readonly TimeListener _timeListener = new TimeListener();
		public static GameMenuNavigator MenuNavigator;
		public static EnvironmentManager EnvironmentManager;

	    public void Awake()
	    {
		    MenuNavigator = Camera.main.GetComponent<GameMenuNavigator>();
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
			_timeListener.OnDay(IncrementDaysSpentHere);
		}
    }
}
