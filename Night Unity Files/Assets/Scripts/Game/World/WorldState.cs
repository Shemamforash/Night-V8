using UnityEngine;

namespace World
{
	using Menus;

    public class WorldState : MonoBehaviour
    {
		public static float currentDanger = 0f;
		public static int daysSpentHere = 0;
		public static int noPreviousLocations = 0;
		private TimeListener timeListener = new TimeListener();
		public static GameMenuNavigator menuNavigator;
		public static EnvironmentManager environmentManager;

		private void IncrementDaysSpentHere(){
			++daysSpentHere;
			currentDanger += 0.5f;
			if(currentDanger > 8){
				currentDanger = 8;
			}
			if(daysSpentHere == 7){
				//TODO gameover
			}
		}

		private void ResetDaysSpentHere(){
			daysSpentHere = 0;
			currentDanger -= 1;
			if(currentDanger < 0){
				currentDanger = 0;
			}
			++noPreviousLocations;
		}

		public WorldState(){
			timeListener.OnDay(IncrementDaysSpentHere);
			timeListener.OnTravel(ResetDaysSpentHere);
		}
    }
}
