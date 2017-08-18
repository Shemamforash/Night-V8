using Facilitating.MenuNavigation;
using UnityEngine;
using World;

namespace Game.World
{
	public class WorldState : MonoBehaviour
    {
		public static float CurrentDanger;
		public static int DaysSpentHere;
		public static int NoPreviousLocations;
		private readonly TimeListener _timeListener = new TimeListener();
		public static GameMenuNavigator MenuNavigator;
		public static EnvironmentManager EnvironmentManager;

	    public void Awake()
	    {
		    MenuNavigator = Camera.main.GetComponent<GameMenuNavigator>();
		    EnvironmentManager = GameObject.Find("Canvas").GetComponent<EnvironmentManager>();
	    }

		private void IncrementDaysSpentHere(){
			++DaysSpentHere;
			CurrentDanger += 0.5f;
			if(CurrentDanger > 8){
				CurrentDanger = 8;
			}
			if(DaysSpentHere == 7){
				//TODO gameover
			}
		}

		private void ResetDaysSpentHere(){
			DaysSpentHere = 0;
			CurrentDanger -= 1;
			if(CurrentDanger < 0){
				CurrentDanger = 0;
			}
			++NoPreviousLocations;
		}

		public WorldState(){
			_timeListener.OnDay(IncrementDaysSpentHere);
			_timeListener.OnTravel(ResetDaysSpentHere);
		}
    }
}
